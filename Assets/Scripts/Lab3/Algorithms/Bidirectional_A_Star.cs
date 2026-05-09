using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace PathFinding{

	public class Bidirectional_A_Star<TNode,TConnection,TNodeConnection,TGraph,THeuristic> : PathFinder<TNode, TConnection, TNodeConnection, TGraph, THeuristic>
    where TNode : Node
    where TConnection : Connection<TNode>
    where TNodeConnection : NodeConnections<TNode, TConnection>
    where TGraph : Graph<TNode, TConnection, TNodeConnection>
    where THeuristic : Heuristic<TNode>
    {
        protected List<TNode> visitedNodes;
        protected enum NodeRecordCategory { OPEN, CLOSED };

        protected class NodeRecord
        {
            public TNode node;
            public NodeRecord connection;
            public float costSoFar;
            public NodeRecordCategory category;
        }

        protected struct PairCandidate
        {
            public int nodeIdF;
            public int nodeIdB;
            public float priority;
        }

        public Bidirectional_A_Star(int maxNodes, float maxTime, int maxDepth) : base()
        {
            visitedNodes = new List<TNode>();
        }

        public override List<TNode> findpath(TGraph graph, TNode start, TNode end, THeuristic heuristic, ref int found)
        {
            visitedNodes.Clear();
            List<TNode> path = new List<TNode>();

            if (start.getId() == end.getId())
            {
                found = 1;
                return path;
            }

            HashSet<int> openSetF = new HashSet<int>();
            HashSet<int> openSetB = new HashSet<int>();

            Dictionary<int, NodeRecord> recordsF = new Dictionary<int, NodeRecord>();
            Dictionary<int, NodeRecord> recordsB = new Dictionary<int, NodeRecord>();

            PriorityQueue<PairCandidate, float> pairQueue = new PriorityQueue<PairCandidate, float>();

            NodeRecord startRecord = new NodeRecord { node = start, connection = null, costSoFar = 0, category = NodeRecordCategory.OPEN };
            openSetF.Add(start.getId());
            recordsF[start.getId()] = startRecord;

            NodeRecord endRecord = new NodeRecord { node = end, connection = null, costSoFar = 0, category = NodeRecordCategory.OPEN };
            openSetB.Add(end.getId());
            recordsB[end.getId()] = endRecord;

            float initialH = heuristic.estimateDistance(start, end);
            pairQueue.Enqueue(new PairCandidate { nodeIdF = start.getId(), nodeIdB = end.getId(), priority = initialH }, initialH);

            NodeRecord bestF = null;
            NodeRecord bestB = null;

            while (openSetF.Count > 0 && openSetB.Count > 0 && pairQueue.Count > 0)
            {
                PairCandidate bestPair;
                bool validPairFound = false;
                
                while (pairQueue.Count > 0)
                {
                    bestPair = pairQueue.Dequeue();
                    
                    if (openSetF.Contains(bestPair.nodeIdF) && openSetB.Contains(bestPair.nodeIdB))
                    {
                        bestF = recordsF[bestPair.nodeIdF];
                        bestB = recordsB[bestPair.nodeIdB];

                        //We verify if the cost is still valid
                        float currentPriority = bestF.costSoFar + heuristic.estimateDistance(bestF.node, bestB.node) + bestB.costSoFar;
                        if (currentPriority <= bestPair.priority + 0.001f)
                        {
                            validPairFound = true;
                            break;
                        }
                        else
                        {
                            //Cost has changed, reinsert with updated priority
                            pairQueue.Enqueue(new PairCandidate { nodeIdF = bestPair.nodeIdF, nodeIdB = bestPair.nodeIdB, priority = currentPriority }, currentPriority);
                        }
                    }
                }

                if (!validPairFound) break;

                //Check if we have met in the middle
                if (bestF.node.getId() == bestB.node.getId())
                {
                    found = 1;
                    break;
                }

                //We expand the search from the side with fewer nodes in the open set
                if (openSetF.Count <= openSetB.Count)
                {
                    ExpandWithPairQueue(graph, bestF, openSetF, recordsF, openSetB, recordsB, pairQueue, heuristic, ref found, ref bestB, true);
                    if (found == 1) { bestF = recordsF[bestB.node.getId()]; break; }
                }
                else
                {
                    ExpandWithPairQueue(graph, bestB, openSetB, recordsB, openSetF, recordsF, pairQueue, heuristic, ref found, ref bestF, false);
                    if (found == 1) { bestB = recordsB[bestF.node.getId()]; break; }
                }
            }

            if (found == 1)
            {
                NodeRecord curr = bestF;
                while (curr != null)
                {
                    path.Add(curr.node);
                    curr = curr.connection;
                }
                path.Reverse();

                curr = bestB.connection;
                while (curr != null)
                {
                    path.Add(curr.node);
                    curr = curr.connection;
                }
            }

            return path;
        }

        private void ExpandWithPairQueue(TGraph graph, NodeRecord current, HashSet<int> myOpenSet, Dictionary<int, NodeRecord> myRecords, HashSet<int> otherOpenSet, Dictionary<int, NodeRecord> otherRecords, PriorityQueue<PairCandidate, float> pairQueue, THeuristic heuristic, ref int found, ref NodeRecord meetingNodeOther, bool isForward)
        {
            //We close the current node
            myOpenSet.Remove(current.node.getId());
            current.category = NodeRecordCategory.CLOSED;

            TNodeConnection connections = graph.getConnections(current.node);
            for (int i = 0; i < connections.Count(); i++)
            {
                TConnection connection = connections.ElementAt(i);
                TNode neighbor = connection.getToNode();
                int neighborId = neighbor.getId();

                //If the neighbor is closed in the other search, we found a meeting point
                if (otherRecords.ContainsKey(neighborId) && otherRecords[neighborId].category == NodeRecordCategory.CLOSED)
                {
                    meetingNodeOther = otherRecords[neighborId];
                    NodeRecord finalRecord = new NodeRecord
                    {
                        node = neighbor,
                        connection = current,
                        costSoFar = current.costSoFar + connection.getCost(),
                        category = NodeRecordCategory.CLOSED
                    };
                    myRecords[neighborId] = finalRecord;
                    found = 1;
                    return;
                }

                float newCost = current.costSoFar + connection.getCost();
                bool isNewOrBetter = false;

                //Update or create the neighbor in the current frontier
                if (myRecords.TryGetValue(neighborId, out NodeRecord neighborNodeRecord))
                {
                    if (neighborNodeRecord.category == NodeRecordCategory.CLOSED) continue;
                    if (newCost < neighborNodeRecord.costSoFar)
                    {
                        neighborNodeRecord.costSoFar = newCost;
                        neighborNodeRecord.connection = current;
                        if (!myOpenSet.Contains(neighborId))
                        {
                            myOpenSet.Add(neighborId);
                            neighborNodeRecord.category = NodeRecordCategory.OPEN;
                        }
                        isNewOrBetter = true;
                    }
                }
                else
                {
                    NodeRecord newRecord = new NodeRecord
                    {
                        node = neighbor,
                        connection = current,
                        costSoFar = newCost,
                        category = NodeRecordCategory.OPEN
                    };
                    myRecords[neighborId] = newRecord;
                    myOpenSet.Add(neighborId);
                    isNewOrBetter = true;
                }

                //If the node is new or better, create pairs with all nodes in the other open frontier
                if (isNewOrBetter)
                {
                    NodeRecord myRecord = myRecords[neighborId];
                    
                    foreach (int otherNodeId in otherOpenSet)
                    {
                        NodeRecord otherRecord = otherRecords[otherNodeId];
                        float h = heuristic.estimateDistance(myRecord.node, otherRecord.node);
                        float priority = myRecord.costSoFar + h + otherRecord.costSoFar;
                        
                        PairCandidate pair;
                        if (isForward)
                        {
                            pair = new PairCandidate { nodeIdF = neighborId, nodeIdB = otherNodeId, priority = priority };
                        }
                        else
                        {
                            pair = new PairCandidate { nodeIdF = otherNodeId, nodeIdB = neighborId, priority = priority };
                        }
                        pairQueue.Enqueue(pair, priority);
                    }
                }
            }
        }
    }
}
