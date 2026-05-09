using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CrowdGenerator : MonoBehaviour
{
    [SerializeField] private float xMin = 0, xMax = 10, zMin = 0, zMax = 10;
    [SerializeField] private int numberOfAgents = 10;
    [SerializeField] Agent[] agentPrefabs;
    [SerializeField] private int tries = 10;


    public void initDimensions()
    {
        Simulator.instance.SetBounds(xMin, xMax, zMin, zMax);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GenerateCrowdFromGrid(Grid grid)
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            bool overlap = false;
            for (int j = 0; j < tries; j++)
            {
                Vector3 position = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                if(grid.getGridCell(position).isOccupied())
                {
                    continue;
                }

                List<Agent> createdAgents = Simulator.instance.GetAgents();

                overlap = false;
                foreach (Agent other in createdAgents)
                {
                    float combinedRadius = other.GetRadius();
                    if (Vector3.Distance(position, other.transform.position) < combinedRadius)
                    {
                        overlap = true;
                        break;
                    }
                }

                if (!overlap)
                {
                    Agent prefab = agentPrefabs[Random.Range(0, agentPrefabs.Length)];
                    Agent newAgent = Instantiate(prefab, position, rotation);
                    newAgent.GenerateRandomGoal(xMin, xMax, zMin, zMax);
                    Simulator.instance.AddAgent(newAgent);
                    break;
                }
            }

            if(overlap)
            {
                Debug.LogWarning("Could not place agent " + i + " without overlap after " + tries + " tries.");
                break;
            }
        }
    }

    public void GenerateCrowd()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            bool overlap = false;
            for (int j = 0; j < tries; j++)
            {
                Vector3 position = new Vector3(Random.Range(xMin, xMax), 0, Random.Range(zMin, zMax));
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                List<Agent> createdAgents = Simulator.instance.GetAgents();

                overlap = false;
                foreach (Agent other in createdAgents)
                {
                    float combinedRadius = other.GetRadius();
                    if (Vector3.Distance(position, other.transform.position) < combinedRadius)
                    {
                        overlap = true;
                        break;
                    }
                }

                if (!overlap)
                {
                    Agent prefab = agentPrefabs[Random.Range(0, agentPrefabs.Length)];
                    Agent newAgent = Instantiate(prefab, position, rotation);
                    newAgent.GenerateRandomGoal(xMin, xMax, zMin, zMax);
                    Simulator.instance.AddAgent(newAgent);
                    break;
                }
            }

            if (overlap)
            {
                Debug.LogWarning("Could not place agent " + i + " without overlap after " + tries + " tries.");
                break;
            }
        }
    }
}
