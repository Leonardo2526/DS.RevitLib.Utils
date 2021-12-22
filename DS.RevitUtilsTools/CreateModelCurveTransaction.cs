using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DS.RevitUtilsTools
{
    class CreateModelCurveTransaction
    {
        readonly Document Doc;

        public CreateModelCurveTransaction(Document doc)
        {
            Doc = doc;
        }

        public ModelCurve GetModelCurve(XYZ startPoint, XYZ endPoint)
        {
            ModelCurve line = null;

            if (startPoint.DistanceTo(endPoint) < 0.01)
                return null;

            Line geomLine = Line.CreateBound(startPoint, endPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = startPoint;
            XYZ p2 = endPoint;
            XYZ p3 = new XYZ();

            if (Math.Abs(p1.X - p2.X) < 0.01 & Math.Abs(p1.Y - p2.Y) < 0.01)
                p3 = p2 + XYZ.BasisY;
            else
                p3 = p2 + XYZ.BasisZ;
            Plane geomPlane = Plane.CreateByThreePoints(p1, p2, p3);

            using (Transaction transNew = new Transaction(Doc, "automep_CreateModelLine"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    line = Doc.Create.NewModelCurve(geomLine, sketch) as ModelCurve;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
            //UIDocument uIDocument = new UIDocument(Doc);
            //uIDocument.RefreshActiveView();
            return line;
        }
    }
}
