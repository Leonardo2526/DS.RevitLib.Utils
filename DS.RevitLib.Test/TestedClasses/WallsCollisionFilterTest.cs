using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Filters;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class WallsCollisionFilterTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trf;
        private bool _checkTraverseDirection = true;
        private double _maxEdgeLength = 1000.MMToFeet();


        public WallsCollisionFilterTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _trf = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
        }

        public void RunCase1()
        {
            var mEPCurve = new ElementSelector(_uiDoc).Pick() as MEPCurve;
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;



            //_trf.CreateAsync(() => 
            //{ 
            //    //wallSolid.ShowShape(_doc); 
            //    //wallSolid.ShowEdges(_doc);
            //    foreach (Edge edge in edges)
            //    {
            //        var curve = edge.AsCurve();
            //        curve.Show(_doc);
            //    }
            //}, 
            //"show edges");


            var collisions = new List<(Solid, Element)>();
            var collision = (mEPCurve.Solid(), wall);
            collisions.Add(collision);

            if (_checkTraverseDirection)
            {
                var dir = mEPCurve.Direction().ToVector3d();
                var rools = new List<Func<(Solid, Element), bool>>
                {SolidElementRulesFilterSet.WallTraversableDirectionRule(dir)};
                Func<(Solid, Element), bool> ruleCollisionFilter = new RulesFilterFactory<Solid, Element>(rools).GetFilter();
                collisions = collisions.Where(ruleCollisionFilter).ToList();
            }

            var filter = GetFilter(_doc);
            collisions = collisions.Where(filter).ToList();

            Debug.WriteLine("Collisions count is: " + collisions.Count);
        }

        private Func<(Solid, Element), bool> GetFilter(Document doc)
        {
            var rools = new List<Func<(Solid, Element), bool>>
            {
                SolidElementRulesFilterSet.WallDistanceToEdgeRule(doc, _maxEdgeLength),
                //SolidElementRulesFilterSet.WallConstructionRule(doc)
            };
            return new RulesFilterFactory<Solid, Element>(rools).GetFilter();
        }


        private void CheckDist()
        {
            Autodesk.Revit.DB.Curve acurve1 = null;
            Autodesk.Revit.DB.Curve acurve2 = null;
            Point3d point3d;
            Rhino.Geometry.Line line1;
            Rhino.Geometry.Line line2;

            Rhino.Geometry.Curve curve1;
            Rhino.Geometry.Curve curve2;
        }

        public void GetWallEdges()
        {
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;

            var planarFaces = GetWallPlanarFaces(wall);

            //var wDir = wall.GetCenterLine().Direction.Normalize();
            var mainFace = planarFaces.OrderByDescending(f => f.Area).First();

            //var uvPoint = new UV();
            ////var wallNormal = wDir.CrossProduct(XYZ.BasisZ).ToVector3d();
            //var wallNormal = mainFace.ComputeNormal(uvPoint).ToVector3d();

            //var at = 3.DegToRad();
            //planarFaces = planarFaces.Where(f => f.ComputeNormal(uvPoint).ToVector3d().IsParallelTo(wallNormal, at) != 0).ToList();
            //if (planarFaces.Count == 2)
            //{ planarFaces.RemoveAt(0); }

            //var wallMainFace = planarFaces.First();

            //var edges = planarFaces.FirstOrDefault().EdgeLoops;

            var facesToShow = new List<Face>() { mainFace };
            var insertsIds = wall.FindInserts(true, false, true, true);
            var edgeArrays = GeometryElementsUtils.GetEdgeArrays(facesToShow);

            var curves = new List<Autodesk.Revit.DB.Curve>();
            foreach (EdgeArray edgeArray in edgeArrays)
            {
                for (int i = 0; i < edgeArray.Size; i++)
                {
                    Edge edge = edgeArray.get_Item(i);
                    if (wall.GetGeneratingElementIds(edge).Any(insertsIds.Contains))
                    { continue; }
                    var curve = edge.AsCurve();
                    curves.Add(curve);
                }
            }

            //edgeArrays = edgeArrays.Where(c => !wall.GetGeneratingElementIds(c).Any(x => insertsIds.Contains(x))).ToList();

            //var curves = GeometryElementsUtils.GetCurves(edgeArrays);

            //curves = curves.Where(c => !wall.GetGeneratingElementIds(c).Any(x => insertsIds.Contains(x))).ToList();         

            _trf.CreateAsync(() =>
            {
                curves.ForEach(obj => obj.Show(_doc));
            }, "ShowOpeningEdges");

            _uiDoc.RefreshActiveView();

            GetOpeningsEdges(wall, mainFace);
        }

        public void GetOpeningsEdges(Wall wall, Face mainFace)
        {
            //var wallElem =  new ElementSelector(_uiDoc).Pick();
            //var wall = wallElem as Wall;

            //var wallSolid = wallElem.Solid();

            var insertsIds = wall.FindInserts(true, false, true, true);
            foreach (var insertId in insertsIds)
            {
                var planarFaces = GetWallOpeningPlanarFaces(wall, insertId);
                //var edges = planarFaces.FirstOrDefault().EdgeLoops;                
                var edgeArrays = GeometryElementsUtils.GetEdgeArrays(planarFaces);
                var curves = GeometryElementsUtils.GetCurves(edgeArrays);

                var mainCurves = new List<Autodesk.Revit.DB.Curve>();
                foreach (var curve in curves)
                {
                    var center = curve.GetCenter();
                    var pCenter = mainFace.Project(center)?.XYZPoint;
                    if (pCenter != null && center.DistanceTo(pCenter) < 0.001)
                    {
                        mainCurves.Add(curve);
                    }
                }

                _trf.CreateAsync(() =>
                {
                    mainCurves.ForEach(obj => obj.Show(_doc));
                }, "ShowOpeningEdges");
            }

            //wallSolid.ShowEdges(_doc);
        }

        /// <summary>
        /// Retrieve all planar faces belonging to the 
        /// specified opening in the given wall.
        /// </summary>
        static List<Face> GetWallOpeningPlanarFaces(
          Wall wall,
          ElementId openingId)
        {
            List<Face> faceList = new List<Face>();

            List<Solid> solidList = new List<Solid>();

            Options geomOptions = wall.Document.Application.Create.NewGeometryOptions();

            if (geomOptions != null)
            {
                geomOptions.ComputeReferences = true; // expensive, avoid if not needed
                geomOptions.DetailLevel = ViewDetailLevel.Fine;
                geomOptions.IncludeNonVisibleObjects = false;

                GeometryElement geoElem = wall.get_Geometry(geomOptions);

                if (geoElem != null)
                {
                    foreach (GeometryObject geomObj in geoElem)
                    {
                        if (geomObj is Solid)
                        {
                            solidList.Add(geomObj as Solid);
                        }
                    }
                }
            }

            foreach (Solid solid in solidList)
            {
                foreach (Face face in solid.Faces)
                {
                    //if (face is PlanarFace)
                    {
                        if (wall.GetGeneratingElementIds(face)
                          .Any(x => x == openingId))
                        {
                            faceList.Add(face);
                        }
                    }
                }
            }
            return faceList;
        }

        /// <summary>
        /// Retrieve all planar faces belonging to the 
        /// specified opening in the given wall.
        /// </summary>
        static List<Face> GetWallPlanarFaces(
          Wall wall)
        {
            List<Face> faceList = new List<Face>();

            List<Solid> solidList = new List<Solid>();

            Options geomOptions = wall.Document.Application.Create.NewGeometryOptions();

            if (geomOptions != null)
            {
                geomOptions.ComputeReferences = true; // expensive, avoid if not needed
                geomOptions.DetailLevel = ViewDetailLevel.Fine;
                geomOptions.IncludeNonVisibleObjects = false;

                GeometryElement geoElem = wall.get_Geometry(geomOptions);

                if (geoElem != null)
                {
                    foreach (GeometryObject geomObj in geoElem)
                    {
                        if (geomObj is Solid)
                        {
                            solidList.Add(geomObj as Solid);
                        }
                    }
                }
            }

            var insertsIds = wall.FindInserts(true, false, true, true);

            foreach (Solid solid in solidList)
            {
                foreach (Face face in solid.Faces)
                {
                    if (wall.GetGeneratingElementIds(face)
                      .Any(x => insertsIds.Contains(x)))
                    { continue; }
                    faceList.Add(face);
                }
            }
            return faceList;
        }
    }
}
