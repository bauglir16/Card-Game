using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameLogic : NetworkBehaviour
{
	public CardData cardPrefab;
	public PlayerScript playerPrefab;
	List<CardIds> m_InitialDeck;
	public List<CardData> m_PlayedCards;
	Stack<CardIds> m_ShuffledDeck;
	public int playerCount;
	public float playerRadius;
	int winnerIndex, loserIndex;
	public List<PlayerScript> m_Players = new List<PlayerScript>();
	Transform cardSpawnPos, playedPos;
	Vector3 initialPlayedPos;
	GameObject m_PhysicalDeck;
	float m_cardThickness;
	public int m_PlayerIndex;
	int startingIndex;
	enum Stages { Null, setup, exchanging, choosing, givingCards, playing }
	public int RankOnTop;
	public int countRankOnTop = 0;
	public int PowerOnTop;
	bool playerIsOut;
	public NetworkVariable<int> Done;
	public NetworkVariable<bool> dataAvailable;
	public int localIndex;
	public NetworkVariable<int> netRemotePlayerIndex;
	public NetworkList<int> netShuffledDeck;
	public NetworkList<int> netClickedObjects;
	NetworkVariable<Stages> netStage = new NetworkVariable<Stages>();
	bool playAgain;

	List<CardData> cards = new List<CardData>();
	bool again = false;
	CardData firstCard;
	private bool isWaitingForServer;
	public Button okButton;
	int roundN = 1;
	public TMP_Text RankOnTopText;
	public TMP_Text countRankOnTopText;
	public TMP_Text PowerOnTopText;
	public TMP_Text WinnerTxt;
	public GameObject indexPointer;

	CardData PrepareCard(CardIds cardId)
	{
		CardData card = Instantiate<CardData>(cardPrefab, cardSpawnPos.position, cardSpawnPos.rotation, transform);
		card.Set(cardId);
		transform.GetChild(0).transform.position -= new Vector3(0, m_cardThickness, 0);
		return card;
	}

	void Give3Cards(int index, CardIds[] cards, PlayerScript player)
	{
		CardData[] newCards = new CardData[3];
		for (int i = 0; i < 3; i++)
		{
			newCards[i] = PrepareCard(cards[i]);
		}
		switch (index)
		{
			case 0:
				player.GiveFirst3Cards(newCards[0], newCards[1], newCards[2]);
				break;
			case 1:
				player.GiveSecond3Cards(newCards[0], newCards[1], newCards[2]);
				break;
			case 2:
				player.GiveThird3Cards(newCards[0], newCards[1], newCards[2]);
				break;
			default:
				break;
		}

		m_PhysicalDeck.transform.localPosition -= new Vector3(0, m_cardThickness * 3, 0);
	}

	void GiveCard(CardIds card, PlayerScript player)
	{
		CardData newCard = PrepareCard(card);
		player.GiveCardAtHand(newCard);
		m_PhysicalDeck.transform.position -= new Vector3(0, m_cardThickness, 0);

		if (m_ShuffledDeck.Count == 0 && !m_Players[0].deckFinished)
		{
			for (int i = 0; i < m_Players.Count; i++)
			{
				m_Players[i].deckFinished = true;
			}

			Debug.Log("Deck finished");
		}
	}

	bool PlayCards(PlayerScript player)
	{
		bool playAgain = false;

		if (player.clickedObjects.Count == 0)
		{
			if (localIndex != m_PlayerIndex) Debug.Log($"Taking all cards {m_PlayerIndex} | {Time.frameCount}");
			player.TakeAllCards(m_PlayedCards);
			RankOnTop = 0;
			PowerOnTop = 0;
			countRankOnTop = 0;
			m_PlayedCards.Clear();
			playedPos.position = initialPlayedPos;
			return playAgain;
		}

		{
			if (m_PlayedCards.Count > 0 && m_PlayedCards[m_PlayedCards.Count - 1].Rank == player.clickedObjects[0].Rank)
				countRankOnTop += player.clickedObjects.Count;

			else if (player.clickedObjects[0].Rank == 10)
				playAgain = true;

			else if (player.clickedObjects[0].Rank == 2)
			{
				PowerOnTop = 0;
				RankOnTop = 2;
				countRankOnTop = player.clickedObjects.Count;
			}
			else if (player.clickedObjects[0].Rank == 4)
			{
				RankOnTop = 4;
				countRankOnTop = player.clickedObjects.Count;
			}
			else
			{
				PowerOnTop = player.clickedObjects[0].power;
				RankOnTop = player.clickedObjects[0].Rank;
				countRankOnTop = player.clickedObjects.Count;
			}

			playAgain |= countRankOnTop == 4;
		}

		for (int i = 0; i < player.clickedObjects.Count; i++)
		{
			//player.clickedObjects[i].transform.SetPositionAndRotation(playedPos.transform.position, playedPos.transform.rotation);
			//player.clickedObjects[i].transform.rotation = playedPos.transform.rotation;
			player.clickedObjects[i].SetTargetPositionAndRotation(playedPos.transform.position, playedPos.transform.rotation, true, true);
			playedPos.transform.localPosition += new Vector3(0, m_cardThickness, 0);
		}

		m_PlayedCards.AddRange(player.clickedObjects);
		RankOnTopText.SetText(RankOnTop.ToString());
		countRankOnTopText.SetText(countRankOnTop.ToString());
		PowerOnTopText.SetText(PowerOnTop.ToString());

		Debug.Log($"Clicked cleared {Time.frameCount}");
		player.clickedObjects.Clear();
		return playAgain;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public IEnumerator Setup()
	{
		localIndex = (int)NetworkManager.Singleton.LocalClientId;
		Debug.Log($"Local index: {localIndex}");
		cardSpawnPos = transform.GetChild(0).transform.GetChild(52);
		m_PhysicalDeck = transform.GetChild(0).gameObject;
		

		initPlayers();
		initDecks();


		yield return StartCoroutine(waitForSync(Stages.setup));

		StartCoroutine(updateCoroutine());
	}

	private IEnumerator waitForSync(Stages next = Stages.Null)
	{
		if (IsHost)
		{
			while (Done.Value != m_Players.Count - 1)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			Debug.Log($"All clients finished");
			if (next != Stages.Null) 
				netStage.Value = next;
			dataAvailable.Value = false;
			Done.Value = 0;
			NotifyClientRpc();
		}
		else
		{
			Debug.Log("Client finished");
			isWaitingForServer = true;
			eventDoneServerRpc(localIndex);
			while (isWaitingForServer || Done.Value != 0)
			{
				Debug.Log("Waiting");
				yield return null;
			}
		}
	}

	private IEnumerator waitForPlaySync(Stages next = Stages.Null)
	{
		if (IsHost)
		{
			while (Done.Value != m_Players.Count - 1)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			Debug.Log($"All clients finished");
			if (next != Stages.Null)
				netStage.Value = next;
			netRemotePlayerIndex.Value = m_PlayerIndex;
			dataAvailable.Value = false;
			Done.Value = 0;
			NotifyClientRpc();
			Debug.Log($"Clients notified frame: {Time.frameCount}");
		}
		else
		{
			Debug.Log($"Client finished frame: {Time.frameCount}");
			isWaitingForServer = true;
			eventDoneServerRpc(localIndex);
			while (isWaitingForServer || Done.Value != 0)
			{
				Debug.Log("Waiting");
				yield return null;
			}
			Debug.Log($"Wait finished frame: {Time.frameCount}");
		}
	}

	private IEnumerator waitForChooseSync(Stages next = Stages.Null)
	{
		if (IsHost)
		{
			while (Done.Value != m_Players.Count - 1)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			netRemotePlayerIndex.Value++;
			netClickedObjects.Clear();
			dataAvailable.Value = false;
			Done.Value = 0;
			NotifyClientRpc();
			Debug.Log("Clients notified");
		}
		else
		{
			Debug.Log("Client finished");
			isWaitingForServer = true;
			eventDoneServerRpc();
			while (isWaitingForServer || Done.Value != 0)
			{
				Debug.Log("Waiting for server");
				yield return null;
			}
		}
	}

	private void initDecks()
	{
		m_InitialDeck = new List<CardIds>(52);
		for (int i = 1; i <= 52; i++)
		{
			m_InitialDeck.Add(CardIds.Null + i);

		}

		m_ShuffledDeck = new Stack<CardIds>(m_InitialDeck.Count);
		int randomCard;
		if (IsHost)
		{
			while (m_InitialDeck.Count > 0)
			{
				randomCard = UnityEngine.Random.Range(0, m_InitialDeck.Count);
				netShuffledDeck.Add((int)m_InitialDeck[randomCard]);
				m_InitialDeck.RemoveAt(randomCard);
			}
			Debug.Log("Net deck count: " + netShuffledDeck.Count + " | " + Time.frameCount);
		}


		m_PlayedCards = new List<CardData>();
		m_cardThickness = m_PhysicalDeck.transform.localPosition.y;
		m_cardThickness -= 1.255998f;
		m_cardThickness /= 52;
		m_PhysicalDeck.transform.position += new Vector3(0, m_cardThickness * 3, 0); m_PlayerIndex = 0;
		playedPos = transform.GetChild(1);
		initialPlayedPos = playedPos.position;
	}

	private void initPlayers()
	{
		for (int i = 0; i < playerCount; i++)
		{
			m_Players.Add(Instantiate<PlayerScript>(playerPrefab, transform));
		}
		for (int i = 0; i < m_Players.Count; i++)
		{
			float angle = Mathf.PI * 2 * i / m_Players.Count;
			float x = transform.position.x + Mathf.Cos(angle) * playerRadius;
			float y = transform.position.y + 6f;
			float z = transform.position.z + Mathf.Sin(angle) * playerRadius;
			m_Players[i].transform.position = new Vector3(x, y, z);
			m_Players[i].transform.LookAt(new Vector3(transform.position.x, m_Players[i].transform.position.y, transform.position.z));
			m_Players[i].id = i;
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void eventDoneServerRpc(int localIndex = -1)
	{
		Done.Value++;
		if (localIndex >= 0) Debug.Log("Client index: " + localIndex);
	}

	IEnumerator updateCoroutine()
	{
		//CLONE REMOTE SHUFFLED DECK
		for (int i = 0; i < netShuffledDeck.Count; i++)
		{
			m_ShuffledDeck.Push((CardIds)netShuffledDeck[i]);
		}
		m_InitialDeck.Clear();

		yield return StartCoroutine(waitForSync());
		if (IsHost)
			netShuffledDeck.Clear();

		//DEAL CARDS AND "PLAY" FIRST CARD
		prepareGame();


		if (winnerIndex != -1)
		{
			yield return StartCoroutine(waitForSync(Stages.exchanging));
			yield return StartCoroutine(exchnageCards());
		}

		yield return StartCoroutine(waitForSync(Stages.choosing));
		if (IsHost && winnerIndex != -1) 
			netClickedObjects.Clear();

		//LET LOCAL PLAYER CHOOSE STARTING CARDS
		m_Players[localIndex].PrepareChooseCards();

		while (!m_Players[localIndex].finishedChoosing)
			yield return null;
		m_Players[localIndex].okButton = null;

		yield return StartCoroutine(waitForSync(Stages.givingCards));

		while (netStage.Value < Stages.givingCards)
		{
			yield return null;
		}

		//TELL SERVER OF THE CARD LOCAL PLAYER CHOSE AND GET AND APPLY THE CHOICES OF OTHER PLAYERS
		yield return StartCoroutine(informOfChosenCards());

		if (IsHost)
		{
			netRemotePlayerIndex.Value = 0;
			netStage.Value = Stages.playing;
		}

		while (netStage.Value < Stages.playing)
		{
			yield return null;
		}

		//START PLAYING
		yield return StartCoroutine(GameLoop());
		
		loserIndex = m_Players[0].id;
		Debug.Log("Player " + loserIndex + " is the whore");
		PlayerPrefs.SetInt("loserIndex", loserIndex);
		PlayerPrefs.Save();
		NetworkManager.SceneManager.LoadScene("LobbyScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
	}

	private IEnumerator exchnageCards()
	{
		if (localIndex == winnerIndex)
		{
			List<int> selectedCards = new List<int>();
			m_Players[localIndex].chooseCardsToExchange(m_Players[localIndex]);
			while (!m_Players[localIndex].finishedChoosing)
				yield return null;
			okButton.onClick.RemoveAllListeners();
			selectedCards.AddRange(m_Players[localIndex].ClickedObjectsIdsAsInts());
			m_Players[localIndex].chooseCardsToExchange(m_Players[loserIndex]);
			while (!m_Players[localIndex].finishedChoosing)
				yield return null;
			okButton.onClick.RemoveAllListeners();
			selectedCards.AddRange(m_Players[localIndex].ClickedObjectsIdsAsInts());

			if (!IsHost)
				eventSetClickedObjectsServerRpc(selectedCards.ToArray(), localIndex);

			else
				SetNetClickedObjects(selectedCards.ToArray());
		}
		else
		{
			yield return StartCoroutine(waitForData());
		}

		List<CardData> cardsToExchange = new List<CardData>(4);
		List<Tuple<int, int>> cardsPos = new List<Tuple<int, int>>(4);
		int i = 0;
		for (; i < 2; i++)
		{
			cardsPos.Add(m_Players[winnerIndex].getDownCardPos((CardIds)netClickedObjects[i]));
			cardsToExchange.Add(m_Players[winnerIndex].getDownCard(cardsPos[i].Item1, cardsPos[i].Item2));
		}
		Debug.Log($"cardPos l: {cardsPos.Count} | cardsToExchange l: {cardsToExchange.Count}");
		for (; i < 4; i++)
		{
			cardsPos.Add(m_Players[loserIndex].getDownCardPos((CardIds)netClickedObjects[i]));
			cardsToExchange.Add(m_Players[loserIndex].getDownCard(cardsPos[i].Item1, cardsPos[i].Item2));
		}

		Debug.Log($"cardPos l: {cardsPos.Count} | cardsToExchange l: {cardsToExchange.Count}");

		for (i = 0; i < 2; i++)
		{
			m_Players[loserIndex].setDownCard(cardsToExchange[i], cardsPos[cardsPos.Count - 1 - i].Item1, cardsPos[cardsPos.Count - 1 - i].Item2);
		}
		for (; i < 4; i++)
		{
			m_Players[winnerIndex].setDownCard(cardsToExchange[i], cardsPos[cardsPos.Count - 1 - i].Item1, cardsPos[cardsPos.Count - 1 - i].Item2);
		}
	}

	private IEnumerator GameLoop()
	{
		setPlayerIndex(startingIndex);
		while (m_Players.Count > 1)
		{
			Debug.Log($"=================================ROUND {roundN}=================================");

			for (int i = 0; i < m_Players.Count; i++)
			{
				if (i == localIndex)
					continue;
				string log = "[";
				m_Players[i].cardsAtHand.ForEach(i => log += i.Id + ", ");
				log += "]";
				Debug.Log($"Cards at Hand: {log}");
			}

			m_Players[localIndex].RankOnTop = RankOnTop;
			m_Players[localIndex].PowerOnTop = PowerOnTop;
			m_Players[localIndex].countRankOnTop = countRankOnTop;
			setUIText();
			m_Players[localIndex].SetCamera();

			if (localIndex == m_PlayerIndex)
				yield return StartCoroutine(Play());
			else
				yield return StartCoroutine(FollowDecisionsOfRemotePlayer());

			yield return StartCoroutine(waitForSync());

			if (m_Players[m_PlayerIndex].finishedPlaying)
			{
				ApplyPlayerDecisions();
				PrepareNextRound();

				yield return StartCoroutine(waitForPlaySync());

				roundN++;
			}
			yield return null;
		}
	}

	private void PrepareNextRound()
	{
		if (!playAgain)
		{
			if (playerIsOut)
			{
				m_Players.RemoveAt(m_PlayerIndex);
				m_PlayerIndex -= (m_PlayerIndex == m_Players.Count) ? m_PlayerIndex + 1 : 1; //Size of list = last index before removal
				playerIsOut = false;
			}

			setPlayerIndex((m_PlayerIndex + 1) % m_Players.Count());

			if (m_Players.Count < playerCount)
				Debug.Log(m_PlayerIndex);
		}
		else
		{
			Debug.Log(m_PlayedCards[m_PlayedCards.Count - 1].Rank);

			playedPos.position = initialPlayedPos;
			for (int i = 0; i < m_PlayedCards.Count; i++)
				DestroyImmediate(m_PlayedCards[i].gameObject);

			RankOnTop = 0;
			PowerOnTop = 0;
			countRankOnTop = 0;
			setUIText();
			m_PlayedCards.Clear();
			playAgain = false;
		}

		m_Players[m_PlayerIndex].finishedPlaying = false;
	}

	private void setPlayerIndex(int v)
	{
		m_PlayerIndex = v;
		rotateIndexPointer();
	}

	private void ApplyPlayerDecisions()
	{
		if (m_Players[m_PlayerIndex].clickedObjects.Count > 0) Debug.Log(m_Players[m_PlayerIndex].clickedObjects[0].Id);

		playAgain = PlayCards(m_Players[m_PlayerIndex]);

		if (playAgain) Debug.Log($"Play again: {m_PlayerIndex}");

		for (int i = m_Players[m_PlayerIndex].cardsAtHand.Count; m_ShuffledDeck.Count > 0 && i < 3; i++)
			GiveCard(m_ShuffledDeck.Pop(), m_Players[m_PlayerIndex]);

		if (m_Players[m_PlayerIndex].Out)
		{
			if (m_Players.Count == playerCount)
			{
				winnerIndex = m_Players[m_PlayerIndex].id;
				Debug.Log("Player " + m_PlayerIndex + " is the winner");
				PlayerPrefs.SetInt("winnerIndex", m_PlayerIndex);
				StartCoroutine(ShowWinnerCoroutine(m_PlayerIndex));
			}

			playerIsOut = true;

			if (playAgain)
			{
				Debug.Log(m_PlayedCards[m_PlayedCards.Count - 1].Rank);

				playedPos.position = initialPlayedPos;
				for (int i = 0; i < m_PlayedCards.Count; i++)
					DestroyImmediate(m_PlayedCards[i].gameObject);

				m_PlayedCards.Clear();
				RankOnTop = 0;
				PowerOnTop = 0;
				countRankOnTop = 0;
				setUIText();
				playAgain = false;
			}
		}
	}

	private IEnumerator FollowDecisionsOfRemotePlayer()
	{
		Debug.Log($"Not playing frame: {Time.frameCount}");

		yield return StartCoroutine(waitForData());

		m_Players[m_PlayerIndex].okButton = okButton;
		m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
		m_Players[m_PlayerIndex].Play(false);
		for (int i = 0; i < netClickedObjects.Count; i++)
			m_Players[m_PlayerIndex].addClickedCard((CardIds)netClickedObjects[i]);
		Debug.Log("Remote player net deck size: " + netClickedObjects.Count + " | " + Time.frameCount);
		m_Players[m_PlayerIndex].okButton.onClick.Invoke();
		okButton.onClick.RemoveAllListeners();
		m_Players[m_PlayerIndex].okButton = null;
	}

	private IEnumerator Play()
	{
		Debug.Log($"Playing frame: {Time.frameCount}");

		m_Players[localIndex].okButton = okButton;
		m_Players[localIndex].okButton.onClick.RemoveAllListeners();
		m_Players[localIndex].Play();

		while (!m_Players[localIndex].finishedPlaying && !m_Players[localIndex].finishedChoosing)
			yield return null;
		okButton.onClick.RemoveAllListeners();
		m_Players[localIndex].okButton = null;
		m_Players[localIndex].finishedChoosing = false;

		int[] temp = (m_Players[localIndex].finishedPlaying) ? m_Players[localIndex].ClickedObjectsIdsAsInts() : m_Players[localIndex].CardsAtHandIdsAsInts();
		if (!IsHost)
		{
			eventSetClickedObjectsServerRpc(temp, localIndex);
		}
		else
			SetNetClickedObjects(temp);

		Debug.Log("Local player net deck size: " + netClickedObjects.Count + " | " + Time.frameCount);
	}

	private void setUIText()
	{
		RankOnTopText.SetText(RankOnTop.ToString());
		PowerOnTopText.SetText(PowerOnTop.ToString());
		countRankOnTopText.SetText(countRankOnTop.ToString());
	}

	private IEnumerator informOfChosenCards()
	{
		while (m_PlayerIndex < playerCount)
		{
			if (m_PlayerIndex == localIndex)
			{
				if (!IsHost)
					eventSetClickedObjectsServerRpc(m_Players[localIndex].CardsAtHandIdsAsInts(), localIndex);

				else
					SetNetClickedObjects(m_Players[localIndex].CardsAtHandIdsAsInts());

				Debug.Log("Local player net deck size: " + netClickedObjects.Count + " | " + Time.frameCount);
			}
			else
			{
				yield return StartCoroutine(waitForData());

				for (int i = 0; i < netClickedObjects.Count; i++)
					Debug.Log("Other player hand: " + (CardIds)netClickedObjects[i]);
				List<CardIds> list = new List<CardIds>();
				for (int i = 0; i < 3; i++)
				{
					list.Add(m_Players[m_PlayerIndex].first3Cards[i].Id);
					list.Add(m_Players[m_PlayerIndex].second3Cards[i].Id);
				}
				for (int i = 0; i < netClickedObjects.Count; i++)
					m_Players[m_PlayerIndex].addClickedCard((CardIds)netClickedObjects[i], list);
				m_Players[m_PlayerIndex].okButton = okButton;
				m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
				m_Players[m_PlayerIndex].PrepareChooseCards(false);
				m_Players[m_PlayerIndex].okButton.onClick.Invoke();
			}



			yield return StartCoroutine(waitForChooseSync());
			m_PlayerIndex++;

			while (m_PlayerIndex != netRemotePlayerIndex.Value && m_PlayerIndex < m_Players.Count)
				yield return null;
		}
		m_PlayerIndex = 0;
	}

	private void prepareGame()
	{
		CardIds[] tempCards = new CardIds[3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < m_Players.Count; j++)
			{
				tempCards[0] = m_ShuffledDeck.Pop();
				tempCards[1] = m_ShuffledDeck.Pop();
				tempCards[2] = m_ShuffledDeck.Pop();
				Give3Cards(i, tempCards, m_Players[j]);
			}
		}
		cards = new List<CardData>();
		again = false;
		firstCard = PrepareCard(m_ShuffledDeck.Pop());
		while (firstCard.Rank == 10)
		{
			cards.Add(firstCard);
			firstCard = PrepareCard(m_ShuffledDeck.Pop());
			again = true;
		}
		if (again)
		{
			cards.Reverse();
			for (int i = 0; i < cards.Count;)
			{
				m_ShuffledDeck.Push(cards[0].Id);
				cards.RemoveAt(0);
			}
		}
		firstCard.transform.SetPositionAndRotation(playedPos.position, playedPos.rotation);
		m_PlayedCards.Add(firstCard);
		RankOnTop = firstCard.Rank;
		countRankOnTop = 1;
		if (firstCard.Rank == 4 || firstCard.Rank == 2)
			PowerOnTop = 0;
		else
			PowerOnTop = firstCard.power;
		playedPos.position += new Vector3(0, m_cardThickness, 0);

		countRankOnTopText.SetText(countRankOnTop.ToString());
		RankOnTopText.SetText(RankOnTop.ToString());
		PowerOnTopText.SetText(PowerOnTop.ToString());
		m_Players[localIndex].RankOnTop = RankOnTop;
		m_Players[localIndex].PowerOnTop = PowerOnTop;
		m_Players[localIndex].countRankOnTop = countRankOnTop;
		m_Players[localIndex].playerCamera = Camera.main;
		m_Players[localIndex].okButton = okButton;
		m_Players[localIndex].okButton.onClick.RemoveAllListeners();
	}

	private IEnumerator waitForData()
	{
		while (!dataAvailable.Value)
		{
			Debug.Log($"No data yet | {Time.frameCount}");
			yield return null;
		}
		Debug.Log($"Data added | {Time.frameCount}");
	}

	private void rotateIndexPointer()
	{
		Vector3 direction = new Vector3(m_Players[m_PlayerIndex].transform.position.x, indexPointer.transform.position.y, m_Players[m_PlayerIndex].transform.position.z);
		indexPointer.transform.LookAt(direction);
	}

	private IEnumerator ShowWinnerCoroutine(int m_PlayerIndex)
	{
		WinnerTxt.SetText("Player " + m_PlayerIndex + " is the winner");
		yield return new WaitForSeconds(2f);
		WinnerTxt.SetText("");
		WinnerTxt.gameObject.SetActive(false);
	}

	void SetNetClickedObjects(int[] clickedObjects)
	{
		netClickedObjects.Clear();
		for (int i = 0; i < clickedObjects.Length; i++)
		{
			netClickedObjects.Add(clickedObjects[i]);
		}
		Debug.Log($"Data added | {Time.frameCount}");
		//NotifyClientRpc(playerId);
		dataAvailable.Value = true;
	}

	[ServerRpc(RequireOwnership = false)]
	public void eventSetClickedObjectsServerRpc(int[] clickedObjects, int playerId)
	{
		netClickedObjects.Clear();
		for (int i = 0; i < clickedObjects.Length; i++)
		{
			netClickedObjects.Add(clickedObjects[i]);
		}
		Debug.Log($"Data added | {Time.frameCount}");
		NotifyClientRpc(playerId);
		dataAvailable.Value = true;
	}

	[ClientRpc]
	public void NotifyClientRpc(int playerId = -1)
	{
		if (playerId == -1 || localIndex == playerId)
		{
			isWaitingForServer = false;
		}
	}

	[ClientRpc]
	public void StartClientRpc(int _playerCount, int _startingIndex, int _winnerIndex, int _loserIndex)
	{
		//Debug.Log("Client Started");
		if (IsClient)
		{
			startingIndex = _startingIndex;
			playerCount = _playerCount;
		}
		winnerIndex = _winnerIndex;
		loserIndex = _loserIndex;
		StartCoroutine(Setup());
	}

	void Awake()
	{
		netShuffledDeck = new NetworkList<int>();
		netClickedObjects = new NetworkList<int>();
		netStage.Value = Stages.Null;
		netRemotePlayerIndex = new NetworkVariable<int>();
		Done = new NetworkVariable<int>(0);
	}

	void Start()
	{
		WinnerTxt.SetText("");
		int wIndex = PlayerPrefs.GetInt("winnerIndex", -1);
		int lIndex = PlayerPrefs.GetInt("loserIndex", -1);
		if (IsHost)
		{
			playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
			startingIndex = UnityEngine.Random.Range(0, playerCount);
			StartClientRpc(playerCount, startingIndex, wIndex, lIndex);
		}
	}

	private void OnApplicationQuit()
	{
		// Delete all PlayerPrefs keys
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();  // Ensures the deletion is saved
		Debug.Log("All PlayerPrefs cleared on game exit.");
	}
}
