using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using DuctRouter.Solver;

namespace DuctRouter
{
    public class DuctRouteHandler
    {
        private Document _doc;
        private UIDocument _uidoc;
        private readonly Application _app;

        public List<Element> Terminals = new List<Element>();
        public List<Element> DuctMains = new List<Element>();

        private Solver.RoutingService _routingService;

        public DuctRouteHandler(ExternalCommandData commandData) 
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            _uidoc = commandData.Application.ActiveUIDocument;
            _app = commandData.Application.Application;
        
        }
    
        public void AddTerminalsToHandler(List<Element> terminals)
        {
            try
            {
                Terminals.AddRange(terminals);
            }
            catch (Exception)
            {
                TaskDialog.Show("DUCTROUTEHANDLER", "AddTerminalsToHandler failed");
                throw;
            }
        }
        public void AddDuctsToHandler(List<Element> ducts) 
        {
            try
            {
                DuctMains.AddRange(ducts);
            }
            catch (Exception)
            {
                TaskDialog.Show("DUCTROUTEHANDLER", "AddDuctsToHandler failed");
                throw;
            }
        }

        public void DiscardAllElements() 
        {
            Terminals.Clear();
            DuctMains.Clear();
        }

        public void RouteAllElements() 
        {
            bool logging = false;
            try
            {
                // Check for valid DuctMains
                if (DuctMains == null || DuctMains.Count == 0)
                {
                    TaskDialog.Show("DuctRouteHandler", "No duct mains found.");
                    return;
                }

                // Get duct geometry and check if it's valid
                GeometryElement ductGeometry = DuctMains[0].get_Geometry(new Options());
                if (ductGeometry == null || !ductGeometry.Any())
                {
                    TaskDialog.Show("DuctRouteHandler", "Duct geometry is invalid or empty.");
                    return;
                }

                BoundingBoxXYZ ductBbox = ductGeometry.GetBoundingBox();
                if (ductBbox == null)
                {
                    TaskDialog.Show("DuctRouteHandler", "Bounding box is null.");
                    return;
                }

                // Log bounding box details
                if (logging) TaskDialog.Show("BoundingBox", $"Duct BBox: {ductBbox.Min.X}, {ductBbox.Min.Y}, {ductBbox.Min.Z}, {ductBbox.Max.X}, {ductBbox.Max.Y}, {ductBbox.Max.Z}");

                // Filter terminals with valid LocationPoint
                List<XYZ> terminalLocs = Terminals
                    .Where(t => t.Location is LocationPoint)
                    .Select(t => (t.Location as LocationPoint).Point)
                    .ToList();

                if (terminalLocs.Count == 0)
                {
                    TaskDialog.Show("DuctRouteHandler", "No valid terminal locations found.");
                    return;
                }

                // Log terminal locations
                if (logging)
                    foreach (var loc in terminalLocs)
                    {
                        TaskDialog.Show("Terminal Location", $"Terminal Location: {loc.X}, {loc.Y}, {loc.Z}");
                    }
                try
                {
                    // Initialize RoutingService
                    _routingService = new RoutingService(ductBbox, terminalLocs, 0, 2);
                    var message = _routingService.PathFindAStar();
                    TaskDialog.Show("A STAR RESULTS", message);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"Exception: {ex.Message}\n{ex.StackTrace}");
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("DuctRouteHandler", $"Error: {ex.Message}\n{ex.StackTrace}");
            }

            


            ShowRouteResultDialog();
        }

        private void ShowRouteResultDialog()
        {


            try
            {
                //var message = _routingService.DebugHandler();
                var routes= _routingService.OptimizeRoutes();
                var message = routes.Select(r => r.ToString()).ToString();

                TaskDialog.Show("DuctRouteHandler Result", message);
               
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }


        public void PlaceDuctRoutes() 
        { 
        
        
        }

        public void ConvertPlaceholders() 
        {
        
        }

        public void ResponseTest() 
        { 
            //Quick method for testing commandaData reference && ability for callling btw classes
            TaskDialog.Show("THIS IS A TEST", _uidoc.Document.PathName.ToString()); 

        }
    
        
    
    }
}
