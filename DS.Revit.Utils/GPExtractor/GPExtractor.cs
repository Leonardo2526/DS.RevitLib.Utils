using Autodesk.Revit.DB;
using System.Collections.Generic;



namespace DS.Revit.Utils.GPExtractor
{
    class GPExtractor
    {

        public Element element { get; set; }

        public GPExtractor(Element elem)
        {
            element = elem;
        }

        public void GetGeneralPoints(out List<XYZ> points)
        {
            //Get element's solid
            List<Solid> solids = ElementUtils.GetSolids(element);
            points = new List<XYZ>();

            Solid elementSolid = null;
            foreach (Solid solid in solids)
            {
                elementSolid = solid;

                foreach (Face face in elementSolid.Faces)
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
            }    


        }

        bool CheckPoint(XYZ newPoint, List<XYZ> points)
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
