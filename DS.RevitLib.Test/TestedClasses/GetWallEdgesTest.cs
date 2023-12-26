using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Filters;
using DS.RevitCollisions.CollisionBuilers;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
namespace DS.RevitLib.Test.TestedClasses
{
    internal class GetWallEdgesTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trf;


        public GetWallEdgesTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _trf = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
        }

        public GetWallEdgesTest GetWallEdges()
        {
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;

            Options geomOptions = new Options()
            {
                ComputeReferences = true, // expensive, avoid if not needed
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };
            (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesDist) = wall.GetFaces(_doc, geomOptions, true);
            var mainWallFace = wallFaces.OfType<PlanarFace>().OrderByDescending(f => f.Area).First();

            Debug.WriteLine("wallFaces: " + wallFaces.Count);

            //show wall faces
            wallFaces.ForEach(ShowFaceAsEdges);
            var insertsFaces = insertsFacesDist.Values.SelectMany(x => x).ToList();

            //show inserts faces
            Debug.WriteLine("inserts: " + insertsFacesDist.Count);
            foreach (var kv in insertsFacesDist)
            {
                Debug.WriteLine("insertFaces: " + kv.Value.Count);
            }
            insertsFaces.ForEach(ShowFaceAsEdges);

            return this;
        }

        public (Rectangle3d wallRect, IEnumerable<Rectangle3d> openingsRect) GetWallMainFaceEdges()
        {
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;

            Options geomOptions = new Options()
            {
                ComputeReferences = true, // expensive, avoid if not needed
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };
            (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesDist) = wall.GetFaces(_doc, geomOptions, true);
            var mainWallFace = wallFaces.OfType<PlanarFace>().OrderByDescending(f => f.Area).First();
            var mainRectangle = Rectangle3dFactoty.Create(mainWallFace);

            var offset = -1000.MMToFeet();
            if (!mainRectangle.TryExtend(offset, out var extendedMainRectangle))
            { throw new Exception(""); }
            _trf.CreateAsync(() => extendedMainRectangle.Show(_doc), "ShowWallEdges");

            //show face
            var insertsFaces = insertsFacesDist.Values.SelectMany(x => x).ToList();

            _uiDoc.RefreshActiveView();
            //return this;
            //show inserts faces on main
            var edgesDict = GeometryElementsUtils.GetEdgesOnOpenings(mainWallFace, wall);
            var openingsRectangles = ToRectangles(edgesDict);

            var openingOffset = 500.MMToFeet();
            var extendedOpeningsRectangles = GetExtendedRectangles(openingsRectangles, openingOffset);
            _trf.CreateAsync(() => { extendedOpeningsRectangles.ForEach(r => r.Show(_doc)); }, "ShowWallEdges");

            return (extendedMainRectangle, extendedOpeningsRectangles);
        }



        private static List<Rectangle3d> ToRectangles(Dictionary<ElementId, List<Autodesk.Revit.DB.Curve>> edgesDict)
        {
            var rectangles = new List<Rectangle3d>();
            foreach (var item in edgesDict)
            {
                var rectangle = Rectangle3dFactoty.Create(item.Value);
                rectangles.Add(rectangle);
            }
            return rectangles;
        }

        private void ShowFaceAsEdges(Face face)
        {
            if (face is PlanarFace planarFace)
            {
                var rectangle = Rectangle3dFactoty.Create(planarFace);
                _trf.CreateAsync(() => rectangle.Show(_doc), "ShowWallEdges");
            }
        }

        private List<Rectangle3d> GetExtendedRectangles(List<Rectangle3d> rectangles, double offset)
        {
            var extended = new List<Rectangle3d>();

            foreach (var rect in rectangles)
            {
                if (!rect.TryExtend(offset, out var exRectangle))
                { throw new Exception(""); }
                extended.Add(exRectangle);
            }

            return extended;
        }

        public void GetJoints()
        {
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;
            var jointsIds = wall.GetJoints();

            var mainFace = wall.GetMainPlanarFace(_doc);
            var mainRhinoPlane = mainFace.GetPlane().ToRhinoPlane();

            Options geomOptions = new Options()
            {
                ComputeReferences = true, // expensive, avoid if not needed
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };

            var offset = 500.MMToFeet();
            foreach (var id in jointsIds)
            {
                if (_doc.GetElement(id) is Wall jWall)
                {
                    (List<Face> jwallFaces, Dictionary<ElementId, List<Face>> jInsertsFacesDist) = jWall.GetFaces(_doc, geomOptions, false);
                    var pFaces = jwallFaces.OfType<PlanarFace>().OrderByDescending(w => w.Area).ToList();
                    pFaces.RemoveRange(0, 2);
                    var at = 3.DegToRad();
                    var perpFaces = pFaces.Where(f => f.FaceNormal.ToVector3d().IsPerpendicularTo(Vector3d.ZAxis, at)).ToList();

                    var rect1 = Rectangle3dFactoty.Create(perpFaces[0]);
                    var rect2 = Rectangle3dFactoty.Create(perpFaces[1]);
                    var rect = mainRhinoPlane.DistanceTo(rect1.Center) < mainRhinoPlane.DistanceTo(rect2.Center) ?
                        rect1 : rect2;

                    var p1 = rect.Corner(0);
                    var p2 = rect.Corner(2);
                    rect = new Rectangle3d(mainRhinoPlane, p1, p2);
                    if (!rect.TryExtend(offset, out var extendedMainRectangle))
                    { throw new Exception(""); }
                    _trf.CreateAsync(() => extendedMainRectangle.Show(_doc), "ShowWallEdges");
                    _trf.CreateAsync(() => rect.Show(_doc), "ShowWallEdges");
                }
            }

        }

        public void GetJointsOld()
        {
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;
            var jointsIds = wall.GetJoints();

            var mainFace = wall.GetMainPlanarFace(_doc);
            var mainRhinoPlane = mainFace.GetPlane().ToRhinoPlane();

            Options geomOptions = new Options()
            {
                ComputeReferences = true, // expensive, avoid if not needed
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };

            var projPoints = new List<XYZ>();
            foreach (var id in jointsIds)
            {
                if (_doc.GetElement(id) is Wall jWall)
                {
                    (List<Face> jwallFaces, Dictionary<ElementId, List<Face>> jInsertsFacesDist) = jWall.GetFaces(_doc, geomOptions, false);
                    var closestFace = jwallFaces.OfType<PlanarFace>().
                        OrderByDescending(w => mainRhinoPlane.DistanceTo(w.Origin.ToPoint3d())).Last();
                    var rect = Rectangle3dFactoty.Create(closestFace);
                    var p1 = rect.Corner(0);
                    var p2 = rect.Corner(2);
                    rect = new Rectangle3d(mainRhinoPlane, p1, p2);

                    //List<EdgeArray> edgeArrays = GeometryElementsUtils.GetEdgeArrays(wallFaces);
                    //var curves = GeometryElementsUtils.GetCurves(edgeArrays);
                    //var points= new List<XYZ>();
                    //curves.ForEach(c => points.AddRange(c.Tessellate()));

                    //var wProjPoints = new List<XYZ>();

                    //foreach (var p in points)
                    //{
                    //    var pp = mainFace.Project(p).XYZPoint;
                    //    if (pp != null)
                    //    { projPoints.Add(pp); }
                    //}

                    var jhLine = jWall.GetHeightLine(_doc);
                    var center = jWall.GetCenterLine().GetCenter();
                    var projCenter = mainFace.Project(center).XYZPoint;


                }
            }

            var rhinoPoints = new List<Point3d>();
            projPoints.ForEach(p => rhinoPoints.Add(p.ToPoint3d()));

            //get joints
            var l = wall.Location as LocationCurve;
            var j0 = GetAdjoiningElements(l, wall.Id, 0);
            var j1 = GetAdjoiningElements(l, wall.Id, 1);
            var j2 = GetAdjoiningElements(l, wall.Id, 2);
            var j3 = GetAdjoiningElements(l, wall.Id, 3);
        }

        /// <summary>
        /// Create a dictionary of adjoinging walls keyed on a particular wall ID.
        /// </summary>
        /// <param name="document">Tje Revit API Document</param>
        /// <returns>A dictionary keyed on a wall id containing a collection of walls that adjoin the key id wall</returns>
        private IDictionary<ElementId, ICollection<Wall>> GetAdjoiningWallsMap(Document document)
        {
            IDictionary<ElementId, ICollection<Wall>> result = new Dictionary<ElementId, ICollection<Wall>>();

            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(Wall));
            foreach (Wall wall in collector.Cast<Wall>())
            {
                IEnumerable<Element> joinedElements0 = GetAdjoiningElements(wall.Location as LocationCurve, wall.Id, 0);
                IEnumerable<Element> joinedElements1 = GetAdjoiningElements(wall.Location as LocationCurve, wall.Id, 1);
                result[wall.Id] = joinedElements0.Union(joinedElements1).OfType<Wall>().ToList();
            }
            return result;
        }

        private IEnumerable<Element> GetAdjoiningElements(LocationCurve locationCurve, ElementId wallId, Int32 index)
        {
            IList<Element> result = new List<Element>();
            ElementArray a = locationCurve.get_ElementsAtJoin(index);
            foreach (Element element in a)
                if (element.Id != wallId)
                    result.Add(element);
            return result;
        }
    }
}
