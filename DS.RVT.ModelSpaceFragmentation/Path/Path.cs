using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Various;
using DS.RVT.ModelSpaceFragmentation.Lines;
using FrancoGustavo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Transform = Rhino.Geometry.Transform;

namespace DS.RVT.ModelSpaceFragmentation
{
    static class Path
    {
        public static List<Point3D> Refine(List<PointPathFinderNode> path)
        {
            List<Point3D> points = new List<Point3D>();
            if (path == null || path.Count == 0)
            { return points; }

            var firstNode = path[0];
            Point3D basePoint = firstNode.Point;
            Vector3D baseDir = firstNode.Dir;

            for (int i = 1; i < path.Count; i++)
            {
                var currentNode = path[i];
                var currentPoint = currentNode.Point;
                var currentDir = path[i].Dir;
                if (currentDir.Length != 0)
                { currentDir.Normalize(); }

                if(baseDir.Length == 0 || !currentDir.IsAlmostEqualTo(baseDir))
                {
                    points.Add(basePoint);
                    baseDir = currentDir;
                }
                basePoint = currentPoint; 
            }

            points.Add(basePoint);

            return points;
        }

        public static List<XYZ> Convert(List<Point3D> path, IPoint3dConverter pointConverter)
        {
            //Convert path to revit coordinates                
            List<XYZ> pathCoords = new List<XYZ>();
            //pathCoords.Add(ElementInfo.StartElemPoint);

            foreach (var point in path)
            {
                //Point3D ucs1Point = point;
                var ucs1Point3d = point.Convert();
                ucs1Point3d = pointConverter.ConvertToUCS1(ucs1Point3d);
                var xYZ = new XYZ(ucs1Point3d.X, ucs1Point3d.Y, ucs1Point3d.Z);
                pathCoords.Add(xYZ);
            }

            return pathCoords;
        }

        public static void ShowPath(List<XYZ> pathCoords)
        {
            //pathCoords.Add(ElementInfo.EndElemPoint);

            //Show path with lines
            LineCreator lineCreator = new LineCreator();
            lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));

            //MEP system changing
            //RevitUtils.MEP.PypeSystem pypeSystem = new RevitUtils.MEP.PypeSystem(Main.Uiapp, Main.Uidoc, Main.Doc, Main.CurrentElement);
            //pypeSystem.CreatePipeSystem(pathCoords);
        }


        private static XYZ ConvertToModel(XYZ point)
        {
            XYZ newpoint = point.Multiply(InputData.PointsStepF);
            newpoint += InputData.ZonePoint1;

            return newpoint;
        }
    }
}
