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
        
        public Node(XYZ _pos, bool _walkable, int gridX, int gridY) 
        {
            position = _pos;
            walkable = _walkable;
            this.gridX = gridX;
            this.gridY = gridY;
        }

        public int fCost
        {
            get { return gCost + hCost; }

        }


    }


}



