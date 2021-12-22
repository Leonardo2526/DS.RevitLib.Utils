using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitUtilsTools
{
    class ElementUtils
    {
        public Element GetCurrent(IElement ielement)
        {
            return ielement.GetElement();
        }

        public void OutputInfo(IOutputElementInfo elementInfo)
        {
            elementInfo.GetInfo();
        }

        public ICollection<ElementId> GetIdCollection(Element element)
        {
            ICollection<ElementId> elementId = new List<ElementId>
            {
                element.Id
            };

            return elementId;
        }

        public List<Solid> GetSolids(Element element)
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

        public void GetPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
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

        public XYZ GetLocationInFeets(Element element)
        {
            LocationPoint LP = element.Location as LocationPoint;
            double xFeet = Math.Round(LP.Point.X, 3);
            double yFeet = Math.Round(LP.Point.Y, 3);
            double zFeet = Math.Round(LP.Point.Z, 3);

            XYZ point = new XYZ(xFeet, yFeet, zFeet);

            return point;
        }

        public XYZ GetLocationInMM(Element element)
        {
            GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            double X = UnitUtils.Convert(centerPoint.X,
                                           DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_METERS);
            double Y = UnitUtils.Convert(centerPoint.Y,
                                          DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_METERS);
            double Z = UnitUtils.Convert(centerPoint.Z,
                                            DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_METERS);
            int X_MM = (int)Math.Round(1000 * X);
            int Y_MM = (int)Math.Round(1000 * Y);
            int Z_MM = (int)Math.Round(1000 * Z);

            XYZ point = new XYZ(X_MM, Y_MM, Z_MM);

            return point;
        }

    }
}
