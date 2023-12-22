using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Solids
{
    internal static class SolidExtractor
    {
        public static List<Solid> GetSolids(Element element, XYZ moveVector = null, Options geomOptions = null)
        {
            List<Solid> solids = new List<Solid>();

            Options options = geomOptions ?? new Options() { DetailLevel = ViewDetailLevel.Fine };
            GeometryElement geomElem = element.get_Geometry(options);

            if (geomElem == null)
                return null;

            if (moveVector != null)
            {
                Transform transform = Transform.CreateTranslation(moveVector);
                geomElem = geomElem.GetTransformed(transform);
            }

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        solids.Add(solid);
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                solids.Add(solid);
                            }
                        }
                    }
                }
            }

            return solids;
        }
    }
}
