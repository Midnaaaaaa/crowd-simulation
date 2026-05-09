using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PathFinding;

public class GridHeuristic : Heuristic<GridCell>
{
	// Class that represents a Heuristic function to estimate the cost of going from 
	// one GridCell to another

	
	// constructor takes a goal node for estimating
	public GridHeuristic(GridCell goal):base(goal){
		goalNode = goal;
	}
	
	 // generates an estimated cost to reach the stored goal from the given node
	public override float estimateCost(GridCell fromNode){
		return Vector3.Distance(goalNode.getCenter(), fromNode.getCenter()) + fromNode.getCount();// TO IMPLEMENT
	}

    public override float estimateDistance(GridCell node1, GridCell node2)
    {
        return Vector3.Distance(node1.getCenter(), node2.getCenter()) + node1.getCount() + node2.getCount();
    }

	// determines if the goal node has been reached by node
	public override bool goalReached(GridCell node){
		return node.id == goalNode.id;
    }

};
