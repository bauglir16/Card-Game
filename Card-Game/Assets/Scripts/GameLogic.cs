using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : NetworkBehaviour
{
	public CardData cardPrefab;
	public PlayerScript playerPrefab;
	List<CardIds> m_InitialDeck;
	public List<CardData> m_PlayedCards;
	Stack<CardIds> m_ShuffledDeck;
	public int playerCount;
	public float playerRadius;
	PlayerScript winner, loser;
	public List<PlayerScript> m_Players = new List<PlayerScript>();
	Transform cardSpawnPos, playedPos;
	Vector3 initialPlayedPos;
	GameObject m_PhysicalDeck;
	float m_cardThickness;
	public int m_PlayerIndex;
	enum Stages { Null, setup, choosing, givingCards, playing }
	Stages stage = Stages.Null;
	public int RankOnTop;
	public int countRankOnTop = 0;
	public int PowerOnTop;
	bool playerIsOut;
	public NetworkVariable<int> Done;
	public int localIndex;
	public NetworkVariable<int> netRemotePlayerIndex;
	public NetworkList<int> netShuffledDeck;
	public NetworkList<int> netClickedObjects;
	NetworkVariable<Stages> netStage = new NetworkVariable<Stages>();

	List<CardData> cards = new List<CardData>();
	bool again = false;
	CardData firstCard;
	private bool isWaitingForServer;
	private Button okButton;
	int roundN = 1;

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
				//Debug.Log("Give3Cards default");
				break;
		}

		m_PhysicalDeck.transform.localPosition -= new Vector3(0, m_cardThickness * 3, 0);
		//Debug.Log(m_cardThickness * 3 + "|" + m_PhysicalDeck.transform.localPosition.y);
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
		//Debug.Log("Playing cards");

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
			{
				//PowerOnTop = player.clickedObjects[0].power;
				countRankOnTop += player.clickedObjects.Count;
			}
			else if (player.clickedObjects[0].Rank == 10)// || player.clickedObjects.Count == 4)
			{
				playAgain = true;
			}
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
				//Debug.Log(countRankOnTop);
			}

			playAgain |= countRankOnTop == 4;
		}

		for (int i = 0; i < player.clickedObjects.Count; i++)
		{
			player.clickedObjects[i].transform.SetPositionAndRotation(playedPos.transform.position, playedPos.transform.rotation);
			playedPos.transform.localPosition += new Vector3(0, m_cardThickness, 0);
		}

		m_PlayedCards.AddRange(player.clickedObjects);

		player.clickedObjects.Clear();
		return playAgain;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	public IEnumerator Setup()
	{
		stage = Stages.Null;
		localIndex = (int)NetworkManager.Singleton.LocalClientId;
		cardSpawnPos = transform.GetChild(0).transform.GetChild(52);
		m_PhysicalDeck = transform.GetChild(0).gameObject;
		for (int i = 0; i < playerCount; i++)
		{
			m_Players.Add(Instantiate<PlayerScript>(playerPrefab, transform));
			//m_Players[i].transform.SetParent(transform, false);
		}
		for (int i = 0; i < m_Players.Count; i++)
		{
			float angle = Mathf.PI * 2 * i / m_Players.Count;
			float x = transform.position.x + Mathf.Cos(angle) * playerRadius;
			float y = transform.position.y + 6f;
			float z = transform.position.z + Mathf.Sin(angle) * playerRadius;
			m_Players[i].transform.position = new Vector3(x, y, z);
			m_Players[i].transform.LookAt(new Vector3(transform.position.x, m_Players[i].transform.position.y, transform.position.z));
		}


		m_InitialDeck = new List<CardIds>(52);
		for (int i = 1; i <= 52; i++)
		{
			m_InitialDeck.Add(CardIds.Null + i);

		}

		m_ShuffledDeck = new Stack<CardIds>(m_InitialDeck.Count);
		int randomCard;
		if (IsHost)
		{
			//Debug.Log(m_InitialDeck.Count);
			while (m_InitialDeck.Count > 0)
			{
				randomCard = Random.Range(0, m_InitialDeck.Count);
				netShuffledDeck.Add((int)m_InitialDeck[randomCard]);
				m_InitialDeck.RemoveAt(randomCard);
			}
			Debug.Log("Net deck count: " + netShuffledDeck.Count + " | " + Time.frameCount);
		}


		m_PlayedCards = new List<CardData>();
		m_cardThickness = m_PhysicalDeck.transform.localPosition.y;//transform.GetChild(0).transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size.z * 4;
		m_cardThickness -= 1.255998f;
		m_cardThickness /= 52;
		m_PhysicalDeck.transform.position += new Vector3(0, m_cardThickness * 3, 0); m_PlayerIndex = 0;
		playedPos = transform.GetChild(1);
		initialPlayedPos = playedPos.position;

		stage = Stages.setup;

		if (IsHost)
		{
			Done.Value++;
			while (Done.Value != m_Players.Count)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			NotifyClientRpc();
		}
		else
		{
			Debug.Log("Client finished");
			eventDoneServerRpc(localIndex);//1);
			isWaitingForServer = true;
			while (isWaitingForServer)
			{
				yield return null;
			}
		}



		if (IsHost)
		{
			yield return null;
			Done.Value = 0;
			netStage.Value = Stages.setup;
		}

		StartCoroutine(updateCoroutine());
	}

	[ServerRpc(RequireOwnership = false)]
	public void eventDoneServerRpc(int localIndex = -1)//int n)
	{
		//if (n == 1)
		Done.Value++;
		if (localIndex >= 0) Debug.Log("Client index: " + localIndex);
		//else if (n == 2)
		//	Done2.Value++;
	}

	IEnumerator updateCoroutine()
	{
		for (int i = 0; i < netShuffledDeck.Count; i++)
		{
			m_ShuffledDeck.Push((CardIds)netShuffledDeck[i]);
		}
		m_InitialDeck.Clear();

		if (IsHost)
		{
			Done.Value++;
			while (Done.Value != m_Players.Count)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			NotifyClientRpc();
		}
		else
		{
			Debug.Log("Client finished");
			eventDoneServerRpc(localIndex);//1);
			isWaitingForServer = true;
			while (isWaitingForServer)
			{
				yield return null;
			}
		}

		if (IsHost)
		{
			Debug.Log("Net deck cleared | " + Done.Value + " | " + Time.frameCount);
			Done.Value = 0;
			netShuffledDeck.Clear();
		}

		CardIds[] tempCards = new CardIds[3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < playerCount; j++)
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

		m_Players[localIndex].RankOnTop = RankOnTop;
		m_Players[localIndex].PowerOnTop = PowerOnTop;
		m_Players[localIndex].countRankOnTop = countRankOnTop;
		m_Players[localIndex].playerCamera = Camera.main;
		m_Players[localIndex].okButton = GameObject.FindAnyObjectByType<Button>();
		m_Players[localIndex].okButton.onClick.RemoveAllListeners();

		stage = Stages.choosing;

		if (IsHost)
		{
			Done.Value++;
			while (Done.Value != m_Players.Count)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			NotifyClientRpc();
		}
		else
		{
			Debug.Log("Client finished");
			eventDoneServerRpc(localIndex);//1);
			isWaitingForServer = true;
			while (isWaitingForServer)
			{
				yield return null;
			}
		}

		if (IsHost)
		{
			Done.Value = 0;
			netStage.Value = Stages.choosing;
		}

		m_Players[localIndex].PrepareChooseCards();

		while (!m_Players[localIndex].finishedChoosing)
			yield return null;
		m_Players[localIndex].okButton = null;

		stage = Stages.givingCards;

		if (IsHost)
		{
			Done.Value++;
			while (Done.Value != m_Players.Count)
			{
				Debug.Log("Set up done: " + Done.Value);
				yield return null;
			}
			Debug.Log("All clients finished");
			NotifyClientRpc();
		}
		else
		{
			eventDoneServerRpc(localIndex);//1);
			Debug.Log($"Client finished id:{localIndex} done:{Done.Value}");
			isWaitingForServer = true;
			while (isWaitingForServer)
			{
				yield return null;
			}
		}

		if (IsHost)
		{
			Done.Value = 0;
			netStage.Value = Stages.givingCards;
		}

		while (netStage.Value < Stages.givingCards)
		{
			yield return null;
		}

		while (m_PlayerIndex < playerCount)
		{
			if (m_PlayerIndex == localIndex)
			{
				if (!IsHost)
				{
					eventSetClickedObjectsServerRpc(m_Players[localIndex].CardsAtHandIdsAsInts(), localIndex);
				}
				else
					SetNetClickedObjects(m_Players[localIndex].CardsAtHandIdsAsInts());

				Debug.Log("Local player net deck size: " + netClickedObjects.Count + " | " + Time.frameCount);
			}
			else
			{
				while (Done.Value == 0)
				{
					Debug.Log($"No data yet | {Time.frameCount}");
					yield return null;
					Debug.Log($"Data added | {Time.frameCount}");
				}

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
				m_Players[m_PlayerIndex].okButton = GameObject.FindAnyObjectByType<Button>();
				m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
				m_Players[m_PlayerIndex].PrepareChooseCards(false);
				m_Players[m_PlayerIndex].okButton.onClick.Invoke();
			}

			if (IsHost)
			{
				if (m_PlayerIndex != localIndex) Done.Value++;
				while (Done.Value != m_Players.Count)
				{
					Debug.Log("Set up done: " + Done.Value);
					yield return null;
				}
				NotifyClientRpc();
				Debug.Log("Clients notified");
			}
			else
			{
				if (m_PlayerIndex != localIndex) eventDoneServerRpc();
				Debug.Log("Client finished");
				isWaitingForServer = true;
				while (isWaitingForServer)
				{
					Debug.Log("Waiting for server");
					yield return null;
				}
			}

			if (IsHost)
			{
				Done.Value = 0;
				netRemotePlayerIndex.Value++;
				netClickedObjects.Clear();
			}
			m_PlayerIndex++;

			while (m_PlayerIndex != netRemotePlayerIndex.Value && m_PlayerIndex < m_Players.Count)
				yield return null;
		}
		stage = Stages.playing;
		m_PlayerIndex = 0;
		if (IsHost)
		{
			netRemotePlayerIndex.Value = 0;
			netStage.Value = Stages.playing;
		}

		while (netStage.Value < Stages.playing)
		{
			yield return null;
		}
		while (m_Players.Count > 1)
		{
			Debug.Log($"=================================ROUND {roundN}=================================");
			m_Players[localIndex].RankOnTop = RankOnTop;
			m_Players[localIndex].PowerOnTop = PowerOnTop;
			m_Players[localIndex].countRankOnTop = countRankOnTop;
			m_Players[localIndex].SetCamera();
			if (localIndex == m_PlayerIndex)
			{
				Debug.Log($"Playing frame: {Time.frameCount}");

				m_Players[localIndex].okButton = GameObject.FindAnyObjectByType<Button>();
				m_Players[localIndex].okButton.onClick.RemoveAllListeners();
				m_Players[localIndex].Play();

				while (!m_Players[localIndex].finishedPlaying)
					yield return null;
				m_Players[localIndex].okButton = null;
				m_Players[localIndex].finishedPlaying = false;

				if (!IsHost)
				{
					eventSetClickedObjectsServerRpc(m_Players[localIndex].ClickedObjectsIdsAsInts(), localIndex);
				}
				else
					SetNetClickedObjects(m_Players[localIndex].ClickedObjectsIdsAsInts());

				Debug.Log("Local player net deck size: " + netClickedObjects.Count + " | " + Time.frameCount);
			}
			else
			{
				Debug.Log($"Not playing frame: {Time.frameCount}");
				//m_Players[localIndex].okButton.enabled = false;

				while (Done.Value < 1) yield return null;

				for (int i = 0; i < netClickedObjects.Count; i++)
					m_Players[m_PlayerIndex].addClickedCard((CardIds)netClickedObjects[i]);
				Debug.Log("Remote player net deck size: " + netClickedObjects.Count + " | " + Time.frameCount);
				m_Players[m_PlayerIndex].okButton = GameObject.FindAnyObjectByType<Button>();
				m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
				m_Players[m_PlayerIndex].Play(false);
				m_Players[m_PlayerIndex].okButton.onClick.Invoke();
				m_Players[m_PlayerIndex].okButton = null;
			}

			if (IsHost)
			{
				if (localIndex != m_PlayerIndex) Done.Value++;
				while (Done.Value != m_Players.Count)
				{
					Debug.Log("Set up done: " + Done.Value);
					yield return null;
				}
				Done.Value = 0;
				NotifyClientRpc();
			}
			else
			{
				Debug.Log("Client finished");
				isWaitingForServer = true;
				if (localIndex != m_PlayerIndex) eventDoneServerRpc(localIndex);//1);
				while (isWaitingForServer)
				{
					yield return null;
				}
			}

			if (m_Players[m_PlayerIndex].clickedObjects.Count > 0) Debug.Log(m_Players[m_PlayerIndex].clickedObjects[0].Id);
			bool playAgain = PlayCards(m_Players[m_PlayerIndex]);
			if (playAgain) Debug.Log($"Play again: {m_PlayerIndex}");
			for (int i = m_Players[m_PlayerIndex].cardsAtHand.Count; m_ShuffledDeck.Count > 0 && i < 3; i++)
			{
				GiveCard(m_ShuffledDeck.Pop(), m_Players[m_PlayerIndex]);
			}

			if (m_Players[m_PlayerIndex].Out)
			{
				if (m_Players.Count == playerCount)
				{
					winner = m_Players[m_PlayerIndex];
					Debug.Log("Player " + m_PlayerIndex + " is the winner");
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
					playAgain = false;
				}
			}

			if (!playAgain)
			{
				if (playerIsOut)
				{
					m_Players.RemoveAt(m_PlayerIndex);
					m_PlayerIndex -= (m_PlayerIndex == m_Players.Count) ? m_PlayerIndex + 1 : 1; //Size of list = last index before removal
					playerIsOut = false;
				}

				m_PlayerIndex = (m_PlayerIndex + 1) % m_Players.Count();

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
				m_PlayedCards.Clear();
				playAgain = false;
			}

			//m_Players[m_PlayerIndex].m_Phase = PlayerScript.phases.Null;

			if (IsHost)
			{
				while (Done.Value < m_Players.Count - 1)
				{
					Debug.Log("Set up done: " + Done.Value);
					yield return null;
				}
				netRemotePlayerIndex.Value = m_PlayerIndex;
				Done.Value = 0;
				NotifyClientRpc();
			}
			else
			{
				Debug.Log($"Client finished frame: {Time.frameCount}");
				isWaitingForServer = true;
				eventDoneServerRpc(localIndex);//1);
				while (isWaitingForServer || Done.Value != 0)
				{
					yield return null;
				}
				Debug.Log($"Wait finished frame: {Time.frameCount}");
			}

			roundN++;
			yield return null;
		}
		loser = m_Players[0];
		Debug.Log("Player " + 0 + " is the whore");
	}

	private void WaitForOthers()
	{
		StartCoroutine(WaitingForOthers());
	}

	private IEnumerator WaitingForOthers()
	{
		Debug.Log("Wait for others: " + Done.Value + " | " + m_Players.Count + " | " + Time.frameCount);
		while (Done.Value != m_Players.Count)
		{
			yield return null;
		}
		Debug.Log("Wait for others finished" + " | " + Time.frameCount);

	}

	private void WaitForData()
	{
		StartCoroutine(WaitingForData());
	}

	private IEnumerator WaitingForData()
	{
		while (Done.Value == 0)
			yield return null;
	}

	private void WaitForServer()
	{
		isWaitingForServer = true;
		StartCoroutine(WaitingForServer());
	}

	private IEnumerator WaitingForServer()
	{
		while (isWaitingForServer)
		{
			yield return null; // Wait for the next frame
		}
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
		Done.Value++;
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
		Done.Value++;
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
	public void StartClientRpc()
	{
		//Debug.Log("Client Started");
		StartCoroutine(Setup());
	}

	void Start()
	{
		netShuffledDeck = new NetworkList<int>();
		netClickedObjects = new NetworkList<int>();
		netStage.Value = Stages.Null;
		netRemotePlayerIndex = new NetworkVariable<int>();
		Done = new NetworkVariable<int>(0);

		okButton = GameObject.FindAnyObjectByType<Button>();
		okButton.onClick.RemoveAllListeners();
		okButton.onClick.AddListener(() =>
		{
			StartClientRpc();

			okButton.onClick.RemoveAllListeners();
		});
	}
}
