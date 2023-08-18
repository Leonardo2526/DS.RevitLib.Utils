using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class ZoomTest
    {
        private readonly UIApplication _app;
        private readonly Document _doc;
        private readonly UIDocument _uidoc;

        public ZoomTest(UIApplication app, Document doc)
        {
            _app = app;
            _doc = doc;
            _uidoc = app.ActiveUIDocument;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var element = _doc.GetElement(reference);
          
            var (con1, con2) = ConnectorUtils.GetMainConnectors(element);
            var solid = ElementUtils.GetSolid(element);

            var boxXYZ = new Zoomer(_doc, _app).Zoom(solid);
            //var boxXYZ = new Zoomer(_doc, _app).Zoom(con1.Origin, con2.Origin, 5);

            //show bb
            new TransactionBuilder(_doc).Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(boxXYZ, _doc);
                visualizator.Show();
            }, "show boundingBox");


            new SectionBox(_app).Set(boxXYZ);
        }
    }
}
