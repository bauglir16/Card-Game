using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
	public List<CardData> cardsAtHand = new List<CardData>();
	public CardData[] first3Cards;
	public CardData[] second3Cards;
	public CardData[] third3Cards;
	public Transform[][] downCards = new Transform[3][];
	List<CardData> clickedObjects = new List<CardData>();
	Transform m_HandPivot;
	Vector3 m_ColliderSizeVector;
	Vector3 m_RendererSizeVector;
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
	public bool ChoosingCards;

	void ArrangeCardsAtHand()
	{
		if (cardsAtHand.Count == 0) return;

		cardsAtHand.Sort((x, y) => x.Rank - y.Rank);

		m_ColliderSizeVector = transform.InverseTransformVector(cardsAtHand[0].transform.GetChild(0).GetComponent<BoxCollider>().bounds.size);
		m_RendererSizeVector = cardsAtHand[0].transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size;
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

			cardsAtHand[sideCardsNumber].transform.LookAt(new Vector3(cardsAtHand[sideCardsNumber].transform.position.x, transform.position.y + playerHeadHight, transform.position.z));
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
	}

	public void GiveFirst3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCards[2][0].transform.position;
		card1.transform.rotation = downCards[2][0].transform.rotation;
		card2.transform.position = downCards[2][1].transform.position;
		card2.transform.rotation = downCards[2][1].transform.rotation;
		card3.transform.position = downCards[2][2].transform.position;
		card3.transform.rotation = downCards[2][2].transform.rotation;

		third3Cards[0] = card1;
		third3Cards[1] = card2;
		third3Cards[2] = card3;

		Debug.Log("GiveFirst3Cards");
	}

	public void GiveSecond3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCards[1][0].transform.position;
		card1.transform.rotation = downCards[1][0].transform.rotation;
		card2.transform.position = downCards[1][1].transform.position;
		card2.transform.rotation = downCards[1][1].transform.rotation;
		card3.transform.position = downCards[1][2].transform.position;
		card3.transform.rotation = downCards[1][2].transform.rotation;

		second3Cards[0] = card1;
		second3Cards[1] = card2;
		second3Cards[2] = card3;
	}

	public void GiveThird3Cards(CardData card1, CardData card2, CardData card3)
	{
		card1.transform.position = downCards[0][0].transform.position;
		card1.transform.rotation = downCards[0][0].transform.rotation;
		card2.transform.position = downCards[0][1].transform.position;
		card2.transform.rotation = downCards[0][1].transform.rotation;
		card3.transform.position = downCards[0][2].transform.position;
		card3.transform.rotation = downCards[0][2].transform.rotation;

		first3Cards[0] = card1;
		first3Cards[1] = card2;
		first3Cards[2] = card3;
	}

	public void GiveCardAtHand(CardData card)
	{
		cardsAtHand.Add(card);
		ArrangeCardsAtHand();
	}

	//public CardIds[] ChooseCards()
	//{
		
	//}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		float x = transform.rotation.x, y = transform.rotation.y;
		transform.LookAt(new Vector3(transform.parent.transform.position.x, transform.position.y, transform.parent.transform.position.z));
		Debug.Log(transform.rotation.x.ToString() + " " + transform.rotation.y.ToString() + " " + transform.rotation.z.ToString());

		for (int i = 0; i < 3; i++)
		{
			downCards[i] = new Transform[3];
			downCards[i][0] = transform.GetChild(i * 3);
			downCards[i][1] = transform.GetChild(i * 3 + 1);
			downCards[i][2] = transform.GetChild(i * 3 + 2);
		}
		third3Cards = new CardData[3];
		second3Cards = new CardData[3];
		first3Cards = new CardData[3];

		BehindCameraPos = transform.GetChild(9);
		AboveCardsCameraPos = transform.GetChild(10);
	}

	void HoverCards()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			CardData hoveredObject = hit.collider.gameObject.transform.parent.GetComponent<CardData>();
			if (cardsAtHand.Contains(hoveredObject))
			{

				if (Input.GetMouseButtonDown(0))
				{
					if (!clickedObjects.Contains(hoveredObject)) clickedObjects.Add(hoveredObject);
					else clickedObjects.Remove(hoveredObject);
				}

				if (hoveredObject != lastHoveredObject)
				{
					// Handle leaving the last hovered object
					if (lastHoveredObject != null && !clickedObjects.Contains(lastHoveredObject))
					{
						OnHoverExit(lastHoveredObject.transform.GetChild(0).gameObject);
					}

					// Handle entering the new hovered object
					OnHoverEnter(hoveredObject.transform.GetChild(0).gameObject);
					lastHoveredObject = hoveredObject;
				}
			}
		}
		else
		{
			// If no object is hovered, handle exiting the last hovered object
			if (lastHoveredObject != null)
			{
				OnHoverExit(lastHoveredObject.transform.GetChild(0).gameObject);
				lastHoveredObject = null;
			}
		}
	}

	public void Try()
	{
		CardData temp = third3Cards[0];
		third3Cards[0] = null;
		GiveCardAtHand(temp);
		temp = third3Cards[1];
		third3Cards[1] = null;
		GiveCardAtHand(temp);
		temp = third3Cards[2];
		third3Cards[2] = null;
		GiveCardAtHand(temp);
	}

	// Update is called once per frame
	void Update()
	{
		debugCardSize = cardsAtHand.Count;
		right = transform.right;
		rotation = transform.rotation.eulerAngles;

		ArrangeCardsAtHand();
		HoverCards();
		
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

	void OnHoverEnter(GameObject obj)
	{
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer != null)
		{
			renderer.material.color = Color.cyan; // Highlight color
		}
	}

	void OnHoverExit(GameObject obj)
	{
		Renderer renderer = obj.GetComponent<Renderer>();
		if (renderer != null)
		{
			renderer.material.color = Color.white; // Original color
		}
	}
}
