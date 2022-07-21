using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RevitLib.Utils.ModelCurveUtils
{

    public class ModelCurveCreator
    {

        private readonly Document _doc;
        private readonly string _transactionPrefix;

        public ModelCurveCreator(Document doc, string transactionPrefix = "")
        {
            _doc = doc;

            if (!String.IsNullOrEmpty(transactionPrefix))
            {
                _transactionPrefix = transactionPrefix + "_";
            }
        }

        public string ErrorMessages { get; private set; }


        public ModelCurve CreateByPoints(XYZ startPoint, XYZ endPoint)
        {
            ModelCurve modelLine = null;

            (Plane geomPlane, Line geomLine) = GetBaseElements(startPoint, endPoint);

            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "CreateModelCurve"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(_doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    modelLine = _doc.Create.NewModelCurve(geomLine, sketch) as ModelCurve;
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return modelLine;
        }


        private (Plane plane, Line line) GetBaseElements(XYZ startPoint, XYZ endPoint)
        {
            Line line = Line.CreateBound(startPoint, endPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = startPoint;
            XYZ p2 = endPoint;
            XYZ p3 = new XYZ();

            if (Math.Abs(p1.X - p2.X) < 0.01 & Math.Abs(p1.Y - p2.Y) < 0.01)
                p3 = p2 + XYZ.BasisY;
            else
                p3 = p2 + XYZ.BasisZ;

            Plane plane = Plane.CreateByThreePoints(p1, p2, p3);

            return (plane, line);
        }
    }
}
