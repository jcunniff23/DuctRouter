using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;
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
                    if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost && openSet[i].hCost < node.hCost)
                    {
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

                foreach (Node neighbor in grid.GetNeighborNodes(node))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                    int newCostToNeighbor = node.gCost + GetManhattanDistance(node, neighbor);
                    //TODO set logic here for ignoring diagonal moves

                    if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbor;
                        neighbor.hCost = GetManhattanDistance(neighbor, targetNode); //heuristic cost
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