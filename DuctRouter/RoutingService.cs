using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;    // namespace that contains all of the needed geometry manipulation and data structures from revit

namespace DuctRouter
{
    public class RoutingService
    {

        private readonly int _xMax;
        private readonly int _xMin;
        private readonly int _yMax;
        private readonly int _yMin;
        private readonly int _zMax;
        private readonly int _zMin;

        private readonly List<XYZ> _ductBounds;
        private readonly List<XYZ> _terminalLocations;


        private readonly int? _minClearance; // clearance wanted between new ducts created (in what units??)
        private readonly int? _boundaryMultiplier;
        //  fudge factor to multiply to the max distance centerlines of duct main to terminals,
        //  with the goal of creating an easy way for making a "boundary" for the ducts to be solved within.
        //  this factor is necessary on the Z coordiante because duct elbows often will be larger than expected (radius requirements as a function of duct diameter)

        private Dictionary<int, XYZ> _nodeLocations = new Dictionary<int, XYZ>();

        public RoutingService(BoundingBoxXYZ ductBBox, List<XYZ> terminalLocations, int clearance = 0, int multiplier = 1)
        {
            _minClearance = clearance;
            _boundaryMultiplier = multiplier;

            //Find the solver boundaries utilizing the provided duct and terminal geometry in model

            //BoundingBoxXYZ ductBBox = ductGeometry.GetBoundingBox();
            List<XYZ> ductBounds = new List<XYZ>{
                ductBBox.get_Bounds(0),
                ductBBox.get_Bounds(1)
            };
            _ductBounds = ductBounds;

            //List<XYZ> terminalLocationsXYZ = terminalLocations.Select(loc => new XYZ(loc.Point.X, loc.Point.Y, loc.Point.Z)).ToList();
            List<XYZ> terminalLocationsXYZ = terminalLocations;
            _terminalLocations = terminalLocationsXYZ;

            List<double> xPoints = terminalLocationsXYZ.Select(x => x.X)
                                   .Concat(ductBounds.Select(x => x.X))
                                   .ToList();

            List<double> yPoints = terminalLocationsXYZ.Select(y => y.Y)
                                    .Concat(ductBounds.Select(y => y.Y))
                                    .ToList();

            List<double> zPoints = terminalLocationsXYZ.Select(z => z.Z)
                        .Concat(ductBounds.Select(z => z.Z))
                        .ToList();

            _xMax = (int)Math.Round(xPoints.Max()) + 1;
            _xMin = (int)Math.Round(xPoints.Min()) - 1;

            _yMax = (int)Math.Round(yPoints.Max()) + 1;
            _yMin = (int)Math.Round(yPoints.Min()) - 1;

            _zMax = (int)Math.Round(zPoints.Max()) + 1;
            _zMin = (int)Math.Round(zPoints.Min()) - 1;

            Console.WriteLine("X MAX " + _xMax);
            Console.WriteLine("X MIN " + _xMin);
            Console.WriteLine("Y MAX " + _yMax);
            Console.WriteLine("Y MIN " + _yMax);
            Console.WriteLine("Z MAX " + _zMax);
            Console.WriteLine("Z MIN " + _zMax);

        }

        public string DebugHandler()
        {
            var res = "";

            res = "X MAX " + _xMax + "\n" +
                  "X MIN " + _xMin + "\n" +
                  "Y MAX " + _yMax + "\n" +
                  "Y MIN " + _yMax + "\n" +
                  "Z MAX " + _zMax + "\n" +
                  "Z MIN " + _zMax;


            return res;

        }

        public List<(XYZ, XYZ)> OptimizeRoutes()
        {
            //Manage and setup routing data and indicdes

           

            //After branch off point, determine length for next duct on branch,
            //iterate through until all ducts in new branch are made
            //solve for shortest distance
            return new List<(XYZ, XYZ)>();
        }

        // Helper method to create a cost evaluator for the arc (distance-based)
       
        private bool ValidateOrthogonality(List<(XYZ, XYZ)> routes)
        {
            foreach (var route in routes)
            {
                XYZ start = route.Item1;
                XYZ end = route.Item2;

                int changedAxes = 0;
                if (start.X != end.X) changedAxes++;
                if (start.Y != end.Y) changedAxes++;
                if (start.Z != end.Z) changedAxes++;

                // If more than one axis changes, it's not orthogonal
                if (changedAxes > 1)
                    return false;
            }
            return true;
        }
       
       
        public enum BranchMethod
        {
            OnePerBranch,
            MultiplePerBranch
        }
        private void InitializeLocations(BranchMethod mode = BranchMethod.OnePerBranch)
        {
            int depotCount = 0;

            switch (mode)
            {
                case BranchMethod.OnePerBranch:
                    depotCount = _terminalLocations.Count();
                    break;
                case BranchMethod.MultiplePerBranch:
                    depotCount = (int)Math.Round(_terminalLocations.Count() * 0.5);
                    break;
                default:
                    depotCount = _terminalLocations.Count();
                    break;
            }


            int key = 0;
            DuctMainDir();
            for (int i = 0; i < _terminalLocations.Count; i++)
            {
                XYZ branchNode = GetBranchNode(_ductMainDir);
                //this does not prevent same node from being used twice. oops
                _nodeLocations.Add(key, branchNode);
                key++;
                _nodeLocations.Add(key, _terminalLocations[i]);
                key++;

            }

        }

        private Axis _ductMainDir;
        private enum Axis
        {
            X,
            Y, 
            Z
        }

        private void DuctMainDir()
        {
            var boundsLower = _ductBounds[0];
            var boundsUpper = _ductBounds[1];
        
            var xSpan = Math.Abs(boundsUpper.X - boundsLower.X);
            var ySpan = Math.Abs(boundsUpper.Y - boundsLower.Y);
            
            if (xSpan > ySpan) _ductMainDir = Axis.X;
            else _ductMainDir = Axis.Y;
        }

        private XYZ GetBranchNode(Axis mainAx)
        {
            //get a random location for a new branch node on the mainduct
            //
            var longCenterline = 0.0;
            double longAxLen;
            double longAxMin;
            if (mainAx == Axis.X)
            { 
                longCenterline = (_ductBounds[0].Y + _ductBounds[1].Y) / 2;
                longAxLen = Math.Abs(_ductBounds[0].X + _ductBounds[1].X);
                longAxMin = _ductBounds[0].X;
            }
            else
            { 
                longCenterline = (_ductBounds[0].X + _ductBounds[1].X) / 2;
                longAxLen = Math.Abs(_ductBounds[0].Y + _ductBounds[1].Y);
                longAxMin = _ductBounds[0].Y;

            }
            int ductDivisions = 12;
            var random = new Random();
            int chosenDiv = random.Next(1, ductDivisions + 1);

            double axDivisions = longAxLen / ductDivisions;

            var branchCoord = axDivisions * chosenDiv + longAxMin;

            if (mainAx == Axis.X) 
                return new XYZ(branchCoord, longCenterline, _ductBounds[0].Z);
            else 
                return new XYZ(longCenterline, branchCoord, _ductBounds[0].Z);



        }
    }
}
