using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DuctRouter.Solver
{
    public class Node
    {
        public bool walkable;
        public XYZ position;
        public int gCost;
        public int hCost;
        public int gridX;
        public int gridY;
        public Node parent;
        public int pCost;
        
        public Node(XYZ _pos, bool _walkable, int gridX, int gridY) 
        {
            position = _pos;
            walkable = _walkable;
            this.gridX = gridX;
            this.gridY = gridY;
        }


        public string x { get { return position.X.ToString(); } }
        public string y { get { return position.Y.ToString(); } }
        public int G_Cost { get { return gCost; } }
        public int H_Cost { get {return hCost; } }

        public int P_Cost {  get { return pCost; } }
        public int fCost
        {
            get { return gCost + hCost + pCost; }

        }
        public int GridX { get { return gridX; } }
        public int GridY { get { return gridY; } }



    }


}



