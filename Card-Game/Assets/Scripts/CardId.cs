using System;
using UnityEngine;

public class CardId : MonoBehaviour
{
	public static int CalculatePower(int Rank)
	{
		int power;
		switch (Rank)
		{
			case 1:
				power = 14;
				break;
			case 2:
			case 4:
			case 10:
				power = 15;
				break;
			default:
				power = Rank;
				break;
		}
		return power;
	}
}

public enum CardIds : int
{
	Null, ClubAce, Club2, Club3, Club4, Club5, Club6, Club7, Club8, Club9, Club10, ClubJack, ClubQueen, ClubKing,
	DiamondAce, Diamond2, Diamond3, Diamond4, Diamond5, Diamond6, Diamond7, Diamond8, Diamond9, Diamond10, DiamondJack, DiamondQueen, DiamondKing,
	heartAce, Heart2, Heart3, Heart4, Heart5, Heart6, Heart7, Heart8, Heart9, Heart10, HeartJack, HeartQueen, HeartKing,
	spadeAce, Spade2, Spade3, Spade4, Spade5, Spade6, Spade7, Spade8, Spade9, Spade10, SpadeJack, SpadeQueen, SpadeKing
}
