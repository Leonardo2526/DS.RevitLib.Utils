using Autodesk.Revit.DB;
using System.Collections.Generic;



namespace DS.RevitLib.Utils.GPExtractor
{
    public static class GPExtractor
    {
        public static List<XYZ> GetGeneralPoints(List<Solid> solids)
        {
            var points = new List<XYZ>();

            foreach (Solid solid in solids)
            {
                points.AddRange(GetGeneralPoints(solid));
            }

            return points;
        }

        public static List<XYZ> GetGeneralPoints(Solid solid)
        {
            var points = new List<XYZ>();

            foreach (Face face in solid.Faces)
            {
                Mesh mesh = face.Triangulate();

                if (points.Count == 0)
                    points.Add(mesh.Vertices[0]);

                int i;
                for (i = 0; i < mesh.Vertices.Count; i++)
                {
                    XYZ newPoint = mesh.Vertices[i];

                    if (CheckPoint(newPoint, points))
                        points.Add(newPoint);
                }

            }


            return points;
        }

        static bool CheckPoint(XYZ newPoint, List<XYZ> points)
        {
            foreach (XYZ p in points)
            {
                if (newPoint.DistanceTo(p) < 0.01)
                    return false;

            }

            return true;
        }
    }
}
