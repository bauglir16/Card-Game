using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
	public List<GameObject> cards = new List<GameObject>();
	public GameObject[] first3Cards;
	public GameObject[] second3Cards;
	public GameObject[] third3Cards;
	List<GameObject> clickedObjects = new List<GameObject>();
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
	Mouse m_Mouse = Mouse.current;
	public Camera playerCamera; // Assign the player's camera in the Inspector
	private GameObject lastHoveredObject;

	void ArrangeCardsAtHand()
	{
		if (cards.Count == 0) return;


		m_ColliderSizeVector = transform.InverseTransformVector(cards[0].GetComponent<BoxCollider>().bounds.size);
		m_RendererSizeVector = cards[0].GetComponent<MeshFilter>().mesh.bounds.size;
		float xDistanceOfCards = m_ColliderSizeVector.x;
		float zDistanceOfCards = m_RendererSizeVector.z;
		float xOffestMultiplier = 0.5f;
		sideCardsNumber = cards.Count / 2;

		if ((cards.Count - 1) * Mathf.Abs(xDistanceOfCards) > handRightRadius * 2)
		{
			xDistanceOfCards = handRightRadius * 2 / cards.Count;
		}
		debugDistanceOfCards = xDistanceOfCards;

		if (cards.Count % 2 != 0) //Odd number of cards
		{
			cards[sideCardsNumber].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);

			cards[sideCardsNumber].transform.LookAt(new Vector3(cards[sideCardsNumber].transform.position.x, transform.position.y + playerHeadHight, transform.position.z));
			cards[sideCardsNumber].transform.position = transform.position + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * sideCardsNumber);
			if (cards.Count == 1) return;
			xOffestMultiplier = 1;
		}

		cards[sideCardsNumber - 1].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
		cards[cards.Count - sideCardsNumber].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);

		toTheRight = transform.right * (xDistanceOfCards * xOffestMultiplier);
		cards[sideCardsNumber - 1].transform.position = transform.position - toTheRight + transform.forward * (handForwardRadius - (zDistanceOfCards * 3 * (sideCardsNumber - 1)));
		cards[cards.Count - sideCardsNumber].transform.position = transform.position + toTheRight + transform.forward * (handForwardRadius - (zDistanceOfCards * 3 * (cards.Count - sideCardsNumber)));
		int iterationCounter = 1;
		for (int i = sideCardsNumber - 2, j = cards.Count - sideCardsNumber + 1; i >= 0 && j < cards.Count; i--, j++)
		{
			cards[i].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
			cards[j].transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
			toTheRight = transform.right * (xDistanceOfCards * iterationCounter + xDistanceOfCards * xOffestMultiplier);
			cards[i].transform.position = transform.position - toTheRight + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * i);
			cards[j].transform.position = transform.position + toTheRight + transform.forward * (handForwardRadius - zDistanceOfCards * 3 * j);
			iterationCounter++;
		}
	}

	public void GiveFirst3Cards(GameObject card1, GameObject card2, GameObject card3)
	{
		card1.transform.position = third3Cards[0].transform.position;
		card1.transform.rotation = third3Cards[0].transform.rotation;
		card2.transform.position = third3Cards[1].transform.position;
		card2.transform.rotation = third3Cards[1].transform.rotation;
		card3.transform.position = third3Cards[2].transform.position;
		card3.transform.rotation = third3Cards[2].transform.rotation;

		third3Cards[0] = card1;
		third3Cards[1] = card2;
		third3Cards[2] = card3;
	}

	public void GiveSecond3Cards(GameObject card1, GameObject card2, GameObject card3)
	{
		card1.transform.position = second3Cards[0].transform.position;
		card1.transform.rotation = second3Cards[0].transform.rotation;
		card2.transform.position = second3Cards[1].transform.position;
		card2.transform.rotation = second3Cards[1].transform.rotation;
		card3.transform.position = second3Cards[2].transform.position;
		card3.transform.rotation = second3Cards[2].transform.rotation;

		second3Cards[0] = card1;
		second3Cards[1] = card2;
		second3Cards[2] = card3;
	}

	public void GiveThird3Cards(GameObject card1, GameObject card2, GameObject card3)
	{
		card1.transform.position = first3Cards[0].transform.position;
		card1.transform.rotation = first3Cards[0].transform.rotation;
		card2.transform.position = first3Cards[1].transform.position;
		card2.transform.rotation = first3Cards[1].transform.rotation;
		card3.transform.position = first3Cards[2].transform.position;
		card3.transform.rotation = first3Cards[2].transform.rotation;

		first3Cards[0] = card1;
		first3Cards[1] = card2;
		first3Cards[2] = card3;
	}

	public void GiveCardAtHand(CardData card)
	{
		//cards.Add(card);
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
			first3Cards[i] = transform.GetChild(i * 3).gameObject;
			second3Cards[i] = transform.GetChild((i + 1) * 3).gameObject;
			third3Cards[i] = transform.GetChild((i + 2) * 3).gameObject;
		}
	}

	// Update is called once per frame
	void Update()
	{
		debugCardSize = cards.Count;
		right = transform.right;
		rotation = transform.rotation.eulerAngles;

		ArrangeCardsAtHand();

		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			GameObject hoveredObject = hit.collider.gameObject;

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
					OnHoverExit(lastHoveredObject);
				}

				// Handle entering the new hovered object
				OnHoverEnter(hoveredObject);
				lastHoveredObject = hoveredObject;
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
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(transform.position + transform.right * handRightRadius, transform.position - transform.right * handRightRadius);
		Gizmos.DrawLine(transform.position + transform.right * handRightRadius + transform.forward * handForwardRadius, transform.position - transform.right * handRightRadius + transform.forward * handForwardRadius);
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
