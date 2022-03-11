using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.Revit.Utils.MEP
{
    public static class LineUtils
    {

        public static ModelCurve CreateModelCurveByPoints(XYZ startPoint, XYZ endPoint, Document doc)
        {
            ModelCurve line = null;

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

            using (Transaction transNew = new Transaction(doc, "DS.CreateModelCurve"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    line = doc.Create.NewModelCurve(geomLine, sketch) as ModelCurve;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
            return line;
        }

    }
}
