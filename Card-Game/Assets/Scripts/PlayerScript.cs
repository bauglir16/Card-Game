using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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
	public Transform[][] downCardsTransforms = new Transform[3][];
	public Transform downCardsObject;
	public List<CardData> clickedObjects = new List<CardData>();
	public List<CardData> choosingList = new List<CardData>();
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
	public bool last3 = false;
	public bool Out = false;
	bool arranged = false;
	public int id;
	private int cardLimit;

	public bool finishedChoosing { get; set; }

	void ArrangeCardsAtHand()
	{
		if (cardsAtHand.Count == 0 || arranged) return;

		cardsAtHand.Sort((x, y) => x.Rank - y.Rank);

		Quaternion newRot = transform.rotation * Quaternion.Euler(0, 180f, 0);

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
			MoveCard(cardsAtHand[sideCardsNumber], transform.position + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * sideCardsNumber), newRot);
			if (cardsAtHand.Count == 1) return;
			xOffestMultiplier = 1;
		}

		toTheRight = transform.right * (xDistanceOfCards * xOffestMultiplier);
		MoveCard(cardsAtHand[sideCardsNumber - 1], transform.position - toTheRight + transform.forward * (handForwardRadius - (zDistanceOfCards * 3 * (sideCardsNumber - 1))), newRot);
		MoveCard(cardsAtHand[cardsAtHand.Count - sideCardsNumber], transform.position + toTheRight + transform.forward * (handForwardRadius - (zDistanceOfCards * 3 * (cardsAtHand.Count - sideCardsNumber))), newRot);
		int iterationCounter = 1;
		for (int i = sideCardsNumber - 2, j = cardsAtHand.Count - sideCardsNumber + 1; i >= 0 && j < cardsAtHand.Count; i--, j++)
		{
			toTheRight = transform.right * (xDistanceOfCards * iterationCounter + xDistanceOfCards * xOffestMultiplier);
			MoveCard(cardsAtHand[i], transform.position - toTheRight + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * i), newRot);
			MoveCard(cardsAtHand[j], transform.position + toTheRight + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * j), newRot);
			iterationCounter++;
		}

		arranged = true;
	}

	private void MoveCard(CardData cardData, Vector3 newPos, Quaternion newRot)
	{
		cardData.SetTargetPositionAndRotation(newPos, newRot);
	}

	public void GiveFirst3Cards(CardData card1, CardData card2, CardData card3)
	{
		if (card1 == null) Debug.Log("Null");
		card1.transform.position = downCardsTransforms[2][0].position;
		card1.transform.rotation = downCardsTransforms[2][0].rotation;
		card2.transform.position = downCardsTransforms[2][1].position;
		card2.transform.rotation = downCardsTransforms[2][1].rotation;
		card3.transform.position = downCardsTransforms[2][2].position;
		card3.transform.rotation = downCardsTransforms[2][2].rotation;

		third3Cards.Add(card1);
		third3Cards.Add(card2);
		third3Cards.Add(card3);

		//Debug.Log("GiveFirst3Cards");
	}

	public void GiveSecond3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCardsTransforms[1][0].position;
		card1.transform.rotation = downCardsTransforms[1][0].rotation;
		card2.transform.position = downCardsTransforms[1][1].position;
		card2.transform.rotation = downCardsTransforms[1][1].rotation;
		card3.transform.position = downCardsTransforms[1][2].position;
		card3.transform.rotation = downCardsTransforms[1][2].rotation;

		second3Cards.Add(card1);
		second3Cards.Add(card2);
		second3Cards.Add(card3);
	}

	public void GiveThird3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCardsTransforms[0][0].position;
		card1.transform.rotation = downCardsTransforms[0][0].rotation;
		card2.transform.position = downCardsTransforms[0][1].position;
		card2.transform.rotation = downCardsTransforms[0][1].rotation;
		card3.transform.position = downCardsTransforms[0][2].position;
		card3.transform.rotation = downCardsTransforms[0][2].rotation;

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

	public void chooseCardsToExchange(PlayerScript otherPlayer)
	{
		Debug.Log($"Other player index: {otherPlayer.id}");
		clickedObjects.Clear();
		choosingList.Clear();
		cardLimit = 2;
		okButton.enabled = false;
		finishedChoosing = false;
		choosingList.AddRange(otherPlayer.first3Cards);
		choosingList.AddRange(otherPlayer.second3Cards);
		playerCamera.transform.SetPositionAndRotation(otherPlayer.AboveCardsCameraPos.position, otherPlayer.AboveCardsCameraPos.rotation);
		m_Phase = phases.choosing;

		okButton.onClick.AddListener(() => {
			//Debug.Log("Clicked size: " + clickedObjects.Count);

			OnHoverExit(clickedObjects[0]);
			OnHoverExit(clickedObjects[1]);
			SetCamera();
			m_Phase = phases.Null;
			finishedChoosing = true;
		});
	}

	public void PrepareChooseCards(bool Internal = true)
	{
		if (Internal)
		{
			playerCamera.transform.SetPositionAndRotation(AboveCardsCameraPos.position, AboveCardsCameraPos.rotation);
			okButton.enabled = false;
			choosingList.AddRange(first3Cards);
			choosingList.AddRange(second3Cards);
			cardLimit = 3;
			finishedChoosing = false;
			m_Phase = phases.choosing;
		}
		Debug.Log("Button is " + ((okButton == null) ? "" : "not ") + "null");
		okButton.onClick.AddListener(() => {
			Debug.Log("Listener triggered");
			int index;

			Debug.Log("Clicked size: " + clickedObjects.Count);
			for (int i = 0; i < clickedObjects.Count; i++)
			{
				index = first3Cards.IndexOf(clickedObjects[i]);
				switch (index)
				{
					case -1:
						index = second3Cards.IndexOf(clickedObjects[i]);
						GiveCardAtHand(second3Cards[index]);
						second3Cards.RemoveAt(index);
						//Debug.Log("Player: Second3Cards");
						break;
					default:
						GiveCardAtHand(first3Cards[index]);
						first3Cards.RemoveAt(index);
						//Debug.Log("Player: first3Cards");
						break;
				}
			}

			clickedObjects.Clear();
			second3Cards.AddRange(first3Cards);
			Debug.Log("Second hand size: " + second3Cards.Count);
			for (int i = 0; i < second3Cards.Count; i++)
			{
				second3Cards[i].transform.SetPositionAndRotation(downCardsTransforms[1][i].position, downCardsTransforms[1][i].rotation);
			}
			first3Cards.Clear();
			OnHoverExit(cardsAtHand[0]);
			OnHoverExit(cardsAtHand[1]);
			OnHoverExit(cardsAtHand[2]);
			if (Internal) SetCamera();
			finishedChoosing = true;
		});
		//Debug.Log($"Event listener count: {okButton.onClick.GetPersistentEventCount()}");
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		Debug.Log("Player Started");
		float x = transform.rotation.x, y = transform.rotation.y;
		transform.LookAt(new Vector3(transform.parent.transform.position.x, transform.position.y, transform.parent.transform.position.z));
		//Debug.Log(transform.rotation.x.ToString() + " " + transform.rotation.y.ToString() + " " + transform.rotation.z.ToString());

		for (int i = 0; i < 3; i++)
		{
			downCardsTransforms[i] = new Transform[3];
			downCardsTransforms[i][0] = downCardsObject.GetChild(i * 3);
			downCardsTransforms[i][1] = downCardsObject.GetChild(i * 3 + 1);
			downCardsTransforms[i][2] = downCardsObject.GetChild(i * 3 + 2);
		}
		third3Cards = new List<CardData>();
		second3Cards = new List<CardData>();
		first3Cards = new List<CardData>();

		BehindCameraPos = transform.GetChild(0);
		AboveCardsCameraPos = downCardsObject.GetChild(downCardsObject.childCount - 1);
		RankOnTop = 0;
		cardLimit = 0;
		finishedChoosing = false;
	}

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

		if (okButton != null) 
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
				HoverCards(cardLimit);
				break;
			case phases.playing:
				if (last3)
				{
					HoverCards(cardLimit);
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

	internal void Play(bool Internal = true)
	{

		if (cardsAtHand.Count == 0 && deckFinished && second3Cards.Count == 0)
		{
			choosingList = third3Cards;
			cardLimit = 1;
			last3 = true;
		}
		else if (cardsAtHand.Count > 0) 
		{ 
			choosingList = cardsAtHand;
			cardLimit = 4;
		}

		//Debug.Log("Play");
		if (Internal)
			m_Phase = phases.playing;

		finishedChoosing = false;
		finishedPlaying = false;
		okButton.onClick.AddListener(() =>
			{
				if (last3)
				{
					GiveCardAtHand(clickedObjects[0]);
					OnHoverExit(clickedObjects[0], true);
					third3Cards.Remove(clickedObjects[0]);
					choosingList = cardsAtHand;

					//string log = "[";
					//clickedObjects.ForEach(i => log += i.Id + ", ");
					//log += "]";
					//Debug.Log($"Clicked: {log} | {Time.frameCount}");

					clickedObjects.Clear();
					last3 = false;
					finishedPlaying = false;
					finishedChoosing = true;
				}
				else
				{
					for (int i = 0; i < clickedObjects.Count; i++)
					{
						OnHoverExit(clickedObjects[i], true);
						cardsAtHand.Remove(clickedObjects[i]);
					}

					if (cardsAtHand.Count == 0 && deckFinished && second3Cards.Count > 0)
					{
						for (int i = 0; i < 3; i++)
						{
							GiveCardAtHand(second3Cards[i]);
						}
						second3Cards.Clear();
					}

					finishedChoosing = false;
					finishedPlaying = true;
					//Debug.Log(finishedPlaying);
				}

				Out = third3Cards.Count == 0 && cardsAtHand.Count == 0;
				arranged = false;
				ArrangeCardsAtHand();
			});
	}

	internal void addClickedCard(CardIds cardId, List<CardIds> _choosingList = null)
	{
		CardData card;
		if (_choosingList == null) 
			card = choosingList.Find(x => x.Id == cardId);
		else
		{
			card = first3Cards.Find(x => x.Id == cardId);
			if (card == null)
				card = second3Cards.Find(x=> x.Id == cardId);
		}
		if(card == null) Debug.LogError($"Card not found: {cardId}");
		clickedObjects.Add(card);
	}

	internal int[] ClickedObjectsIdsAsInts()
	{
		List<int> result = new List<int>();

		for (int i = 0; i < clickedObjects.Count; i++) 
		{
			result.Add((int)clickedObjects[i].Id);
		}

		return result.ToArray();
	}

	internal int[] CardsAtHandIdsAsInts()
	{
		List<int> result = new List<int>();

		for (int i = 0; i < cardsAtHand.Count; i++)
		{
			result.Add((int)cardsAtHand[i].Id);
		}

		return result.ToArray();
	}

	internal void SetCamera()
	{
		playerCamera.transform.SetPositionAndRotation(BehindCameraPos.position, BehindCameraPos.rotation);
	}

	public Tuple<int, int> getDownCardPos(CardIds card)
	{
		int index = first3Cards.FindIndex(x => x.Id == card);
		if (index != -1)
			return Tuple.Create(0, index);

		index = second3Cards.FindIndex(x => x.Id == card);
		if (index != -1)
			return Tuple.Create(1, index);

		return null;
	}

	public CardData getDownCard(int yIndex, int xIndex)
	{
		if (yIndex > 2 || yIndex < 0) return null;

		if (yIndex == 0)
			return first3Cards[xIndex];
		else
			return second3Cards[xIndex];
	}

	public void setDownCard(CardData card, int yIndex, int xIndex)
	{
		if (yIndex > 1 || yIndex < 0) return;

		if (yIndex == 0)
			first3Cards[xIndex] = card;
		else
			second3Cards[xIndex] = card;

		card.transform.position = downCardsTransforms[yIndex][xIndex].position;
		card.transform.rotation = downCardsTransforms[yIndex][xIndex].rotation;
	}
}
