using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
	public CardData cardPrefab;
	public PlayerScript playerPrefab;
	List<CardIds> m_InitialDeck, m_PlayedCards;
	Stack<CardIds> m_ShuffledDeck;
	public int playerCount;
	public float playerRadius;
	GameObject winner, loser;
	public List<PlayerScript> m_Players = new List<PlayerScript>();
	Transform cardSpawnPos;
	GameObject m_PhysicalDeck;

	bool m_SetupStage;
	int m_PlayerIndex, m_3CardsIndex;

	void Give3Cards(int index, CardIds[] cards, PlayerScript player)
	{
		CardData[] newCards = new CardData[3];
		for (int i = 0; i < 3; i++)
		{
			newCards[i] = Instantiate<CardData>(cardPrefab, cardSpawnPos.position, cardSpawnPos.rotation, transform);
			newCards[i].Set(cards[i]);
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

		Debug.Log("Give3Cards " + index);
	}

	void GiveCard(CardIds card, PlayerScript player)
	{
		CardData newCard = Instantiate<CardData>(cardPrefab, cardSpawnPos.position, cardSpawnPos.rotation, transform);
		newCard.Set(card);
		player.GiveCardAtHand(newCard);
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
			Debug.Log(angle);
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

		m_PlayedCards = new List<CardIds>();
		m_3CardsIndex = 0; m_PlayerIndex = 0;
		m_SetupStage = true;
	}

	void Setup()
	{

	}

	// Update is called once per frame
	void Update()
	{
		Debug.Log("GameLogic: " + m_SetupStage + " " + m_PlayerIndex + " " + m_3CardsIndex);
		if (m_SetupStage == true)
		{
			CardIds[] tempCards = new CardIds[3];
			tempCards[0] = m_ShuffledDeck.Pop();
			tempCards[1] = m_ShuffledDeck.Pop();
			tempCards[2] = m_ShuffledDeck.Pop();
			Give3Cards(m_3CardsIndex, tempCards, m_Players[m_PlayerIndex]);


			Debug.Log("GameLogic: +=1");
			m_PlayerIndex += 1;
			if (m_PlayerIndex >= playerCount) { m_PlayerIndex = 0; m_3CardsIndex += 1; }

			if (m_PlayerIndex == 0 && m_3CardsIndex >= 3) 
			{ 
				m_SetupStage = false;
				m_Players[3].Try();
			}
		}


	}
}
