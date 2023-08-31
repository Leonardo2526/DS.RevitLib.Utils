using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Visualisators;
using System.Windows.Media.Media3D;

namespace DS.RevitLib.Utils.Geometry.Points
{
    public class PointVisualisator : XYZVisualizator, IPointVisualisator<Point3D>
    {
        public PointVisualisator(UIDocument uiDoc, double labelSize = 0, ITransactionFactory transactionBuilder = null, bool refresh = false) : 
            base(uiDoc, labelSize, transactionBuilder, refresh)
        {
        }

        public void Show(Point3D point)
        {
            XYZ xYZPoint = new XYZ(point.X, point.Y, point.Z);
            Show(xYZPoint);
        }

        public void ShowVector(Point3D point1, Point3D point2)
        {
            throw new System.NotImplementedException();
        }

        public void ShowVectorByDirection(Point3D origin, Point3D direction)
        {
            throw new System.NotImplementedException();
        }
    }
}
