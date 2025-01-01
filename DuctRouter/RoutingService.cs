using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Google.OrTools.Sat;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;    // namespace that contains all of the needed geometry manipulation and data structures from revit

namespace DuctRouter.Service
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
            var model = new CpModel();

            var start = new XYZ(_terminalLocations[0].X, _ductBounds[1].Y, ((_ductBounds[0].Z + _ductBounds[1].Z) /2));
            var end = new XYZ(_terminalLocations[0].X, _terminalLocations[0].Y, ((_ductBounds[0].Z + _ductBounds[1].Z) / 2));
            var myTuple = (start, end);
            var res = new List<(XYZ, XYZ)> { myTuple };

            //Start by determining the branch off point








            //After branch off point, determine length for next duct on branch,
            //iterate through until all ducts in new branch are made
            //solve for shortest distance



















            return res;

        }    








    }
}
