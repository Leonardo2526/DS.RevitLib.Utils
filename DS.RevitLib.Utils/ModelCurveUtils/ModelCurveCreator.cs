using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Planes;
using System;

namespace DS.RevitLib.Utils.ModelCurveUtils
{
    /// <summary>
    /// Class for create and modify ModelCurves. 
    /// Transactions are not provided, so a used method should be wrapped into transacion.
    /// </summary>
    public class ModelCurveCreator
    {
        private readonly Document _doc;

        /// <summary>
        /// Create a new instance of object to create and modify ModelCurves. 
        /// </summary>
        /// <param name="doc"></param>
        public ModelCurveCreator(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Create a new ModelCurve between <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns>Returns created ModelCurve.</returns>
        public ModelCurve Create(XYZ startPoint, XYZ endPoint)
        {
            (Plane geomPlane, Line geomLine) = GetBaseElements(startPoint, endPoint);

            // Create a sketch plane in current document
            SketchPlane sketch = SketchPlane.Create(_doc, geomPlane);

            return _doc.Create.NewModelCurve(geomLine, sketch);
        }

        /// <summary>
        /// Create a new ModelCurve from <paramref name="line"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>Returns created ModelCurve.</returns>
        public ModelCurve Create(Line line)
        {
            Plane plane = PlaneUtils.CreateByLineAndPoint(line);

            // Create a sketch plane in current document
            SketchPlane sketch = SketchPlane.Create(_doc, plane);

            return _doc.Create.NewModelCurve(line, sketch);
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
