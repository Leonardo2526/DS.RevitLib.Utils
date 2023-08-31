using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.ModelSpaceFragmentation
{
    class ModelCurveTransaction : ITransaction
    {
        readonly XYZ StartPoint;
        readonly XYZ EndPoint;

        public ModelCurveTransaction(XYZ startPoint, XYZ endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }       

        void GetGeom(out Line geomLine, out Plane geomPlane)
        {
            geomLine = Line.CreateBound(StartPoint, EndPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = StartPoint;
            XYZ p2 = EndPoint;
            XYZ p3 = new XYZ();

            if (Math.Abs(p1.X - p2.X) < 0.01 & Math.Abs(p1.Y - p2.Y) < 0.01)
                p3 = p2 + XYZ.BasisY;
            else
                p3 = p2 + XYZ.BasisZ;
            geomPlane = Plane.CreateByThreePoints(p1, p2, p3);
        }

        public void Create(Document doc)
        {
            if (StartPoint.DistanceTo(EndPoint) < 0.01)
                return;

            GetGeom(out Line geomLine, out Plane geomPlane);

            using (Transaction transNew = new Transaction(doc, "automep_CreateModelLine"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    ModelCurve line = doc.Create.NewModelCurve(geomLine, sketch) as ModelCurve;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
        }
    }
}
