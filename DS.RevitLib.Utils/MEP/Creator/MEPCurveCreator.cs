using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurveCreator
    {
        private readonly Document Doc;
        private readonly MEPCurve BaseMEPCurve;

        public MEPCurveCreator(Document doc, MEPCurve baseMEPCurve)
        {
            Doc = doc;
            BaseMEPCurve = baseMEPCurve;
        }


        #region Properties

        private ElementId MEPSystemTypeId
        {
            get
            {
                MEPCurve mEPCurve = BaseMEPCurve as MEPCurve;
                return mEPCurve.MEPSystem.GetTypeId();
            }
        }
        private ElementId ElementTypeId
        {
            get
            {
                return BaseMEPCurve.GetTypeId();
            }
        }
        private string ElementTypeName
        {
            get { return BaseMEPCurve.GetType().Name; }
        }
        private ElementId MEPLevelId
        {
            get
            {
                return new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault().Id;
            }
        }

        #endregion


        /// <summary>
        /// Create pipe between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByPoints(XYZ p1, XYZ p2, MEPCurve baseMEPCurve = null)
        {
            MEPCurve mEPCurve = null;

            if(baseMEPCurve is null)
            {
                baseMEPCurve = BaseMEPCurve;
            }

            using (Transaction transNew = new Transaction(Doc, "CreateMEPCurveByPoints"))
            {
                try
                {
                    transNew.Start();
                    if (ElementTypeName == "Pipe")
                    {
                        mEPCurve = Pipe.Create(Doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);
                    }
                    else
                    {
                        mEPCurve = Duct.Create(Doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);
                    }

                    Insulation.Create(baseMEPCurve, mEPCurve);
                    MEPCurveParameter.Copy(baseMEPCurve, mEPCurve);
                }

                catch (Exception e)
                { }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }
            return mEPCurve;
        }


        public static void Swap(MEPCurve targetMEPCurve, MEPCurve baseMEPCurve)
        {
            if (targetMEPCurve.IsRecangular() &&
            !MEPCurveUtils.IsDirectionEqual(baseMEPCurve, targetMEPCurve) &&
            IsWrongSize(baseMEPCurve, targetMEPCurve))
            {
                MEPCurveUtils.SwapSize(targetMEPCurve);
            }

        }

        /// <summary>
        /// Create pipe between 2 connectors
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByConnectors(Connector c1, Connector c2)
        {
            MEPCurve mEPCurve = null;
            using (Transaction transNew = new Transaction(Doc, "CreateMEPCurveByConnectors"))
            {
                try
                {
                    transNew.Start();
                    if (ElementTypeName == "Pipe")
                    {
                        mEPCurve = Pipe.Create(Doc, MEPSystemTypeId, ElementTypeId, c1, c2);
                    }
                    else
                    {
                        mEPCurve = Duct.Create(Doc, MEPSystemTypeId, ElementTypeId, c1, c2);
                    }

                    Insulation.Create(BaseMEPCurve, mEPCurve);
                    MEPCurveParameter.Copy(BaseMEPCurve, mEPCurve);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

            return mEPCurve;
        }

        public static Element SplitElement(Document Doc, Element element, XYZ splitPoint)
        {
            Element newElement = null;
            var elementTypeName = element.GetType().Name;

            using (Transaction transNew = new Transaction(Doc, "SplitElement"))
            {
                try
                {
                    transNew.Start();

                    ElementId newCurveId;
                    if (elementTypeName == "Pipe")
                    {
                        newCurveId = PlumbingUtils.BreakCurve(Doc, element.Id, splitPoint);
                    }
                    else
                    {
                        newCurveId = MechanicalUtils.BreakCurve(Doc, element.Id, splitPoint);
                    }
                    newElement = Doc.GetElement(newCurveId);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return newElement;
        }



        private static bool IsWrongSize(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            Plane plane = MEPCurveUtils.GetPlane(mEPCurve, baseMEPCurve);

            if (plane is null)
            {
                return false;
            }

            double baseSize = GetSizeInPlane(baseMEPCurve, plane);
            double size = GetSizeInPlane(mEPCurve, plane);

            if (Math.Abs(baseSize - size) < 0.001)
            {
                return false;
            }

            return true;
        }

        private static List<XYZ> GetVectorsInPlane(Plane plane, List<XYZ> vectors, XYZ centerPoint)
        {
            List<XYZ> planeVectors = new List<XYZ>();
            foreach (var vector in vectors)
            {
                XYZ cpVector = centerPoint + vector;
                if (cpVector.IsPointOntoPlane(plane))
                {
                    planeVectors.Add(vector);
                }
            }

            return planeVectors;
        }

        private static double GetSize(MEPCurve mEPCurve, List<XYZ> normVectorsInPlane)
        {
            List<Solid> elemSolids = ElementUtils.GetSolids(mEPCurve);
            Solid elemSolid = elemSolids.First();

            XYZ centerPoint = ElementUtils.GetLocationPoint(mEPCurve);

            var locCurve = mEPCurve.Location as LocationCurve;
            Line mEPCurveline = locCurve.Curve as Line;

            Line intersectLine = Line.CreateBound(centerPoint, centerPoint + normVectorsInPlane.First().Multiply(100));

            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            //Get intersections with curve
            SolidCurveIntersection intersection = elemSolid.IntersectWithCurve(intersectLine, intersectOptions);

            XYZ intersectionPoint = null;
            if (intersection.SegmentCount != 0)
            {
                XYZ p1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ p2 = intersection.GetCurveSegment(0).GetEndPoint(1);

                (XYZ minPoint, XYZ maxPoint) = PointUtils.GetMinMaxPoints(new List<XYZ> { p1, p2 }, mEPCurveline);
                intersectionPoint = maxPoint;
            }

            return mEPCurveline.Distance(intersectionPoint);
        }

        private static double GetSizeInPlane(MEPCurve mEPCurve, Plane plane)
        {
            List<XYZ> baseNormOrthoVectors = MEPCurveUtils.GetOrthoNormVectors(mEPCurve);
            List<XYZ> baseVectorsInPlane = GetVectorsInPlane(plane, baseNormOrthoVectors, ElementUtils.GetLocationPoint(mEPCurve));

            return GetSize(mEPCurve, baseVectorsInPlane);
        }


    }
}
