using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
	public List<CardData> cardsAtHand = new List<CardData>();
	public List<CardData> first3Cards;
	public List<CardData> second3Cards;
	public List<CardData> third3Cards;
	public Transform[][] downCards = new Transform[3][];
	public List<CardData> clickedObjects = new List<CardData>();
	public List<CardData> choosingList = new List<CardData>();
	Transform m_HandPivot;
	Vector3 m_ColliderSizeVector;
	public Vector3 m_RendererSizeVector;
	public float handRightRadius;
	public float handForwardRadius = 1.552f;
	public float playerHeadHight;
	public float rotationZOffset = 0.5f;
	public int sideCardsNumber;
	public int debugCardSize;
	public float zOffset = 1.552f;
	public Vector3 toTheRight;
	public Vector3 right;
	public Vector3 rotation;
	public float debugDistanceOfCards;
	//Mouse m_Mouse = Mouse.current;
	public Camera playerCamera; // Assign the player's camera in the Inspector
	private CardData lastHoveredObject;
	private Transform BehindCameraPos, AboveCardsCameraPos;
	public enum phases {Null, choosing, playing, last3};
	public phases m_Phase = phases.Null;
	public Button okButton;
	public bool deckFinished = false;
	public int RankOnTop;
	public int PowerOnTop;
	public int countRankOnTop = 0;
	public bool finishedPlaying = false;
	bool last3 = false;
	public bool Out = false;
	bool arranged = false;

	public bool finishedChoosing { get; private set; }

	void ArrangeCardsAtHand()
	{
		if (cardsAtHand.Count == 0 || arranged) return;

		cardsAtHand.Sort((x, y) => x.Rank - y.Rank);

		m_ColliderSizeVector = transform.InverseTransformVector(cardsAtHand[0].m_Collider.bounds.size);
		m_RendererSizeVector = cardsAtHand[0].m_MeshFilter.mesh.bounds.size;
		float xDistanceOfCards = m_ColliderSizeVector.x;
		float zDistanceOfCards = m_RendererSizeVector.z;
		float xOffestMultiplier = 0.5f;
		sideCardsNumber = cardsAtHand.Count / 2;

		if ((cardsAtHand.Count - 1) * Mathf.Abs(xDistanceOfCards) > handRightRadius * 2)
		{
			xDistanceOfCards = handRightRadius * 2 / cardsAtHand.Count;
		}
		debugDistanceOfCards = xDistanceOfCards;

		if (cardsAtHand.Count % 2 != 0) //Odd number of cards
		{
			cardsAtHand[sideCardsNumber].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);

			//cardsAtHand[sideCardsNumber].transform.LookAt(new Vector3(cardsAtHand[sideCardsNumber].transform.position.x, transform.position.y + playerHeadHight, transform.position.z));
			cardsAtHand[sideCardsNumber].transform.position = transform.position + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * sideCardsNumber);
			if (cardsAtHand.Count == 1) return;
			xOffestMultiplier = 1;
		}

		cardsAtHand[sideCardsNumber - 1].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
		cardsAtHand[cardsAtHand.Count - sideCardsNumber].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);

		toTheRight = transform.right * (xDistanceOfCards * xOffestMultiplier);
		cardsAtHand[sideCardsNumber - 1].transform.position = transform.position - toTheRight + transform.forward * (handForwardRadius - (zDistanceOfCards * 3 * (sideCardsNumber - 1)));
		cardsAtHand[cardsAtHand.Count - sideCardsNumber].transform.position = transform.position + toTheRight + transform.forward * (handForwardRadius - (zDistanceOfCards * 3 * (cardsAtHand.Count - sideCardsNumber)));
		int iterationCounter = 1;
		for (int i = sideCardsNumber - 2, j = cardsAtHand.Count - sideCardsNumber + 1; i >= 0 && j < cardsAtHand.Count; i--, j++)
		{
			cardsAtHand[i].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
			cardsAtHand[j].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
			toTheRight = transform.right * (xDistanceOfCards * iterationCounter + xDistanceOfCards * xOffestMultiplier);
			cardsAtHand[i].transform.position = transform.position - toTheRight + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * i);
			cardsAtHand[j].transform.position = transform.position + toTheRight + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * j);
			iterationCounter++;
		}

		arranged = true;
	}

	public void GiveFirst3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCards[2][0].transform.position;
		card1.transform.rotation = downCards[2][0].transform.rotation;
		card2.transform.position = downCards[2][1].transform.position;
		card2.transform.rotation = downCards[2][1].transform.rotation;
		card3.transform.position = downCards[2][2].transform.position;
		card3.transform.rotation = downCards[2][2].transform.rotation;

		third3Cards.Add(card1);
		third3Cards.Add(card2);
		third3Cards.Add(card3);

		//Debug.Log("GiveFirst3Cards");
	}

	public void GiveSecond3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCards[1][0].transform.position;
		card1.transform.rotation = downCards[1][0].transform.rotation;
		card2.transform.position = downCards[1][1].transform.position;
		card2.transform.rotation = downCards[1][1].transform.rotation;
		card3.transform.position = downCards[1][2].transform.position;
		card3.transform.rotation = downCards[1][2].transform.rotation;

		second3Cards.Add(card1);
		second3Cards.Add(card2);
		second3Cards.Add(card3);
	}

	public void GiveThird3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCards[0][0].transform.position;
		card1.transform.rotation = downCards[0][0].transform.rotation;
		card2.transform.position = downCards[0][1].transform.position;
		card2.transform.rotation = downCards[0][1].transform.rotation;
		card3.transform.position = downCards[0][2].transform.position;
		card3.transform.rotation = downCards[0][2].transform.rotation;

		first3Cards.Add(card1);
		first3Cards.Add(card2);
		first3Cards.Add(card3);
	}

	public void GiveCardAtHand(CardData card)
	{
		cardsAtHand.Add(card);
		arranged = false;
		ArrangeCardsAtHand();
	}

	public void TakeAllCards(List<CardData> cards)
	{
		cardsAtHand.AddRange(cards);
		cards.Clear();
		arranged = false;
		ArrangeCardsAtHand();
	}

	public void PrepareChooseCards()
	{
		//for (int i = 0; i < 3; i++) {
		//	third3Cards[i].transform.GetChild(0).gameObject.GetComponent<BoxCollider>().enabled = false;
		//}
		playerCamera.transform.SetPositionAndRotation(AboveCardsCameraPos.position, AboveCardsCameraPos.rotation);
		okButton.enabled = false;
		choosingList.AddRange(first3Cards);
		choosingList.AddRange(second3Cards);
		m_Phase = phases.choosing;
		okButton.onClick.AddListener(() => {
			int index;

			if (last3)
			{
				if (clickedObjects.Count > 1)
				{
					//Debug.Log("Error. Only one card");
					Time.timeScale = 0f;
				}

				index = third3Cards.IndexOf(clickedObjects[0]);
				GiveCardAtHand(third3Cards[index]);
				third3Cards.RemoveAt(index);
				//Debug.Log("Player: Third3Cards");
			}

			for (int i = 0; i < clickedObjects.Count; i++)
			{
				index = first3Cards.IndexOf(clickedObjects[i]);
				switch (index)
				{
					case -1:
						index = second3Cards.IndexOf(clickedObjects[i]);
						GiveCardAtHand(second3Cards[index]);
						second3Cards.RemoveAt(index);
						//second3Cards[index] = first3Cards[index];
						////first3Cards[index].transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
						//first3Cards[index] = null;
						//Debug.Log("Player: Second3Cards");
						break;
					default:
						GiveCardAtHand(first3Cards[index]);
						first3Cards.RemoveAt(index);
						//first3Cards[index].transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;
						//first3Cards[index] = null;
						//Debug.Log("Player: first3Cards");
						//first3Cards.Insert(0, null);
						break;
				}
			}

			clickedObjects.Clear();
			second3Cards.AddRange(first3Cards);
			for (int i = 0; i < second3Cards.Count; i++)
			{
				second3Cards[i].transform.SetPositionAndRotation(downCards[1][i].position, downCards[1][i].rotation);
			}
			first3Cards.Clear();
			//if (playerCamera == null) Debug.Log("Camera is null");
			OnHoverExit(cardsAtHand[0]);
			OnHoverExit(cardsAtHand[1]);
			OnHoverExit(cardsAtHand[2]);
			finishedChoosing = true;
		});
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		float x = transform.rotation.x, y = transform.rotation.y;
		transform.LookAt(new Vector3(transform.parent.transform.position.x, transform.position.y, transform.parent.transform.position.z));
		//Debug.Log(transform.rotation.x.ToString() + " " + transform.rotation.y.ToString() + " " + transform.rotation.z.ToString());

		for (int i = 0; i < 3; i++)
		{
			downCards[i] = new Transform[3];
			downCards[i][0] = transform.GetChild(i * 3);
			downCards[i][1] = transform.GetChild(i * 3 + 1);
			downCards[i][2] = transform.GetChild(i * 3 + 2);
		}
		third3Cards = new List<CardData>();
		second3Cards = new List<CardData>();
		first3Cards = new List<CardData>();

		BehindCameraPos = transform.GetChild(9);
		AboveCardsCameraPos = transform.GetChild(10);
		RankOnTop = 0;
	}

	//void HoverDownCards()
	//{
	//	if (clickedObjects.Capacity != 3) clickedObjects = new List<CardData>(3);
	//	Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
	//	RaycastHit hit;

	//	okButton.enabled = clickedObjects.Count == 3;

	//	if (Physics.Raycast(ray, out hit))
	//	{
	//		Collider hoveredCollider = hit.collider;
	//		if (hoveredCollider == null)
	//		{
	//			Debug.Log("Collider null");
	//			return;
	//		}
	//		else if (hoveredCollider.gameObject == null)
	//		{
	//			Debug.Log("GameObject null");
	//			return;
	//		}
	//		else if (hoveredCollider.gameObject.transform.parent == null)
	//		{
	//			//Debug.Log("Parent null");
	//			return;
	//		}
	//		CardData hoveredCard = hoveredCollider.gameObject.transform.parent.GetComponent<CardData>();
	//		if (first3Cards.Contains(hoveredCard) || second3Cards.Contains(hoveredCard))
	//		{

	//			if (Input.GetMouseButtonDown(0))
	//			{
	//				//if (clickedObjects.Count > 0 && hoveredObject.Rank != clickedObjects[0].Rank)
	//				//{
	//				//	for (int i = 0; i < clickedObjects.Count; i++)
	//				//		OnHoverExit(clickedObjects[i].transform.GetChild(0).gameObject);
	//				//	clickedObjects.Clear();
	//				//}
	//				if (!clickedObjects.Contains(hoveredCard) && clickedObjects.Count < 3) clickedObjects.Add(hoveredCard);
	//				else clickedObjects.Remove(hoveredCard);
	//			}

	//			if (hoveredCard != lastHoveredObject)
	//			{
	//				// Handle leaving the last hovered object
	//				if (lastHoveredObject != null && !clickedObjects.Contains(lastHoveredObject))
	//				{
	//					OnHoverExit(lastHoveredObject.transform.GetChild(0).gameObject);
	//				}

	//				// Handle entering the new hovered object
	//				OnHoverEnter(hoveredCard.transform.GetChild(0).gameObject);
	//				lastHoveredObject = hoveredCard;
	//			}
	//		}
	//	}
	//	else
	//	{
	//		// If no object is hovered, handle exiting the last hovered object
	//		if (lastHoveredObject != null)
	//		{
	//			OnHoverExit(lastHoveredObject.transform.GetChild(0).gameObject);
	//			lastHoveredObject = null;
	//		}
	//	}
	//}

	public bool CanBePlayed()
	{
		bool canBePlayed = false;

		if (clickedObjects.Count == 0) return true;

		if (PowerOnTop == 7)
			canBePlayed = clickedObjects[0].power <= 7 || clickedObjects[0].Rank == 10 || clickedObjects[0].Rank == 2 || clickedObjects[0].Rank == 4;
		else 
			canBePlayed = (clickedObjects[0].power >= PowerOnTop) ? true : false;

		int temp = clickedObjects[0].Rank;
	 	return clickedObjects.All(x => x.Rank == temp) && canBePlayed;
	}

	void HoverCards(int limit = 4)
	{

		if (clickedObjects.Capacity != limit) clickedObjects = new List<CardData>(limit);
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			Collider hoveredCollider = hit.collider;
			if (hoveredCollider == null || hoveredCollider.gameObject.layer != 6)
			{
				if (lastHoveredObject != null)
				{
					OnHoverExit(lastHoveredObject);
					lastHoveredObject = null;
				}
				return;
			}
			CardData hoveredCard = hoveredCollider.gameObject.transform.parent.GetComponent<CardData>();
			if (choosingList.Contains(hoveredCard))
			{

				if (Input.GetMouseButtonDown(0))
				{
					if (!clickedObjects.Contains(hoveredCard) && clickedObjects.Count < limit) clickedObjects.Add(hoveredCard);
					else clickedObjects.Remove(hoveredCard);
				}

				if (hoveredCard != lastHoveredObject)
				{
					// Handle leaving the last hovered object
					if (lastHoveredObject != null)
					{
						OnHoverExit(lastHoveredObject);
					}

					// Handle entering the new hovered object
					if (clickedObjects.Count < limit)
						OnHoverEnter(hoveredCard);
					lastHoveredObject = hoveredCard;
				}
			}
			if (hoveredCard != lastHoveredObject)
			{
				// Handle leaving the last hovered object
				if (lastHoveredObject != null)
				{
					OnHoverExit(lastHoveredObject);
				}
			}
		}
		else
		{
			// If no object is hovered, handle exiting the last hovered object
			if (lastHoveredObject != null)
			{
				OnHoverExit(lastHoveredObject);
				lastHoveredObject = null;
			}
		}

		okButton.enabled = (m_Phase == phases.choosing || last3) ? clickedObjects.Count == limit : CanBePlayed() ;
	}

	public void Try()
	{
		CardData temp = first3Cards[0];
		first3Cards.RemoveAt(0);
		GiveCardAtHand(temp);
		temp = first3Cards[0];
		first3Cards.RemoveAt(0);
		GiveCardAtHand(temp);
		temp = first3Cards[0];
		first3Cards.RemoveAt(0);
		GiveCardAtHand(temp);
	}

	// Update is called once per frame
	void Update()
	{
		debugCardSize = cardsAtHand.Count;
		right = transform.right;
		rotation = transform.rotation.eulerAngles;

		switch (m_Phase)
		{
			case phases.choosing:
				//PrepareChooseCards();
				HoverCards(3);
				break;
			case phases.playing:
				if (last3)
				{
					HoverCards(1);
					break;
				}
				ArrangeCardsAtHand();
				HoverCards();
				break;
		}

		
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position + transform.right * handRightRadius, transform.position - transform.right * handRightRadius);
		Gizmos.DrawLine(transform.position + transform.right * handRightRadius + transform.forward * handForwardRadius, transform.position - transform.right * handRightRadius + transform.forward * handForwardRadius);
		//for (int i = 0; i < 3; i++)
		//{
		//	for (int j = 0; j < 3; j++)
		//	{
		//		Gizmos.DrawCube(downCards[i][j].transform.position, new Vector3(0.1f, 0.1f, 0.1f));
		//	}
		//}
	}

	void OnHoverEnter(CardData obj)
	{
		obj.m_Renderer.material.color = Color.cyan; // Highlight color
	}

	void OnHoverExit(CardData obj, bool force = false)
	{
		if (force || !clickedObjects.Contains(obj))
			obj.m_Renderer.material.color = Color.white; // Original color
	}

	internal void Play()
	{
		if (cardsAtHand.Count == 0 && deckFinished && second3Cards.Count > 0)
		{
			for (int i = 0; i < 3; i++)
			{
				GiveCardAtHand(second3Cards[i]);
			}
			second3Cards.Clear();
		}
		else if (cardsAtHand.Count == 0 && deckFinished && second3Cards.Count == 0)
		{
			choosingList = third3Cards;
			last3 = true;
		}
		else if (cardsAtHand.Count > 0) choosingList = cardsAtHand;

		m_Phase = phases.playing;
		finishedPlaying = false;
		playerCamera.transform.SetPositionAndRotation(BehindCameraPos.position, BehindCameraPos.rotation);

		//Debug.Log("Play");

		okButton.onClick.AddListener(() => {
			if (last3)
			{
				GiveCardAtHand(clickedObjects[0]);
				OnHoverExit(clickedObjects[0], true);
				third3Cards.Remove(clickedObjects[0]);
				clickedObjects.Clear();
				choosingList = cardsAtHand;
				last3 = false;
			}
			else
			{
				for (int i = 0; i < clickedObjects.Count; i++)
				{
					OnHoverExit(clickedObjects[i], true);
					cardsAtHand.Remove(clickedObjects[i]);
				}
				finishedPlaying = true;
				//Debug.Log(finishedPlaying);
			}

			Out = third3Cards.Count == 0 && cardsAtHand.Count == 0;
			arranged = false;
			ArrangeCardsAtHand();
		});
	}
}
