using UnityEngine;

public class CardData : MonoBehaviour
{
	GameObject m_CardModel;
	public CardIds Id;
	public int Rank, power;
	public string debugName;
	public Renderer m_Renderer;
	public BoxCollider m_Collider;
	public MeshFilter m_MeshFilter;
	public void Set(CardIds p_Id)
	{
		gameObject.layer = 6;
		if (p_Id == CardIds.Null)
			return;

		Id = p_Id;
		Rank = (((int)p_Id) % 13);
		Rank += (Rank == 0) ? 13 : 0;
		switch (Rank)
		{
			case 1:
				power = 14;
				break;
			case 2: case 4: case 10:
				power = 15; 
				break;
			default:
				power = Rank;
				break;
		}
		debugName = p_Id.ToString();
		m_CardModel = Resources.Load("Card_" + p_Id.ToString()) as GameObject;
		m_CardModel = Instantiate(m_CardModel, transform.position, transform.rotation, transform);
		m_CardModel.transform.SetParent(transform, false);
		m_Renderer = m_CardModel.GetComponent<Renderer>();
		m_Collider = m_CardModel.GetComponent<BoxCollider>();
		m_MeshFilter = m_CardModel.GetComponent<MeshFilter>();
	}
}
