using NUnit.Framework.Interfaces;
using PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class Grid : FiniteGraph<GridCell, CellConnection, GridConnections>
{
	// Class that represent the finite graph corresponding to a grid of cells
	// There is a known set of nodes (GridCells), 
	// and a known set of connections (CellConnections) between those nodes (GridConnections)

	protected float xMin;
	protected float xMax;
	protected float zMin;
	protected float zMax;

	protected float gridHeight;

	protected float sizeOfCell;

	protected int numCells;
	protected int numRows;
	protected int numColumns;
	protected float obstacleProbability = 0.2f;


	// Example Constructor function declaration
	public Grid(float minX, float maxX, float minZ, float maxZ, float cellSize, float height = 0, float obsProb = 0.2f) : base()
	{
		xMin = minX;
		xMax = maxX;
		zMin = minZ;
		zMax = maxZ;
		numRows = Mathf.CeilToInt((maxX - minX) / cellSize);
		numColumns = Mathf.CeilToInt((maxZ - minZ) / cellSize);
		numCells = numRows * numColumns;
		sizeOfCell = cellSize;
		gridHeight = height;
		obstacleProbability = obsProb;


        for (int i = 0; i < numCells; i++)
		{
			GridCell cell = new GridCell(i);
			Vector2Int rowCol = indexToRowColumn(i);

            bool isBorder = (rowCol.x == 0 || rowCol.x == numRows - 1 || rowCol.y == 0 || rowCol.y == numColumns - 1);

            if (isBorder || Random.value < obstacleProbability)
            {
                cell.setOccupied(true);
            }

            float cellXMin = xMin + rowCol.x * sizeOfCell;
			float cellXMax = cellXMin + sizeOfCell;
			float cellZMin = zMin + rowCol.y * sizeOfCell;
			float cellZMax = cellZMin + sizeOfCell;
			cell.setBounds(cellXMin, cellXMax, cellZMin, cellZMax);
			nodes.Add(cell);
			connections.Add(new GridConnections());
        }


		for (int i = 0; i < numCells; i++)
		{
			GridCell fromCell = nodes[i];
			Vector2Int rowCol = indexToRowColumn(i);

			// Check all 8 possible directions (left down, down, right down, right)
			//Vector2Int[] directions = new Vector2Int[]{ new Vector2Int(1,-1), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1,1), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1)};
			Vector2Int[] directions = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };

			for (int d = 0; d < directions.Length; d++)
			{
				Vector2Int neighborRowCol = rowCol + directions[d];

				if (isValidCell(neighborRowCol))
				{
					int toIndex = neighborRowCol.x * numColumns + neighborRowCol.y;
					GridCell toCell = nodes[toIndex];
					if (!fromCell.isOccupied() && !toCell.isOccupied())
					{
						CellConnection connection = new CellConnection(fromCell, toCell);
						connections[i].Add(connection);
					}
				}
			}

			Vector2Int[] diagonalDirections = new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(1, 1) };

			for (int d = 0; d < diagonalDirections.Length; ++d)
			{
				Vector2Int neighborRowCol = rowCol + diagonalDirections[d];

				if (!isValidCell(neighborRowCol)) continue;

				int toIndex = neighborRowCol.x * numColumns + neighborRowCol.y;
				GridCell toCell = nodes[toIndex];

				if (fromCell.isOccupied() || toCell.isOccupied()) continue;

				Vector2Int sidePosA = new Vector2Int(rowCol.x + diagonalDirections[d].x, rowCol.y);
				Vector2Int sidePosB = new Vector2Int(rowCol.x, rowCol.y + diagonalDirections[d].y);

				GridCell sideCellA = nodes[sidePosA.x * numColumns + sidePosA.y];
				GridCell sideCellB = nodes[sidePosB.x * numColumns + sidePosB.y];

				if (!sideCellA.isOccupied() && !sideCellB.isOccupied())
				{
					CellConnection connection = new CellConnection(fromCell, toCell);
					connections[i].Add(connection);
				}
			}
		}

		// You have basically to fill the base fields "nodes" and "connections", 
		// i.e. create your list of GridCells (with random obstacles) 
		// and then create the corresponding GridConnections for each one of them
		// based on where the obstacles are and the valid movements allowed between GridCells. 


		// TO IMPLEMENT



	}

	public GridCell getGridCell(Vector3 position)
	{
		Vector2 transformedPosition = new Vector2(position.x - xMin, position.z - zMin);

		if (transformedPosition.x < 0 || transformedPosition.x >= (xMax - xMin) ||
			transformedPosition.y < 0 || transformedPosition.y >= (zMax - zMin))
		{
			return null;
        }

        int row = Mathf.FloorToInt(transformedPosition.x / sizeOfCell);
		int column = Mathf.FloorToInt(transformedPosition.y / sizeOfCell);

		int index = row * numColumns + column;
		return nodes[index];
    }

    public void getBounds(out float xMin, out float xMax, out float zMin, out float zMax)
    {
        xMin = this.xMin;
        xMax = this.xMax;
        zMin = this.zMin;
        zMax = this.zMax;
    }

	public float getGridHeight()
	{
		return gridHeight;

    }

    public float getCellSize()
    {
        return sizeOfCell;
    }

	public void computeGridDensity(List<Agent> agents)
	{
		foreach (GridCell node in nodes)
		{
			node.setCount(0);
		}

		foreach (Agent agent in agents)
		{
			GridCell cell = getGridCell(agent.transform.position);
			if (cell != null)
			{
				cell.incrementCount();
			}
		}
	}

    Vector2Int indexToRowColumn(int index)
	{
		int row = index / numColumns;
		int column = index % numColumns;
		return new Vector2Int(row, column);
    }

    private bool isValidCell(Vector2Int rowCol)
	{
		if (rowCol.x < 0 || rowCol.x >= numRows || rowCol.y < 0 || rowCol.y >= numColumns)
		{
			return false;
		}
		return true;
    }


}
