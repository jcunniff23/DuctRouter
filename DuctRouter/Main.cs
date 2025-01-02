using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using or = Google.OrTools;
//using Google.OrTools.LinearSolver;

namespace DuctRouter
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {

            // Set up the AssemblyResolve event handler
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name.Contains("Google.ORTools"))
                {
                    string path = @"C:\Users\jcunniff\source\repos\DuctRouter\DuctRouter\bin\x64\Debug\Google.OrTools.dll"; // Replace with the actual path
                    return Assembly.LoadFrom(path);
                }
                return null;
            };

            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            try
            {
                //DuctRouteUI mainWindow = new DuctRouteUI(doc, uiDoc);
                DuctRouteUI mainWindow = new DuctRouteUI(commandData);
                mainWindow.Show();
                return Result.Succeeded;
            }
            catch (Exception)
            {
                TaskDialog.Show("ERROR", "DUCTROUTER FAILED TO EXECUTE");
                return Result.Failed;
            }
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class TestORToolsMain : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            // Create the linear solver with the GLOP backend.
            or.LinearSolver.Solver solver = or.LinearSolver.Solver.CreateSolver("SimpleLpProgram");

            // Create the variables x and y.
            or.LinearSolver.Variable x = solver.MakeNumVar(0.0, 1.0, "x");
            or.LinearSolver.Variable y = solver.MakeNumVar(0.0, 2.0, "y");

            Console.WriteLine("Number of variables = " + solver.NumVariables());

            // Create a linear constraint, 0 <= x + y <= 2.
            or.LinearSolver.Constraint ct = solver.MakeConstraint(0.0, 2.0, "ct");
            ct.SetCoefficient(x, 1);
            ct.SetCoefficient(y, 1);

            Console.WriteLine("Number of constraints = " + solver.NumConstraints());

            // Create the objective function, 3 * x + y.
            or.LinearSolver.Objective objective = solver.Objective();
            objective.SetCoefficient(x, 3);
            objective.SetCoefficient(y, 1);
            objective.SetMaximization();

            solver.Solve();

            var msg = "Solution:" + "\n" +
           "Objective value = " + solver.Objective().Value() + "\n" +
           "x = " + x.SolutionValue() + "\n" +
           "y = " + y.SolutionValue() + "\n" +
            "Run Completed";
            TaskDialog.Show("TEST OR TOOLS", msg);
            return Result.Succeeded;
        }
    }
}
