using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace DS.RevitLib.Test
{
    internal class SolildPlacerTest
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public SolildPlacerTest(UIDocument uidoc, Document doc, UIApplication uiapp)
        {
            Uidoc = uidoc;
            Doc = doc;
            Uiapp = uiapp;
        }

        public void Run()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select operation element");
            Element operationElement = Doc.GetElement(reference);

            reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select target MEPCurve");
            MEPCurve targetElement = (MEPCurve)Doc.GetElement(reference);

            XYZ point = ElementUtils.GetLocationPoint(targetElement);

            var model = new SolidModelExt(operationElement);

            var solidPlacer = new SolidPlacer(model, targetElement, point);
            model = solidPlacer.Place();

            BoundingBoxXYZ box = model.Solid.GetBoundingBox();

            IVisualisator vs = new BoundingBoxVisualisator(box, Doc);
            new Visualisator(vs);
        }
    }
}
