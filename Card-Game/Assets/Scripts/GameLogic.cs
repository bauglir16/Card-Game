using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public GameObject cardPrefab;
    List<CardIds> m_InitialDeck, m_PlayedCards;
    Stack<CardIds> m_ShuffledDeck;
    public GameObject[] m_Players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Players = GameObject.FindGameObjectsWithTag("Player");
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
