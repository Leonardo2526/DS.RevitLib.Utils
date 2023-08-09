using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Creation.Transactions;
using Rhino.Geometry;

namespace DS.RevitLib.Utils.Geometry.Points
{
    /// <summary>
    /// An object is used to show <see cref="Point3d"/> points.
    /// </summary>
    public class Point3dVisualisator : XYZVisualizator, IPointVisualisator<Point3d>
    {
        private readonly IPoint3dConverter _pointConverter;

        /// <summary>
        /// Instansiate an object to show <see cref="Point3d"/> points.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="pointConverter"></param>
        /// <param name="labelSize"></param>
        /// <param name="transactionBuilder"></param>
        /// <param name="refresh"></param>
        public Point3dVisualisator(UIDocument uiDoc, IPoint3dConverter pointConverter = null,
            double labelSize = 0, ITransactionFactory transactionBuilder = null, bool refresh = false) :
            base(uiDoc, labelSize, transactionBuilder, refresh)
        {
            _pointConverter = pointConverter;
        }

        /// <inheritdoc/>
        public void Show(Point3d point)
        {
            var point3d = _pointConverter is null ? point : _pointConverter.ConvertToUCS1(point);
            XYZ xYZPoint = new XYZ(point3d.X, point3d.Y, point3d.Z);
            Show(xYZPoint);
        }

        /// <inheritdoc/>
        public void ShowVector(Point3d point1, Point3d point2)
        {
            var point3d1 = _pointConverter is null ? point1 : _pointConverter.ConvertToUCS1(point1);
            var point3d2 = _pointConverter is null ? point2 : _pointConverter.ConvertToUCS1(point2);
            XYZ xYZPoint1 = new XYZ(point3d1.X, point3d1.Y, point3d1.Z);
            XYZ xYZPoint2 = new XYZ(point3d2.X, point3d2.Y, point3d2.Z);
            ShowVector(xYZPoint1, xYZPoint2);
        }

        /// <inheritdoc/>
        public void ShowVectorByDirection(Point3d origin, Point3d direction)
        {
            var p2 = origin + direction;
            ShowVector(origin, p2);
        }
    }
}
