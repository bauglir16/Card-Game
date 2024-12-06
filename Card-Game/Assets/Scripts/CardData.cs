using UnityEngine;

public class CardData : MonoBehaviour
{
    GameObject m_CardModel;
    public CardIds Id;
    public int Rank;
    public void Set(CardIds p_Id)
    {
        Id = p_Id;
        Rank = ((int)p_Id) % 13;
        m_CardModel = Resources.Load("Resources/Card_" + (CardIds.Null + Rank).ToString()) as GameObject;
        m_CardModel.transform.SetParent(transform, false);
    }
}
