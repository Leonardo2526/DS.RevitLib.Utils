using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace DS.Revit.Utils
{
    public class ElementUtils
    {
        /// <summary>
        /// Get points of central line of the element.
        /// </summary>
        public static void GetPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
        {
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            startPoint = c.GetEndPoint(0);
            endPoint = c.GetEndPoint(1);
            centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

        }

        /// <summary>
        /// Get center point location of element in millimeters.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetLocation(Element element)
        {
            GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            double X = UnitUtils.Convert(centerPoint.X,
                                           DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_MILLIMETERS);
            double Y = UnitUtils.Convert(centerPoint.Y,
                                          DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_MILLIMETERS);
            double Z = UnitUtils.Convert(centerPoint.Z,
                                            DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_MILLIMETERS);
            int X_MM = (int)Math.Round(X);
            int Y_MM = (int)Math.Round(Y);
            int Z_MM = (int)Math.Round(Z);

            XYZ point = new XYZ(X_MM, Y_MM, Z_MM);

            return point;
        }

        public static List<Solid> GetSolids(Element element)
        {
            List<Solid> solids = new List<Solid>();

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = element.get_Geometry(options);

            if (geomElem == null)
                return null;

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

        public static List<Solid> GetSolidsOfElements(List<Element> elements)
        {
            List<Solid> solids = new List<Solid>();

            foreach (Element element in elements)
            {
                List<Solid> elementSolids = GetSolids(element);
                solids.AddRange(elementSolids);
            }

            return solids;
        }

        private static List<Solid> solids = new List<Solid>();

        private static List<Solid> GetSolidsFromGeometryObject(GeometryElement geomElem)
        {
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                        solids.Add(solid);
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();

                    solids.AddRange(GetSolidsFromGeometryObject(instGeomElem));

                }
            }

            return solids;
        }

        public static List<Solid> GetTransformedSolids(Element element, XYZ moveVector)
        {
            List<Solid> solids = new List<Solid>();

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = element.get_Geometry(options);

            if (geomElem == null)
                return null;

            Transform transform = Transform.CreateTranslation(moveVector);
            GeometryElement geomElemTransformed = geomElem.GetTransformed(transform);

            foreach (GeometryObject geomObj in geomElemTransformed)
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

        public static List<Solid> GetTransformSolidsOfElements(List<Element> elements, XYZ moveVector)
        {
            List<Solid> solids = new List<Solid>();

            foreach (Element element in elements)
            {
                List<Solid> elementSolids = GetTransformedSolids(element, moveVector);
                solids.AddRange(elementSolids);
            }

            return solids;
        }
    }
}
