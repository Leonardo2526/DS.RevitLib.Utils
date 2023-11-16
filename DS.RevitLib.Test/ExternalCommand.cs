using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Test.TestedClasses;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using QuickGraph;

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


            //new SegmentFactoryTest(uidoc)
            //    .BuildGraph()
            //    .GetSegements();

            //new GetEdgeConnectorsTest(uidoc)
            //  .BuildGraph()
            //  .GetConnectionSegment();

            var test = new MEPSystemGraphFactoryTest(uidoc);
            //new GetFamInstLocationTest( uidoc);
            //new MEPSystemGraphFactoryTest(uidoc);
            //test.Iterate(test.Graph);
            //test.PairIterate(test.Graph);
            test.SortTest(test.Graph as AdjacencyGraph<IVertex, Edge<IVertex>>);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
