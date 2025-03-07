﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;

namespace DuctRouter.Solver
{
    public class Grid
    {
        private Node[,] grid;
        private int gridSizeX, gridSizeY;
        private XYZ gridMin, gridMax;
        private double stepSize;

        public Grid(XYZ gridMin, XYZ gridMax, double stepSize)
        {
            this.gridMin = gridMin;
            this.gridMax = gridMax;
            this.stepSize = stepSize;
            gridSizeX = (int)Math.Round((gridMax.X - gridMin.X) / stepSize);
            gridSizeY = (int)Math.Round((gridMax.Y - gridMin.Y) / stepSize);
            grid = new Node[gridSizeX, gridSizeY];

            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    XYZ modelLocation = new XYZ(gridMin.X + i * stepSize, gridMin.Y + j * stepSize, gridMin.Z);
                    bool walkable = true; // TODO implement a collision detection method w/ model geometry.
                    grid[i, j] = new Node(modelLocation, walkable, i, j);
                }
            }
        }

        public Node GetGridFromModelLocation(XYZ location)
        {
            if (location.X < gridMin.X || location.X > gridMax.X)
                return null;
            else if (location.Y < gridMin.Y || location.Y > gridMax.Y)
                return null;

            //Math.Round(t.X * (1 / gridSize), MidpointRounding.AwayFromZero) / (1 / gridSize);
            double xpos = Math.Round((location.X - gridMin.X) * (1 / stepSize), MidpointRounding.AwayFromZero);
            double ypos = Math.Round((location.Y - gridMin.Y) * (1 / stepSize), MidpointRounding.AwayFromZero);
            int x = (int)(xpos);
            int y = (int)(ypos);

            return grid[x, y];
        }

        public List<Node> GetNeighborNodes(Node node)
        {
            //find where provided node is in array using nodes own tracking varaibles
            //gridX gridY are needed here. look into way to remove them to remove memory overhead
            List<Node> neighbors = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; //skip iteration if we are on provided node
                    
                    //comment out following block to reconsider 45deg connections
                    if (x == -1 && y == -1) continue;
                    if (x == -1 && y == 1) continue;
                    if (x == 1 && y == -1) continue;
                    if (x == 1 && y == 1) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }

        public void SetCollisionArea(double xmax, double ymax, double xmin, double ymin)
        {
            //Method will take a real XY coordinate polygon rectangle and set it to unwalkable for nodes in the grid
            //If collision area intersects or goes out of bounds of grid, points will be ignored.

            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    var pos = this.grid[i, j];
                    if (pos.position.X < xmax && pos.position.Y < ymax && pos.position.X > xmin && pos.position.Y > ymin)
                    {
                        //If the node in the array has coordinates within the bounds of this box,
                        //set that node to UNwalkable
                        pos.walkable = false;
                    }
                }
            }
        }
    }
}