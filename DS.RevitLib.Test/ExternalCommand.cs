using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DS.RevitLib.Test.TestedClasses;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Extensions;
using Autodesk.Revit.UI.Selection;

namespace DS.RevitLib.Test
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
            Document doc = uiapp.ActiveUIDocument.Document;



            var selector = new ElementSelector(uidoc) { AllowLink = true };
            var element = selector.Pick();
            var insulation = element.GetInsulation();

            //var test = new SolidContainsPointTest( doc, uidoc);
            //var test = new BuilderByPointsTest(uidoc);
            //var test = new GetAssociatedParameterTest(uidoc);

            //var test = new MEPSystemBuilderTest(doc, uidoc);

            //var test = new ConnectionFactoryClient(uidoc);
            //test.Run();

            //var test = new GetBasisVectorOnPlaneTest(uidoc);
            //test.Run();
            //test.RunMultiple();
            //test.RepeatRun();


            //var pathFinderTest = new SimplePathFinderTest(uidoc, doc);
            //var path = pathFinderTest.RunTest2();
            //pathFinderTest.ShowPath(path);
            //SolidCollisionCheckerTest.RunWithLink(doc);

            //var selector = new SystemModelTest(uidoc, doc, uiapp);
            //selector.RunTest();

            //var test = new SelectionTest(uidoc);
            //var elem = test.PickElement();
            //test.SelectInLink();
            //try
            //{ var elem = test.CenterPoint(); }
            //catch (Exception ex)
            //{ var elem = test.PickPoint(); }


            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
