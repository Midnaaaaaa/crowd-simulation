using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace PathFinding{

	public class A_Star<TNode,TConnection,TNodeConnection,TGraph,THeuristic> : PathFinder<TNode,TConnection,TNodeConnection,TGraph,THeuristic>
	where TNode : Node
	where TConnection : Connection<TNode>
	where TNodeConnection : NodeConnections<TNode,TConnection>
	where TGraph : Graph<TNode,TConnection,TNodeConnection>
	where THeuristic : Heuristic<TNode>
	{
	// Class that implements the A* pathfinding algorithm
	// You have to implement the findpath function.
	// You can add whatever you need.
				
		protected List<TNode> visitedNodes; // list of visited nodes 
		
		protected NodeRecord currentBest; // current best node found
		
		protected enum NodeRecordCategory{ OPEN, CLOSED, UNVISITED };
				
		protected class NodeRecord{	
		// You can use (or not) this structure to keep track of the information that we need for each node
			
			public NodeRecord(){}
			
			public TNode node; 
			public NodeRecord connection;	// connection traversed to reach this node 
			public float costSoFar; // cost accumulated to reach this node
			public float estimatedTotalCost; // estimated total cost to reach the goal from this node
			public NodeRecordCategory category; // category of the node: open, closed or unvisited
			public int depth; // depth in the search graph
		};

		public	A_Star(int maxNodes, float maxTime, int maxDepth):base(){ 
			
			visitedNodes = new List<TNode> ();
			
		}

		public virtual List<TNode> getVisitedNodes(){
			return visitedNodes;
		}
		
		public override List<TNode> findpath(TGraph graph, TNode start, TNode end, THeuristic heuristic, ref int found)
		{
			List<TNode> path = new List<TNode>();

			if (start.getId() == end.getId())
			{
				found = 1;
				return path;
            }

            Dictionary<int, NodeRecord> allRecords = new Dictionary<int, NodeRecord>();
            PriorityQueue<NodeRecord, float> open = new PriorityQueue<NodeRecord, float>();
			
            NodeRecord startRecord = new NodeRecord{ node = start, connection = null, costSoFar = 0, estimatedTotalCost = heuristic.estimateCost(start), category = NodeRecordCategory.OPEN };
			allRecords[start.getId()] = startRecord;
            open.Enqueue(startRecord, startRecord.estimatedTotalCost);

            while (open.Count > 0)
            {
				NodeRecord currentNode = open.Dequeue();

                if (currentNode.category == NodeRecordCategory.CLOSED)
                    continue;

                if (currentNode.node.getId() == end.getId())
                {
                    found = 1;
                    break;
                }

				currentNode.category = NodeRecordCategory.CLOSED;
				visitedNodes.Add(currentNode.node);

                TNodeConnection connections = graph.getConnections(currentNode.node);
				for(int i = 0; i < connections.Count(); i++)
				{
					TConnection connection = connections.ElementAt(i);
					TNode neighborNode = connection.getToNode();

					float neighborCostSoFar = currentNode.costSoFar + connection.getCost();

					NodeRecord neighborNodeRecord;

					if (allRecords.TryGetValue(neighborNode.getId(), out neighborNodeRecord))
					{
						if (neighborNodeRecord.category == NodeRecordCategory.CLOSED) continue;
						if (neighborCostSoFar >= neighborNodeRecord.costSoFar) continue;

						neighborNodeRecord.costSoFar = neighborCostSoFar;
						neighborNodeRecord.connection = currentNode;
						neighborNodeRecord.estimatedTotalCost = neighborCostSoFar + heuristic.estimateCost(neighborNode);
						open.Enqueue(neighborNodeRecord, neighborNodeRecord.estimatedTotalCost);
                    }

                    else
					{
						neighborNodeRecord = new NodeRecord
						{
							node = neighborNode,
							connection = currentNode,
							costSoFar = neighborCostSoFar,
							estimatedTotalCost = neighborCostSoFar + heuristic.estimateCost(neighborNode),
							category = NodeRecordCategory.OPEN
						};
						allRecords[neighborNode.getId()] = neighborNodeRecord;
						open.Enqueue(neighborNodeRecord, neighborNodeRecord.estimatedTotalCost);
                    }
                }
            }

			if(found != 1) return path;
            //Path reconstruction
            TNode current = end;
			while (current.getId() != start.getId())
			{
				path.Add(current);
				int index = current.getId();
                NodeRecord record = allRecords[index];
				current = record.connection.node;
            }
			path.Reverse();


            return path;
		}

	};

}