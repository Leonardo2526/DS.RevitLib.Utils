using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisons;
using DS.RevitLib.Utils.Extensions;
using MoreLinq;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Collisions
{
    public static class SolidElementRulesFilterSet
    {
        public static Func<(Solid, Element), bool> WallTraversableDirectionRule(Vector3d traverseDirection = default)
        {
            return f => IsTraversableDirection(f, traverseDirection);
        }

        public static Func<(Solid, Element), bool> WallConstructionRule(Document doc)
        {
            return f => IsConstructionWall(f, doc);
        }

        public static Func<(Solid, Element), bool> WallDistanceToEdgeRule(Document doc, double maxEdgeLength)
        {
            return f => IsDistanceToEdgeValid(f, doc, maxEdgeLength);
        }





        #region PrivateMethods

        private static bool IsDistanceToEdgeValid((Solid, Element) f, Document activeDoc, double maxEdgeLength)
        {
            var s = f.Item1;
            var elem = f.Item2;
            var elemSolid = elem.Solid();

            if (elem is not Wall wall) { return true; }
            var wLine = wall.GetCenterLine();

            var intersectionSolid = CollisionUtils.GetIntersectionSolid(activeDoc, activeDoc, s, elem.Document, elemSolid);
            (XYZ point1, XYZ point2) = intersectionSolid.GetEdgeProjectPoints(wLine);

            var wp1 = wLine.GetEndPoint(0);
            var wp2 = wLine.GetEndPoint(1);

            if(
                point1.DistanceTo(wp1) < maxEdgeLength
                || point1.DistanceTo(wp2) < maxEdgeLength
                || point2.DistanceTo(wp1) < maxEdgeLength
                || point2.DistanceTo(wp2) < maxEdgeLength
                )
            {  return false; }

            return true;
        }

        private static bool IsConstructionWall((Solid, Element) f, Document doc)
        {
            var e1 = f.Item1;
            var e2 = f.Item2;

            if (e2 is not Wall wall) { return true; }

            string path;
            if (doc.IsWorkshared)
            {
                var modelPath = doc.GetWorksharingCentralModelPath();
                path = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
            }
            else { path = wall.GetLink(doc)?.Document.PathName ?? doc.PathName; }
            string lastFolderName = Path.GetFileName(Path.GetDirectoryName(path));

            return lastFolderName.Contains("КР") || lastFolderName.Contains("KR");
        }

        private static bool IsTraversableDirection((Solid, Element) f, Vector3d traverseDirection)
        {
            var e1 = f.Item1;
            var e2 = f.Item2;

            if (e2 is not Wall wall) { return true; }

            var wDir = wall.GetCenterLine().Direction.ToVector3d();
            wDir = Vector3d.Divide(wDir, wDir.Length);
            return traverseDirection.IsPerpendicularTo(wDir, 3.DegToRad());
        }

        #endregion


    }
}
