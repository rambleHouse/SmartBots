using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Brain
{
//	standardInputNeurons[0] = position X
//	standardInputNeurons[1] = position Y
//	standardInputNeurons[2] = heading
//	standardInputNeurons[3] = north border
//	standardInputNeurons[4] = south border
//	standardInputNeurons[5] = east border
//	standardInputNeurons[6] = west border
//	standardInputNeurons[7] = energy
//	standardInputNeurons[8] = age
//	standardInputNeurons[9] = food bearing
//	standardInputNeurons[10] = food distance
//	standardInputNeurons[11] = food energy

	public List<Neuron> StandardInputNeurons{get; set;}

	public List<PeerInputNeuronSet> PeerInputNeurons{get; set;}

	private Neuron TurnEffortOutputNeuron = new Neuron();
	private Neuron MoveEffortOutputNeuron = new Neuron();

	public Brain(int numberOfPeers)
	{
		int inputNodeCount = 12 + (numberOfPeers * 4); //there are 12 standard input nodes

		StandardInputNeurons = new List<Neuron>();
		PeerInputNeurons = new List<PeerInputNeuronSet>();

		for(int i = 0; i < 12; i++)
		{
			Neuron newNeuron = new Neuron();
			newNeuron.AddConnection(TurnEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newNeuron.AddConnection(MoveEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			StandardInputNeurons.Add(newNeuron);
		}

		for(int i = 0; i < numberOfPeers; i++)
		{
			PeerInputNeuronSet newSet = new PeerInputNeuronSet();
			newSet.HeadingInputNeuron.AddConnection(TurnEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.HeadingInputNeuron.AddConnection(MoveEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.BearingInputNeuron.AddConnection(TurnEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.BearingInputNeuron.AddConnection(MoveEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.DistanceInputNeuron.AddConnection(TurnEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.DistanceInputNeuron.AddConnection(MoveEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.EnergyInputNeuron.AddConnection(TurnEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			newSet.EnergyInputNeuron.AddConnection(MoveEffortOutputNeuron, UnityEngine.Random.Range(-(float)inputNodeCount, (float)inputNodeCount));
			PeerInputNeurons.Add(newSet);
		}
	}

	public void SpawnBrainFromParents(Brain fatherBrain, Brain motherBrain)
	{
		for(int i = 0; i <= 12; i++)
		{
			StandardInputNeurons[i].MergeWeights(fatherBrain.StandardInputNeurons[i], motherBrain.StandardInputNeurons[i]);
		}

		for(int i = 0; i < PeerInputNeurons.Count; i++)
		{
			PeerInputNeurons[i].MergeNeuronSets(fatherBrain.PeerInputNeurons[i], motherBrain.PeerInputNeurons[i]);
		}
	}

	public Decision Decide(Vector2 position, float heading, PlayField playField, float energy, float age, float foodBearing, float foodDistance, float foodEnergy, List<Peer> peers)
	{
		StandardInputNeurons[0].SubmitInput(HTan.getHTan(position.x));
		StandardInputNeurons[1].SubmitInput(HTan.getHTan(position.y));
		StandardInputNeurons[2].SubmitInput(HTan.getHTan(heading));
		StandardInputNeurons[3].SubmitInput(HTan.getHTan(playField.NorthBorder));
		StandardInputNeurons[4].SubmitInput(HTan.getHTan(playField.SouthBorder));
		StandardInputNeurons[5].SubmitInput(HTan.getHTan(playField.EastBorder));
		StandardInputNeurons[6].SubmitInput(HTan.getHTan(playField.WestBorder));
		StandardInputNeurons[7].SubmitInput(HTan.getHTan(energy));
		StandardInputNeurons[8].SubmitInput(HTan.getHTan(age));
		StandardInputNeurons[9].SubmitInput(HTan.getHTan(foodBearing));
		StandardInputNeurons[10].SubmitInput(HTan.getHTan(foodDistance));
		StandardInputNeurons[11].SubmitInput(HTan.getHTan(foodEnergy));

		for(int i = 0; i < PeerInputNeurons.Count; i++)
		{
			PeerInputNeurons[1].HeadingInputNeuron.SubmitInput(HTan.getHTan(peers[i].Heading));
			PeerInputNeurons[1].BearingInputNeuron.SubmitInput(HTan.getHTan(peers[i].Bearing));
			PeerInputNeurons[1].DistanceInputNeuron.SubmitInput(HTan.getHTan(peers[i].Distance));
			PeerInputNeurons[1].EnergyInputNeuron.SubmitInput(HTan.getHTan(peers[i].Energy));
		}

		foreach(Neuron neuron in StandardInputNeurons)
		{
			neuron.SendOutput();
		}

		foreach(PeerInputNeuronSet neuronSet in PeerInputNeurons)
		{
			neuronSet.HeadingInputNeuron.SendOutput();
			neuronSet.BearingInputNeuron.SendOutput();
			neuronSet.DistanceInputNeuron.SendOutput();
			neuronSet.EnergyInputNeuron.SendOutput();
		}

		return new Decision(HTan.getHTan(TurnEffortOutputNeuron.ExtractOutput()), HTan.getHTan(MoveEffortOutputNeuron.ExtractOutput()));
	}
}

public class Decision
{
	public float TurnEffort;
	public float MoveEffort;
	
	public Decision()
	{
		TurnEffort = UnityEngine.Random.Range(-1f, 1f);
		MoveEffort = UnityEngine.Random.Range(-1f, 1f);
	}

	public Decision(float turnEffort, float moveEffort)
	{
		this.TurnEffort = turnEffort;
		this.MoveEffort = moveEffort;
	}
}

public class Peer
{
	public float Heading;
	public float Bearing;
	public float Distance;
	public float Energy;
}

public class PeerInputNeuronSet
{
	public Neuron HeadingInputNeuron = new Neuron();
	public Neuron BearingInputNeuron = new Neuron();
	public Neuron DistanceInputNeuron = new Neuron();
	public Neuron EnergyInputNeuron = new Neuron();

	public void MergeNeuronSets(PeerInputNeuronSet fatherNeuronSet, PeerInputNeuronSet motherNeuronSet)
	{
		HeadingInputNeuron.MergeWeights(fatherNeuronSet.HeadingInputNeuron, motherNeuronSet.HeadingInputNeuron);
		BearingInputNeuron.MergeWeights (fatherNeuronSet.BearingInputNeuron, motherNeuronSet.BearingInputNeuron);
		DistanceInputNeuron.MergeWeights(fatherNeuronSet.DistanceInputNeuron, motherNeuronSet.DistanceInputNeuron);
		EnergyInputNeuron.MergeWeights(fatherNeuronSet.EnergyInputNeuron, motherNeuronSet.EnergyInputNeuron);
	}
}

public class Neuron
{
	private float inputSum;
	private List<NeuronConnection> neuronConnections;

	private class NeuronConnection
	{
		public Neuron OutputNeuron;
		public float Weight;

		public NeuronConnection(Neuron outputNeuron, float weight)
		{
			this.OutputNeuron = outputNeuron;
			this.Weight = weight;
		}
	}

	public Neuron()
	{
		inputSum = 0f;
		neuronConnections = new List<NeuronConnection>();
	}

	public void AddConnection(Neuron OutputNeuron, float Weight)
	{
		neuronConnections.Add(new NeuronConnection(OutputNeuron, Weight));
	}

	public void SubmitInput(float input)
	{
		inputSum += input;
	}

	public void SendOutput()
	{
		foreach(NeuronConnection connection in neuronConnections)
		{
			connection.OutputNeuron.SubmitInput(HTan.getHTan(inputSum) * connection.Weight);
		}
		inputSum = 0f;
	}

	public float ExtractOutput()
	{
		float temp = inputSum;
		inputSum = 0f;
		return temp;
	}

	public void MergeWeights(Neuron fatherNeuron, Neuron motherNeuron)
	{
		for(int i = 0; i < neuronConnections.Count; i++)
		{
			neuronConnections[i].Weight = (fatherNeuron.neuronConnections[i].Weight + motherNeuron.neuronConnections[i].Weight) / 2;
		}
	}
}

public static class HTan
{
	private static float e = 2.71828f;

	public static float getHTan(float x)
	{
		float sin = (Mathf.Pow(2.71828f, x) - Mathf.Pow(2.71828f, -x)) * .5f;
		float cos = (Mathf.Pow(2.71828f, x) + Mathf.Pow(2.71828f, -x)) * .5f;
		return (sin / cos);
	}
}
