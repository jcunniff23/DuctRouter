using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuctRouterTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RoutingServiceTest()
        {
            BoundingBoxXYZ ductMainBbox = new BoundingBoxXYZ();
            ductMainBbox.Min = new XYZ(-43.8097, 18.2646, 2.5416);
            ductMainBbox.Max = new XYZ(-19.1847, 19.4313, 3.4583);

            List<XYZ> terminals = new List<XYZ> { new XYZ(-39.800, 31.325, 0.00) };

            //var Routing = new RoutingService(ductMainBbox, terminals);

            //var result = Routing.OptimizeRoutes();
            //Console.WriteLine(result);
            //Assert.IsNotNull(result);

        }
    }
}
