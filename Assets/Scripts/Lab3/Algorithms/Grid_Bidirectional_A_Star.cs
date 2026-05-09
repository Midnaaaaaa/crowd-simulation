using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PathFinding;

public class Grid_Bidirectional_A_Star : Bidirectional_A_Star<GridCell, CellConnection, GridConnections, Grid, GridHeuristic>
{
	// Class that implements the A* pathfinding algorithm	
	// over a Grid graph, componsed of GridCells and CellConnections
	// using GridHeuristic as the Heuristic function.

	public Grid_Bidirectional_A_Star(int maxNodes, float maxTime, int maxDepth) : base(maxNodes, maxTime, maxDepth)
	{

	}
};
