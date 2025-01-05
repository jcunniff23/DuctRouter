using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using DuctRouter.Solver;

namespace DuctRouter
{
    public class Pathfinder
    {
        private Solver.Grid grid;

        public Pathfinder(Solver.Grid grid)
        {
            this.grid = grid;
        }

        public List<Node> FindPath(XYZ startPos, XYZ endPos)
        {
            Node startNode = grid.GetGridFromModelLocation(startPos);
            Node targetNode = grid.GetGridFromModelLocation(endPos);

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node node = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost <= node.fCost)
                    {
                        if (openSet[i].fCost != node.fCost)
                            node=openSet[i];
                        else if (openSet[i].pCost < node.pCost && openSet[i].hCost < node.hCost)
                            node = openSet[i];
                    }
                }

                openSet.Remove(node);
                closedSet.Add(node);

                if (node == targetNode)
                {
                    var path = RetracePath(startNode, targetNode);
                    return path;
                }

                //Step to neighbors while gradually getting closer to goal position
                //Main loop for iterating across map
                foreach (Node neighbor in grid.GetNeighborNodes(node))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                    //Following logic is for ignoring diagonal moves & punishing elbows.
                    int newElbowPenaltyToNeighbor;
                    int dx, dy;
                    if (node.parent != null)
                    {
                        dx = Math.Abs(node.parent.gridX - node.gridX);
                        dy = Math.Abs(node.parent.gridY - node.gridY);
                    } else
                    {
                        dx = 0;
                        dy = 1; //set y vector away from duct main --- TODO dynamically set dx, dy based on duct main direction (get to 45degs later)
                    }

                    if (neighbor.gridX != node.gridX && neighbor.gridY != node.gridY)
                        newElbowPenaltyToNeighbor = 10000000; //do not set max value or it will bust and go negative
                    //case of diagonal moves. disabled for now.
                    else if (dy == 1 & neighbor.gridX == node.gridX)
                        newElbowPenaltyToNeighbor = 0;
                    else if (dx == 1 & neighbor.gridY == node.gridY)
                        newElbowPenaltyToNeighbor = 0;
                    else
                        newElbowPenaltyToNeighbor = 20; // TWEAK THIS NUMBER!!!!!!!

                    //else if (dy == 1 & neighbor.gridX != node.gridX)
                    //    newElbowPenaltyToNeighbor = 20;


                    int newCostToNeighbor = node.gCost + GetManhattanDistance(node, neighbor);

                    if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbor;
                        neighbor.hCost = GetManhattanDistance(neighbor, targetNode); //heuristic cost
                        neighbor.pCost = newElbowPenaltyToNeighbor;
                        neighbor.parent = node;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            Debug.WriteLine("ESCAPED WHILE LOOP SOMEHOW!?");
            return new List<Node>();
        }

        private List<Node> RetracePath(Node start, Node target)
        {
            List<Node> path = new List<Node>();
            Node currentNode = target;

            while (currentNode != start)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();
            return path;
        }

        private int GetManhattanDistance(Node nodeA, Node nodeB)
        {
            //Implements a Manhattan Distance to prevent diagonal calculation

            int distX = Math.Abs(nodeA.gridX - nodeB.gridX);
            int distY = Math.Abs(nodeA.gridY - nodeB.gridY);

            return distX * 10 + distY * 10;
        }
    }
}