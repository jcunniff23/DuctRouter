using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DuctRouter
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {

            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            try
            {

                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CommandLoad_AssemblyResolve);
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

        private Assembly CommandLoad_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("CsvHelper"))
            {
                string assemblyFile = "C:\\Users\\jcunniff\\source\\repos\\DuctRouter\\DuctRouter\\bin\\Debug\\CsvHelper.dll";
                if (File.Exists(assemblyFile))
                    return Assembly.LoadFrom(assemblyFile);
                else 
                    return null;
            }
            else return null;
        }
    }

}
