using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
	public CardData cardPrefab;
	public PlayerScript playerPrefab;
	List<CardIds> m_InitialDeck;
	List<CardData> m_PlayedCards;
	Stack<CardIds> m_ShuffledDeck;
	public int playerCount;
	public float playerRadius;
	GameObject winner, loser;
	public List<PlayerScript> m_Players = new List<PlayerScript>();
	Transform cardSpawnPos, playedPos;
	GameObject m_PhysicalDeck;
	float m_cardThickness;
	bool m_SetupStage;
	int m_PlayerIndex, m_3CardsIndex;
	enum Stages { setup, playing, choosing }
	Stages stage = Stages.setup;
	int RankOnTop;
	int countRankOnTop = 0;

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
				Debug.Log("Give3Cards default");
				break;
		}
	}

	void GiveCard(CardIds card, PlayerScript player)
	{
		CardData newCard = PrepareCard(card);
		player.GiveCardAtHand(newCard);
	}

	bool PlayCards(PlayerScript player)
	{
		bool playAgain = false;
		
		if (player.clickedObjects.Count == 0)
		{
			player.TakeAllCards(m_PlayedCards);
			playedPos.transform.position -= new Vector3(0, m_cardThickness * m_PlayedCards.Count, 0);
			RankOnTop = 0;
			countRankOnTop = 0;
			m_PlayedCards.Clear();
			return playAgain;
		}

		if (m_PlayedCards[m_PlayedCards.Count - 1].Rank == player.clickedObjects[0].Rank)
		{
			countRankOnTop += player.clickedObjects.Count();
			if (countRankOnTop == 4)
			{
				playAgain = true;
			}
		}

		if (player.clickedObjects[0].Rank == 10 || player.clickedObjects.Count == 4)
		{
			RankOnTop = 0;
			countRankOnTop = 0;
			playAgain = true;
		}
		else if (player.clickedObjects[0].Rank == 2) RankOnTop = 0;
		else if (player.clickedObjects[0].Rank == 4) RankOnTop = m_PlayedCards[m_PlayedCards.Count - 1].Rank;
		
		

		for (int i = 0; i < player.clickedObjects.Count; i++)
		{
			
			m_PlayedCards.Add(player.clickedObjects[i]);
			player.clickedObjects[i].transform.position = playedPos.transform.position;
			player.clickedObjects[i].transform.rotation = playedPos.transform.rotation;
			playedPos.transform.position += new Vector3(0, m_cardThickness, 0);
		}

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
		m_cardThickness = transform.GetChild(0).transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size.z * 4;
		m_3CardsIndex = 0; m_PlayerIndex = 0;
		playedPos = transform.GetChild(1).gameObject.transform;
	}

	void Setup()
	{

	}

	// Update is called once per frame
	void Update()
	{
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
					stage = Stages.choosing;
					//m_Players[3].Try();
				}
				break;

			case Stages.choosing:
				//for (int i = 0; i < m_Players.Count; i++)
				//{
				//	m_Players[i].PrepareChooseCards();
				//}
				//bool finished = true;
				//for (int i = 0; i < m_Players.Count; i++)
				//{
				//	finished &= m_Players[i].finishedChoosing;
				//}
				//if (!finished)
				//{
				//	for (int i = 0; i < m_Players.Count; i++)
				//	{
				//		finished &= m_Players[i].finishedChoosing;
				//	}
				//}
				if (m_Players[m_PlayerIndex].m_Phase != PlayerScript.phases.choosing)
				{
					Debug.Log("Player" + m_PlayerIndex + " is choosing");
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
					
					m_Players[m_PlayerIndex].RankOnTop = (m_PlayedCards.Count > 0) ? m_PlayedCards[m_PlayedCards.Count - 1].Rank : 0;
					m_Players[m_PlayerIndex].playerCamera = Camera.main;
					m_Players[m_PlayerIndex].okButton = GameObject.FindAnyObjectByType<Button>();
					m_Players[m_PlayerIndex].okButton.onClick.RemoveAllListeners();
					m_Players[m_PlayerIndex].Play();
					if (m_Players[m_PlayerIndex].m_Phase == PlayerScript.phases.Null)
					{
						bool playAgain = PlayCards(m_Players[m_PlayerIndex]);
						for (int i = m_Players[m_PlayerIndex].cardsAtHand.Count; i < 3;)
						{
							GiveCard(m_ShuffledDeck.Pop(), m_Players[m_PlayerIndex]);
						}
						if (!playAgain) 
						{ 
							m_PlayerIndex = (m_PlayerIndex + 1) % m_Players.Count();
							m_Players[m_PlayerIndex].m_Phase = PlayerScript.phases.Null;
							m_Players[m_PlayerIndex].okButton = null;
							m_Players[m_PlayerIndex].playerCamera = null;
						}
						else
						{
							for (int i = 0; i < m_PlayedCards.Count; i++)
								GameObject.Destroy(m_PlayedCards[i]);
							m_PlayedCards.Clear();
						}
					}
				}
				break;
		}
	}
}
