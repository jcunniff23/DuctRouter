using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using DuctRouter.Solver;

namespace DuctRouter
{
    public class DuctAnalysis
    {
        private List<Node> nodes;
        private Solver.Grid grid;
        private Application crApp;
        private List<Point> points;

        private Autodesk.Revit.DB.Document doc;
        private Autodesk.Revit.UI.UIApplication app;

        public List<Line> lines { get; private set; }

        public DuctAnalysis(ExternalCommandData commandData, List<Node> route, Solver.Grid grid)
        {
            this.nodes = route;
            this.grid = grid;
            this.doc = commandData.Application.ActiveUIDocument.Document;
            this.app = commandData.Application.ActiveUIDocument.Application;
            this.crApp = commandData.Application.Application.Create;
            CreateLines();
        }

        public void GrabPoints()
        {
            var radius = 0.25;
            foreach (Node node in nodes)
            {
                var point = crApp.NewXYZ(
                    node.position.X,
                    node.position.Y,
                    node.position.Z);
                Frame frame = new Frame(point, XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ);
                Arc arc = Arc.Create(
                    point - radius * XYZ.BasisZ,
                    point + radius * XYZ.BasisZ,
                    point + radius * XYZ.BasisX);

                Line line = Line.CreateBound(arc.GetEndPoint(1), arc.GetEndPoint(0));
                CurveLoop halfCirc = new CurveLoop();
                halfCirc.Append(arc);
                halfCirc.Append(line);
                List<CurveLoop> loops = new List<CurveLoop>(1);
                loops.Add(halfCirc);

                GeometryCreationUtilities.CreateRevolvedGeometry(frame, loops, 0, 2 * Math.PI);
            }
        }

        private void CreateLines()
        {
            List<Line> lines = new List<Line>();

            foreach (Node node in nodes)
            {
                if (node.parent != null)
                {
                    Line line = Line.CreateBound(node.position, node.parent.position);
                    lines.Add(line);
                }
            }

            this.lines = lines;
        }

        public void PaintPath()
        {
            View view = doc.ActiveView;
            //CreateAnalysisDisplayStyle(view);

            if (!(view is View3D))
            {
                throw new InvalidOperationException("The active view must be a 3D view to display the path.");
            }

            SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(view) ??
                                      SpatialFieldManager.CreateSpatialFieldManager(view, 1);
            int _schemaId = -1;

            IList<int> registeredResults = sfm.GetRegisteredResults();
            foreach (int id in registeredResults)
            {
                AnalysisResultSchema existingSchema = sfm.GetResultSchema(id);
                if (existingSchema.Name == "PathVisualization")
                {
                    _schemaId = id; // Reuse the existing schema
                    break;
                }
            }

            if (_schemaId == -1)
            {
                AnalysisResultSchema schema = new AnalysisResultSchema("PathVisualization", "A* Path Data");
                _schemaId = sfm.RegisterResult(schema);
            }

            foreach (Line path in lines)
            {
                Transform trf = Transform.Identity;
                int idx = sfm.AddSpatialFieldPrimitive(path, trf);

                // Example: Add penalty or metadata as field values
                IList<XYZ> xyzPts = new List<XYZ> { path.GetEndPoint(0), path.GetEndPoint(1) };
                FieldDomainPointsByXYZ pnts = new FieldDomainPointsByXYZ(xyzPts);

                var startNode = nodes.FirstOrDefault(n => n.position.IsAlmostEqualTo(path.GetEndPoint(0)));
                var endNode = nodes.FirstOrDefault(n => n.position.IsAlmostEqualTo(path.GetEndPoint(1)));
                double startVal = 0;
                double endVal = 0;

                if (startNode == null)
                    startVal = 0;
                else
                    startVal = startNode.fCost;
                if (endNode == null)
                    endVal = 0;
                else
                    endVal = endNode.fCost;

                List<double> doubleList = new List<double>();
                IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                doubleList.Add(1.0);
                valList.Add(new ValueAtPoint(doubleList));
                //doubleList.Clear();
                //doubleList.Add(12.2);
                //valList.Add(new ValueAtPoint(doubleList));
                FieldValues vals = new FieldValues(valList);
               


                sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, _schemaId);
            }
        }

        private void CreateAnalysisDisplayStyle(View view)
        {
            using (Transaction t = new Transaction(doc, "Create Analysis Display Style"))
            {
                t.Start();

                AnalysisDisplayColoredSurfaceSettings surfaceSettings = new AnalysisDisplayColoredSurfaceSettings
                {
                    ShowGridLines = false
                };

                AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
                AnalysisDisplayLegendSettings legendSettings = new AnalysisDisplayLegendSettings
                {
                    ShowLegend = true // Optional, if you want a legend
                };

                AnalysisDisplayStyle style = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(
                    doc, "Path Analysis Style", surfaceSettings, colorSettings, legendSettings);

                view.AnalysisDisplayStyleId = style.Id;

                t.Commit();
            }
        }

    }
}