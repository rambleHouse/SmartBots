using UnityEngine;
using System.Collections;

public class PlayField : MonoBehaviour 
{
	public float NorthBorder;
	public float SouthBorder;
	public float EastBorder;
	public float WestBorder;
	
	public Vector2 GetRandomPosition()
	{
		return new Vector2(Random.Range(EastBorder, WestBorder), Random.Range(SouthBorder, NorthBorder));
	}

	public bool IsWithinBorders(Vector2 position)
	{
		return position.x > EastBorder && position.x < WestBorder && position.y > SouthBorder && position.y < NorthBorder;
	}
}