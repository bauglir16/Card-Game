using TreeEditor;
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
	public Vector3 targetPos = new Vector3(-1, -1, -1);
	public Quaternion targetRot = Quaternion.identity;
	bool linear, moving = false;
	public float speed;

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

	public void SetTargetPositionAndRotation(Vector3 pos, Quaternion rot, bool rotateImmediatly = false, bool linearMovement = false)
	{
		targetRot = rot;
		if(rotateImmediatly) transform.rotation = rot;
		targetPos = pos;
		speed = Vector3.Distance(transform.position, targetPos) * 3;
		linear = linearMovement;
		moving = true;
	}

	public void Update()
	{
		if (!moving) return;
		if (transform.position == targetPos || targetPos.Equals(new Vector3(-1, -1, -1)))
		{
			moving = false;
			return;
		}

		if (linear)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
			return;
		}
		Vector3 dist = targetPos - transform.position;
		Vector3 temp;
		if (dist.x > 0 || dist.z > 0)
			temp = new Vector3(targetPos.x, transform.position.y, targetPos.z);
		else
		{
			temp = targetPos;
			transform.rotation = targetRot;
		}
		transform.position = Vector3.MoveTowards(transform.position, temp, speed * Time.deltaTime);
	}
}
