using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitCollisions.ManualTest.TestCases;
using DS.RevitCollisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.ManualTest
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;


            var test = new ResolveProcessorBuilderTest(uiapp);
            //test.RunTest1();
            //Task.Run(() => test.RunTest1ASync());
            //test.RunTest1ASync();
            test.RunTestMany();


            //var test = new ElementCollisionFactoryTest(uiapp);
            //test.BuildFactory();
            //test.CreateCollision();
            //test.CreateCollision();

            //var test = new DummyTest(uidoc);
            //test.CreateResolveProcessor();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
