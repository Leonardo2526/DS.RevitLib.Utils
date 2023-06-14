using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class SolidCreatorTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public SolidCreatorTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.PointOnElement, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            XYZ center = reference.GlobalPoint;
            //XYZ center = _uiDoc.Selection.PickPoint(ObjectSnapTypes.Centers, "PickPoint");
            var creator = new SphereCreator(1, center);
            creator.CreateSolid();

            GeometryElementsUtils.Show(creator.Profile, _doc);
            creator.ShowSolid(_doc);
        }
    }
}
