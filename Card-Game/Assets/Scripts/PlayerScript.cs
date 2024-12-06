using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using static Fusion.Sockets.NetBitBuffer;
using static UnityEngine.Rendering.GPUSort;

public class PlayerScript : MonoBehaviour
{
    public GameObject[] cards;
    public GameObject[] first3Cards;
    public GameObject[] second3Cards;
    public GameObject[] third3Cards;
    public SplineContainer splineContainer;
    public float radius = 5f;   // Radius of the curve
    public float arcAngle = 120f;  // Total angle of the arc in degrees
    public Vector3 centerOffset = Vector3.zero;  // Offset for the curve's center
    public float yOffset = 0f;  // Height of the cards
    public Vector3 splinePosition;

    Transform m_HandPivot;
    float m_ColliderWidthOfCard;
    public float handRightRadius;
    public float handForwardRadius;

    void ArrangeCards()
    {
        if (splineContainer == null || cards.Length == 0) return;

        Spline spline = splineContainer.Spline;

        // Get the total length of the spline
        float splineLength = spline.GetLength();

        // Calculate spacing along the spline
        float step = 1f / (cards.Length - 1); // Normalized position along the spline (0 to 1)

        for (int i = 0; i < cards.Length; i++)
        {
            // Get the position on the spline (normalized value between 0 and 1)
            float t = i * step;

            // Evaluate the spline at position t
            splinePosition = spline.EvaluatePosition(t);

            // Add yOffset to lift the cards off the spline slightly
            splinePosition.y += yOffset;

            // Place the card
            cards[i].transform.position = splinePosition;

            // Orient the card to face forward along the spline
            Vector3 forward = math.normalize (spline.EvaluateTangent(t));
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
            cards[i].transform.rotation = rotation;
        }
    }

    void ArrangeCards2()
    {
        if (cards.Length == 0) return;

        m_ColliderWidthOfCard = cards[0].GetComponent<Collider>().bounds.size.x;
        float distanceOfCards = m_ColliderWidthOfCard;
        float xOffestMultiplier = 0.5f;
        int sideCardsNumber = cards.Length / 2;
        Vector3 zOffset = new Vector3(0, 0, 1.552f);
        //float zOffsetMultiplier = 1f;


        if (cards.Length % 2 == 0) //Even number of cards
        {

        }
        else //Odd number of cards
        {
            cards[sideCardsNumber + 1].transform.position = transform.position + transform.forward * 1.552f;
            cards[sideCardsNumber + 1].transform.LookAt(new Vector3(transform.position.x, cards[sideCardsNumber + 1].transform.position.y, transform.position.z));
            xOffestMultiplier = 1;
        }

        if (cards.Length * m_ColliderWidthOfCard > handRightRadius * 2)
        {
            distanceOfCards = (handRightRadius - m_ColliderWidthOfCard / 2) / cards.Length;
        }

        cards[sideCardsNumber - 1].transform.position = transform.position - transform.right * (distanceOfCards * xOffestMultiplier);
        cards[cards.Length - sideCardsNumber].transform.position = transform.position + transform.right * (distanceOfCards * xOffestMultiplier);
        for (int i = sideCardsNumber - 2, j = cards.Length - 1 - sideCardsNumber; i >= 0 && j < cards.Length; i--, j++)
        {
            cards[i].transform.position = cards[i+1].transform.position - transform.right * (distanceOfCards);
            cards[j].transform.position = cards[j - 1].transform.position + transform.right * (distanceOfCards);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_HandPivot = transform.GetChild(0);

        float x = transform.rotation.x, y = transform.rotation.y;
        transform.LookAt(new Vector3(transform.parent.transform.position.x, transform.position.y, transform.parent.transform.position.z));
        Debug.Log(transform.rotation.x.ToString() + " " + transform.rotation.y.ToString() + " " + transform.rotation.z.ToString());
        

    }

    // Update is called once per frame
    void Update()
    {
        ArrangeCards();
    }
}
