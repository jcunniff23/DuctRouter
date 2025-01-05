using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper; 

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Interop;
using System.IO;
using System.Globalization;    // namespace that contains all of the needed geometry manipulation and data structures from revit

namespace DuctRouter.Solver
{
    public class RoutingService
    {

        private readonly double _xMax;
        private readonly double _xMin;
        private readonly double _yMax;
        private readonly double _yMin;
        private readonly double _zMax;
        private readonly double _zMin;
        private readonly List<XYZ> _ductBounds;
        private readonly List<XYZ> _terminalLocations;
        private readonly double? _minClearance; // clearance wanted between new ducts created (in what units??)
        private readonly double? _boundaryMultiplier;
            //  fudge factor to multiply to the max distance centerlines of duct main to terminals,
            //  with the goal of creating an easy way for making a "boundary" for the ducts to be solved within.
            //  this factor is necessary on the Z coordiante because duct elbows often will be larger than expected (radius requirements as a function of duct diameter)

        public RoutingService(BoundingBoxXYZ ductBBox, List<XYZ> terminalLocations, int clearance = 0, int multiplier = 1)
        {

            //From revit geometry, determine the size of the working grid
            _minClearance = clearance;
            _boundaryMultiplier = multiplier;

            List<XYZ> ductBounds = new List<XYZ>{
                ductBBox.get_Bounds(0),
                ductBBox.get_Bounds(1)
            };
            _ductBounds = ductBounds;

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

            _xMax = Math.Round(xPoints.Max());
            _xMin = Math.Round(xPoints.Min());

            _yMax = Math.Round(yPoints.Max());
            _yMin = Math.Round(yPoints.Min());

            _zMax = Math.Round(zPoints.Max());
            _zMin = Math.Round(zPoints.Min());

            var rect = new (double, double)[] { (_xMax, _yMax), (_xMax, _yMin), (_xMin, _yMax), (_xMin, _yMin) };
            var newRect = scaleRectangle(rect, multiplier);

            _xMax = newRect[0].Item1;
            _yMax = newRect[0].Item2;
            _xMin = newRect[3].Item1;
            _yMin = newRect[3].Item2;

        }

        private (double,double)[] scaleRectangle((double, double)[] coordinates, double multiplier)
        {
            double centerX = 0, centerY = 0;
            foreach (var (x, y) in coordinates)
            {
                centerX += x;
                centerY += y;
            }

            centerX /= 4;
            centerY /= 4;

            var transformedCoords = new (double, double)[4];
            for (var i = 0; i < coordinates.Length; i++)
            {
                double dx = coordinates[i].Item1 - centerX;
                double dy = coordinates[i].Item2 - centerY;

                double newX = centerX + dx * multiplier;
                double newY = centerY + dy * multiplier;

                transformedCoords[i] = (newX, newY);
            }

            return transformedCoords;

        }
        public string DebugHandler()
        {
            return "X MAX " + _xMax + "\n" +
                  "X MIN " + _xMin + "\n" +
                  "Y MAX " + _yMax + "\n" +
                  "Y MIN " + _yMax + "\n" +
                  "Z MAX " + _zMax + "\n" +
                  "Z MIN " + _zMax;
        }

        public List<(XYZ, XYZ)> OptimizeRoutes()
        {
            //v2 - Simulated Annealing Routing, custom implementation





            return new List<(XYZ, XYZ)>();
        }

        public string PathFindAStar()
        {
            string mymsg = "";

            var startLoc = new XYZ((_ductBounds[0].X + _ductBounds[1].X) / 2, _ductBounds[1].Y, _ductBounds[0].Z);
            var endLoc = new XYZ(_terminalLocations[0].X, _terminalLocations[0].Y, _ductBounds[0].Z);

            XYZ min = new XYZ(_xMin, _yMin, _zMin);
            XYZ max = new XYZ(_xMax, _yMax, _zMax);
            Debug.WriteLine("min: " + min + "\nmax: " + max);

            var grid = new Solver.Grid(min, max, 0.25);
            var PathFinder = new Pathfinder(grid);

            var nodes = PathFinder.FindPath(startLoc, endLoc);
            WriteResultsCSV(nodes, $"AStar_Results_8");

            foreach (var item in nodes)
            {
                mymsg += $"Node {nodes.IndexOf(item)}: X {item.position.X}, Y {item.position.Y}, Fcost {item.fCost}\n";
            }
            mymsg += "\n";
            mymsg += $"Start Pos: ({startLoc.X}, {startLoc.Y})";
            mymsg += "\n";
            mymsg += $"End Pos: ({endLoc.X}, {endLoc.Y})";

            return mymsg;
        }

        private void WriteResultsCSV(List<Node> records, string name)
        {
            using (var writer = new StreamWriter($"C:\\Users\\jcunniff\\source\\repos\\DuctRouter\\data\\{name}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
            
        }






    }
}
