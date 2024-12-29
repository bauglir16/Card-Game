using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
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
	bool m_SetupStage;
	int m_PlayerIndex, m_3CardsIndex;
	enum Stages { setup, playing, choosing }
	Stages stage = Stages.setup;
	public int RankOnTop;
	public int countRankOnTop = 0;
	public int PowerOnTop;
	bool sameTurn = false;

	List<CardData> cards = new List<CardData>();
	bool again = false;
	CardData firstCard;

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

		if (m_ShuffledDeck.Count == 0)
		{
			for (int i = 0;i < m_Players.Count;i++)
			{
				m_Players[i].deckFinished = true;
			}
		}
	}

	bool PlayCards(PlayerScript player)
	{
		//Debug.Log("Playing cards");

		bool playAgain = false;
		
		if (player.clickedObjects.Count == 0)
		{
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
			else if (player.clickedObjects[0].Rank == 2) PowerOnTop = 0;
			else if (player.clickedObjects[0].Rank == 4) 
			{
				RankOnTop = 4;
				countRankOnTop = 1;
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
	void Start()
	{
		//m_Players = GameObject.FindGameObjectsWithTag("Player").ToList();
		cardSpawnPos = transform.GetChild(0).transform.GetChild(52);
		m_PhysicalDeck = transform.GetChild(0).gameObject;
		//playerRadius = 6.7f;
		for (int i = 0; i < playerCount; i++)
		{
			m_Players.Add(Instantiate<PlayerScript>(playerPrefab));
			m_Players[i].transform.SetParent(transform, false);
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
			//cardPrefab.id = cardId.Null + i;
			//cardPrefab = Resources.Load("Resources/Card_" + (CardIds.Null + i).ToString()) as GameObject;
			m_InitialDeck.Add(CardIds.Null + i);

		}

		m_ShuffledDeck = new Stack<CardIds>(m_InitialDeck.Count);
		int randomCard;
		while (m_InitialDeck.Count > 0)
		{
			randomCard = Random.Range(0, m_InitialDeck.Count);
			m_ShuffledDeck.Push(m_InitialDeck[randomCard]);
			m_InitialDeck.RemoveAt(randomCard);
		}

		m_PlayedCards = new List<CardData>();
		m_cardThickness = m_PhysicalDeck.transform.localPosition.y;//transform.GetChild(0).transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size.z * 4;
		m_cardThickness -= 1.255998f;
		m_cardThickness /= 52;
		m_PhysicalDeck.transform.position += new Vector3(0, m_cardThickness * 3, 0);
		m_3CardsIndex = 0; m_PlayerIndex = 0;
		playedPos = transform.GetChild(1);
		initialPlayedPos = playedPos.position;
	}

	// Update is called once per frame
	void Update()
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

		foreach (Renderer renderer in renderers)
		{
			combinedBounds.Encapsulate(renderer.bounds);
		}

		Vector3 totalSize = combinedBounds.size;
		
		switch (stage)
		{
			case Stages.setup:
				CardIds[] tempCards = new CardIds[3];
				tempCards[0] = m_ShuffledDeck.Pop();
				tempCards[1] = m_ShuffledDeck.Pop();
				tempCards[2] = m_ShuffledDeck.Pop();
				Give3Cards(m_3CardsIndex, tempCards, m_Players[m_PlayerIndex]);

				m_PlayerIndex += 1;
				if (m_PlayerIndex >= playerCount) { m_PlayerIndex = 0; m_3CardsIndex += 1; }

				if (m_PlayerIndex == 0 && m_3CardsIndex >= 3)
				{
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
					stage = Stages.choosing;
					//m_Players[3].Try();
				}

				//Debug.Log("Total Composite Object Size: " + totalSize);

				break;

			case Stages.choosing:
				if (m_Players[m_PlayerIndex].m_Phase != PlayerScript.phases.choosing)
				{
					//Debug.Log("Player" + m_PlayerIndex + " is choosing");
					m_Players[m_PlayerIndex].playerCamera = Camera.main;
					m_Players[m_PlayerIndex].okButton = GameObject.FindAnyObjectByType<Button>();
					m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
					m_Players[m_PlayerIndex].PrepareChooseCards();
				}
				else if (m_Players[m_PlayerIndex].finishedChoosing)
				{
					m_Players[m_PlayerIndex].m_Phase = PlayerScript.phases.Null;
					m_Players[m_PlayerIndex].okButton = null;
					m_Players[m_PlayerIndex++].playerCamera = null;
				}

				if (m_PlayerIndex == playerCount)
				{
					m_PlayerIndex = 0;
					stage = Stages.playing;
				}
				break;

			case Stages.playing:
				if (m_Players.Count > 1)
				{
					if (!sameTurn)
					{
						//Debug.Log("Player :" + (m_PlayerIndex + 1) + " is playing");
						m_Players[m_PlayerIndex].RankOnTop = (m_PlayedCards.Count > 0) ? m_PlayedCards[m_PlayedCards.Count - 1].Rank : 0;
						m_Players[m_PlayerIndex].PowerOnTop = PowerOnTop;
						m_Players[m_PlayerIndex].countRankOnTop = countRankOnTop;
						m_Players[m_PlayerIndex].playerCamera = Camera.main;
						m_Players[m_PlayerIndex].okButton = GameObject.FindAnyObjectByType<Button>();
						m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
						m_Players[m_PlayerIndex].Play();
						sameTurn = true;
					}

					if (m_Players[m_PlayerIndex].finishedPlaying)
					{
						bool playAgain = PlayCards(m_Players[m_PlayerIndex]);
						for (int i = m_Players[m_PlayerIndex].cardsAtHand.Count; m_ShuffledDeck.Count > 0 && i < 3; i++)
						{
							GiveCard(m_ShuffledDeck.Pop(), m_Players[m_PlayerIndex]);
							//Debug.Log("Total Composite Object Size: " + totalSize);
						}

						if (m_Players[m_PlayerIndex].Out)
						{
							if (m_Players.Count == playerCount)
							{
								winner = m_Players[m_PlayerIndex];
								Debug.Log("Player " + m_PlayerIndex + " is the winner");
							}

							m_Players.RemoveAt(m_PlayerIndex);
							m_PlayerIndex -= (m_PlayerIndex == m_Players.Count) ? m_PlayerIndex + 1 : 1; //Size of list = last index before removal

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
							m_Players[m_PlayerIndex].m_Phase = PlayerScript.phases.Null;
							m_Players[m_PlayerIndex].okButton = null;
							m_Players[m_PlayerIndex].playerCamera = null;
							m_PlayerIndex = (m_PlayerIndex + 1) % m_Players.Count();
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
						}

						m_Players[m_PlayerIndex].m_Phase = PlayerScript.phases.Null;
						sameTurn = false;
					}
					break;
				}
				loser = m_Players[0];
				Debug.Log("Player " + 0 + " is the whore");
				break;
		}
	}
}
