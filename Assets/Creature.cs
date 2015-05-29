using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Creature : MonoBehaviour 
{
	[SerializeField]
	private PlayField playField;
	[SerializeField]
	private float maxMoveSpeed;
	[SerializeField]
	private float maxTurnSpeed;
	[SerializeField]
	private float energyDepletionRate;
	[SerializeField]
	private Food food;

	private List<Creature> otherCreatures;

	public float Energy {get; private set;}
	public float Age{get; private set;}
	public Vector2 Position{
		get{
			return new Vector2(transform.position.x, transform.position.z);
		}
	}
	public float Heading{
		get{
			return transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
		}
	}

	public Brain Brain {get; private set;}

	void Awake()
	{
		Energy = 1f;
		otherCreatures = new List<Creature>();
		Creature[] allCreatures = FindObjectsOfType<Creature>();
		Brain = new Brain(otherCreatures.Count);
		foreach(Creature creature in allCreatures)
		{
			if(creature != this)
				otherCreatures.Add(creature);
		}
	}

	void Update()
	{
		//Debug.DrawRay(transform.position, (transform.TransformDirection(Vector3.forward) * 10), Color.green);

		Energy -= energyDepletionRate * Time.deltaTime;
		gameObject.renderer.material.color = new Color(1f - Energy, Energy, 0f);
		if(Energy <= 0f)
			die ();
		if(!playField.IsWithinBorders(Position))
			die ();

		List<Peer> peers =  new List<Peer>();
		foreach(Creature creature in otherCreatures)
		{
			peers.Add(observePeer(creature));
		}

		float foodBearing = getBearingToObject(food.transform);

		Decision thisDecision = Brain.Decide(Position, Heading, playField, Energy, Age, foodBearing, Vector3.Distance(transform.position, food.transform.position), food.Energy, peers);

		transform.Rotate(Vector3.up * thisDecision.TurnEffort * maxTurnSpeed * Time.deltaTime);
		transform.Translate(Vector3.forward * thisDecision.MoveEffort * maxMoveSpeed * Time.deltaTime);

		Energy -= energyDepletionRate * Time.deltaTime * Mathf.Abs(thisDecision.MoveEffort);
		Energy -= energyDepletionRate * Time.deltaTime * Mathf.Abs(thisDecision.TurnEffort);
	}

	void OnTriggerStay(Collider collider)
	{
		if(collider.gameObject.GetComponent<Food>() == food)
			Energy += food.Eat();
	}

	private Peer observePeer(Creature creature)
	{
		Peer newPeer = new Peer();
		newPeer.Heading = creature.Heading;
		newPeer.Bearing = getBearingToObject(creature.transform);
		newPeer.Distance = Vector3.Distance(transform.position, creature.transform.position);
		newPeer.Energy = creature.Energy;
		return newPeer;
	}

	private float getBearingToObject(Transform target)
	{
		float angle = (Vector3.Angle(transform.forward, (target.position - transform.position))) / 180f;
		if(Vector3.Cross(transform.forward, (target.position - transform.position)).y < 0)
			angle *= -1;
		return angle;
	}

	private void die()
	{

		Energy = 1f;
		Vector2 randomPosition = playField.GetRandomPosition();
		transform.position = new Vector3(randomPosition.x, 0, randomPosition.y);
		transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

		Creature father = selectParent();
		Creature mother = selectParent();
//		while(father == mother)
//		{
//			mother = selectParent();
//		}
		//this.Brain.SpawnBrainFromParents(father.Brain, mother.Brain);
	}

	private Creature selectParent()
	{
		float totalEnergy = 0f;
		foreach(Creature eligibleParent in otherCreatures)
		{
			totalEnergy += eligibleParent.Energy;
		}

		float randomSelection = UnityEngine.Random.Range(0f, totalEnergy);
		totalEnergy = 0f;
		foreach(Creature eligibleParent in otherCreatures)
		{
			if(totalEnergy < randomSelection && totalEnergy + eligibleParent.Energy >= randomSelection)
				return eligibleParent;
			totalEnergy += eligibleParent.Energy;
		}
		Debug.LogError("Something Went Wrong Selecting a Parent");
		return null;
	}
}