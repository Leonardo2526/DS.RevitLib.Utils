using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils;
using Rhino.Geometry.Intersect;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Lines;
using DS.ClassLib.VarUtils;
using Rhino.UI;
using DS.ClassLib.VarUtils.Basis;
using DS.RevitLib.Utils.Various.Bases;
using System.Collections;
using System.Diagnostics;
using QuickGraph;
using System.Numerics;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class GetFaceIntersectionTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trf;

        public GetFaceIntersectionTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _trf = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
        }

        public void RunToDeleteAdjEdges()
        {
            var wall1 = new ElementSelector(_uiDoc).Pick() as Wall;
            var wall1Faces = GeometryElementsUtils.GetFaces(wall1, _doc);
            //var edges = GeometryElementsUtils.GetEdges(wall1, _doc).ToList();
            //_trf.CreateAsync(() => { edges.ForEach(r => r.Show(_doc)); }, "ShowInsertsEdges");
            //return;

            //(List<Face> wall1Faces, Dictionary<ElementId, List<Face>> insertsFacesCollection1) = wall1.GetFaces(_doc);
            var w1PlanarFaces = wall1Faces.OfType<PlanarFace>().ToList();
            var wall1YFaces = w1PlanarFaces.FindAll(FaceFilter.YNormal(wall1));

            var (freeEdges, joinElementsEdges) =  GeometryElementsUtils.GetSplitedEdges(wall1, _doc, true);
            var curvesValueResult = joinElementsEdges.Values.SelectMany(v => v).ToList();
            //var curvesValueResult = freeEdges.ToList();
            var edgesValueResult = curvesValueResult.OfType<Line>();
            //var edgesValueResult = faceEdges.Values.SelectMany(v => v).ToList();
            //_trf.CreateAsync(() => { edgesValueResult.ToList().ForEach(r => r.Show(_doc)); }, "ShowInsertsEdges");
            //return;

            //get projections
            var mainface = wall1YFaces[0];
            var mainPlane = mainface.GetPlane();
            var mainRhinoPlane = mainPlane.ToRhinoPlane();
            wall1.TryGetLocationLine(out var line);
            var origin = wall1.GetCenterPoint();
            var mainOrigin = mainPlane.ProjectOnto(origin);
            //var origin = line.PointAtLength(line.Length);
            //var projOrigin = mainface.Project(origin);
            //var mainOrigin = projOrigin?.XYZPoint;

            var projectEdgesValueResult = new List<Line>();
            foreach (var edge in edgesValueResult)
            {
                var proj = mainface.Project(edge, true);
                if(proj != null)
                {
                    projectEdgesValueResult.Add(proj);
                }
                //projectEdgesValueResult.Add(proj);
            }
            var adjancyRhinoEdges = GeometryElementsUtils.ToRhinoLines(projectEdgesValueResult);
            //_trf.CreateAsync(() => { projectEdgesValueResult.ForEach(r => r.Show(_doc)); }, "ShowInsertsEdges");
            //_uiDoc.RefreshActiveView();

            var maxLoop = mainface.GetOuterLoop();
            var mainFaceEdges = maxLoop.Select(x => x).OfType<Line>();
            //var mainRectangle= Rectangle3dFactoty.Create(mainface);
            //var mainFaceCurves = mainRectangle.ToLines();
            //var mainFaceCurves = mainface.GetEdges();

            //_trf.CreateAsync(() => mainface.ShowEdges(_doc), "ShowInsertsEdges");
            //_uiDoc.RefreshActiveView();

            //var mainFaceEdges = mainFaceCurves.OfType<Line>();
            var mainSubstractEdges = new List<Rhino.Geometry.Line>();
           
            var mainRhinoLines = GeometryElementsUtils.ToRhinoLines(mainFaceEdges);
            foreach (var rl in mainRhinoLines)
            {
                var subs = LineBooleanTools.Substract(rl, adjancyRhinoEdges.ToList());
                mainSubstractEdges.AddRange(subs);
            }


            //var mainSubstractEdges = Substract(
            //    GeometryElementsUtils.ToRhinoLines(mainFaceEdges), 
            //    GeometryElementsUtils.ToRhinoLines(projectEdgesValueResult)
            //    );

            _trf.CreateAsync(() =>
            {
                GeometryElementsUtils.ToRevitLines(mainSubstractEdges).ToList().
                ForEach(r => r.Show(_doc));
            }, "ShowInsertsEdges");
            //return;
            GetRectangles(mainSubstractEdges, mainOrigin.ToPoint3d(), mainRhinoPlane, 500.MMToFeet());
        }

        private IEnumerable<Rhino.Geometry.Line> Substract(IEnumerable<Rhino.Geometry.Line> line1, IEnumerable<Rhino.Geometry.Line> lines2)
        {
            var result = new List<Rhino.Geometry.Line>();

            foreach (var line in line1)
            {
                if (lines2.ToList().Any(x => x.Round() == line.Round()))
                { result.Add(line); }
            }

            return result;
        }

        private void GetRectangles(
            IEnumerable<Rhino.Geometry.Line> edges,
            Rhino.Geometry.Point3d baseOrigin,
            Rhino.Geometry.Plane plane,
            double offset)
        {
            var rectangles = new List<Rhino.Geometry.Rectangle3d>();
            foreach (var edge in edges)
            {
                var r = CreateRectangle(edge, baseOrigin, plane, offset);
                rectangles.Add(r);
                //break;
            }
            _trf.CreateAsync(() => rectangles.ForEach(r => r.Show(_doc)), "CreateRectangles");
        }

        private Rhino.Geometry.Rectangle3d CreateRectangle(
            Rhino.Geometry.Line edge,
            Rhino.Geometry.Point3d baseOrigin,
            Rhino.Geometry.Plane plane,
            double offset)
        {
            var eDir = edge.Direction;
            eDir.Unitize();
            var centerLine = new Rhino.Geometry.Line(baseOrigin, eDir);
            centerLine.Extend(1000, 1000);
            var p1 = edge.From;
            var cp = centerLine.ClosestPoint(p1, false);
            var cpLine = new Rhino.Geometry.Line(p1, cp);
            var rDir = cpLine.Direction;
            rDir.Unitize();

            var p2 = edge.To + Rhino.Geometry.Vector3d.Multiply(rDir, offset);
            return new Rhino.Geometry.Rectangle3d(plane, p1, p2);
        }

        public void Run1WithAdj()
        {
            var wall1 = new ElementSelector(_uiDoc).Pick() as Wall;
            //var wall2 = new ElementSelector(_uiDoc).Pick() as Wall;
            //Run1(wall1, wall2);

            var joints = wall1.GetJoints(true);
            joints.ToList().ForEach(j => Debug.WriteLine(j));
            foreach (var joint in joints)
            {
                var wall2 = _doc.GetElement(joint) as Wall;
                if (wall2 != null)
                {
                    Run1(wall1, wall2);
                }
            }
        }

        public void Run1(Wall wall1, Wall wall2)
        {

            (List<Face> wall1Faces, Dictionary<ElementId, List<Face>> insertsFacesCollection1) = wall1.GetFaces(_doc);
            (List<Face> wall2Faces, Dictionary<ElementId, List<Face>> insertsFacesCollection2) = wall2.GetFaces(_doc);

            var w1PlanarFaces = wall1Faces.OfType<PlanarFace>().ToList();
            var w2PlanarFaces = wall2Faces.OfType<PlanarFace>().ToList();

            var intersectCurves = new List<Curve>();

            var wall1YFaces = w1PlanarFaces.FindAll(FaceFilter.YNormal(wall1));
            var wall2YFaces = w2PlanarFaces.FindAll(FaceFilter.YNormal(wall2));

            foreach (var wf1 in wall1YFaces)
            {
                foreach (var wf2 in wall2YFaces)
                {
                    if (wf1.Intersect(wf2, out Curve result) == FaceIntersectionFaceResult.Intersecting)
                    {
                        var line = result as Line;
                        //if(IsContains(wf1, line) && IsContains(wf2, line))
                        { intersectCurves.Add(result); }
                    }
                }
                _trf.CreateAsync(() => { intersectCurves.ForEach(r => r.Show(_doc)); }, "ShowInsertsEdges");
            }
            //return;
            _uiDoc.RefreshActiveView();

            var intersectLines = intersectCurves.OfType<Line>();
            var projLines = new List<Line>();
            foreach (var line in intersectLines)
            {
                var projLine = wall1YFaces[0].Project(line, true);
                if (projLine != null)
                { projLines.Add(projLine); }
            }
            //_trf.CreateAsync(() => { projLines.ForEach(r => r.Show(_doc)); }, "ShowEdgesToOffset");

            var edgesToOffset = GetEdgesToOffset(wall1YFaces[0], projLines).ToList();
            _trf.CreateAsync(() => { edgesToOffset.ForEach(r => r.Show(_doc)); }, "ShowEdgesToOffset");
        }

        //public void Run2()
        //{
        //    var at = 3.DegToRad();

        //    var wall1 = new ElementSelector(_uiDoc).Pick() as Wall;
        //    (List<Face> wall1Faces, Dictionary<ElementId, List<Face>> insertsFacesCollection1) = wall1.GetFaces(_doc);

        //    var wloc1 = wall1.GetCenterLine();
        //    var xDir1 = wloc1.Direction.ToVector3d();
        //    xDir1.Unitize();
        //    var wall1PlanarFaces = wall1Faces.OfType<PlanarFace>().
        //                Where(f => f.FaceNormal.ToVector3d().IsParallelTo(Rhino.Geometry.Vector3d.ZAxis, at) == 0 &&
        //                f.FaceNormal.ToVector3d().IsParallelTo(xDir1, at) == 0);
        //    //var rect1 = Rectangle3dFactoty.Create(wall1PlanarFaces[0]);
        //    //var rect2 = Rectangle3dFactoty.Create(wall1PlanarFaces[1]);
        //    //var edges1 = rect1.ToLines();
        //    //var edges2 = rect2.ToLines();


        //    var allJoints = new List<ElementId>();
        //    var wJoints = wall1.GetJoints();
        //    var fJoints = JoinGeometryUtils.GetJoinedElements(_doc, wall1);
        //    allJoints.AddRange(wJoints);
        //    //allJoints.AddRange(fJoints);
        //    var lines = new List<Line>();
        //    foreach (var joint in allJoints)
        //    {
        //        var wall2 = _doc.GetElement(joint) as Wall;
        //        if (wall2 != null)
        //        {
        //            (List<Face> wall2Faces, Dictionary<ElementId, List<Face>> insertsFacesCollection2) = wall2.GetFaces(_doc);
        //            var wloc = wall2.GetCenterLine();
        //            var xDir = wloc.Direction.ToVector3d();
        //            xDir.Unitize();
        //            var wall2PlanarFaces = wall2Faces.OfType<PlanarFace>().
        //                Where(f => f.FaceNormal.ToVector3d().IsParallelTo(Rhino.Geometry.Vector3d.ZAxis, at) == 0 &&
        //                f.FaceNormal.ToVector3d().IsParallelTo(xDir, at) == 0);
        //            var cfaces = GetCoinicidents(wall1PlanarFaces, wall2PlanarFaces, out List<Line> lines1);
        //            if (lines1.Count() > 0)
        //            {
        //                var maxLine = lines1.OrderByDescending(l => l.Length).First();
        //                //lines.Add(maxLine);
        //                lines.AddRange(lines1);
        //            }
        //        }
        //    }


        //    _trf.CreateAsync(() => { lines.ForEach(r => r.Show(_doc)); }, "ShowInsertsEdges");
        //}


        public void RunFacesFilter()
        {
            var wall1 = new ElementSelector(_uiDoc).Pick() as Wall;

            var b = wall1.TryGetBasis(out var basis);
            // var r = basis.IsRighthanded();
            // var o = basis.IsOrthonormal();
            //basis = basis.Round();
            //basis.Show(_uiDoc, 0, _trf);

            var (wallFaces, insertsFacesCollection) = wall1.GetFaces(_doc);
            var planarFaced = wallFaces.OfType<PlanarFace>().ToList();
            var xFilter = FaceFilter.XNormal(wall1);
            var yFilter = FaceFilter.YNormal(wall1);
            var zFilter = FaceFilter.ZNormal(wall1);

            var facesToShow = planarFaced.FindAll(yFilter);
            _trf.CreateAsync(() => { facesToShow.ForEach(f => f.ShowEdges(_doc)); }, "ShowFaces");
        }


        public void ProjectJointFace()
        {
            var wall1 = new ElementSelector(_uiDoc).Pick() as Wall;
            var (wallFaces, insertsFacesCollection) = wall1.GetFaces(_doc);
            var planarFaced = wallFaces.OfType<PlanarFace>().ToList();
            var yFilter = FaceFilter.YNormal(wall1);
            var wall1YFace = planarFaced.Find(yFilter);
            var wall1YFaceEdges = wall1YFace.GetEdges().ToList();
            var wall1YFacePoints = new List<XYZ>();
            wall1YFaceEdges.ForEach(e => wall1YFacePoints.AddRange(e.Tessellate()));

            var projPoints = new List<XYZ>();

            var joints = wall1.GetJoints();
            joints.ToList().ForEach(j => Debug.WriteLine(j));
            return;
            foreach (var joint in joints)
            {
                var wall2 = _doc.GetElement(joint) as Wall;
                if (wall2 != null)
                {
                    var (wall2Faces, inserts2FacesCollection) = wall2.GetFaces(_doc);
                    var wall2XFace = wall2Faces.OfType<PlanarFace>().ToList().
                        Find(FaceFilter.XNormal(wall2));
                    var points = wall2XFace.Tesselate();
                    projPoints.AddRange(Project(points, wall1YFace));
                }
            }
            _trf.CreateAsync(() => projPoints.ForEach(p => p.ShowPoint(_doc)), "ShowPoints");


            static List<XYZ> Project(IEnumerable<Rhino.Geometry.Point3d> points, Face face)
            {
                var projPoints = new List<XYZ>();

                var revitPoints = new List<XYZ>();
                points.ToList().ForEach(p => revitPoints.Add(p.ToXYZ()));
                foreach (var p in revitPoints)
                {
                    var projPoint = face.Project(p, true);
                    if (projPoint != null)
                    { projPoints.Add(projPoint); }
                }

                return projPoints;
            }

        }


        private bool IsContainsOld(PlanarFace planarFace, Line line)
        {
            var p1 = line.GetEndPoint(0).ToPoint3d();
            var p2 = line.GetEndPoint(1).ToPoint3d();

            var rect = Rectangle3dFactoty.Create(planarFace);
            return rect.ContainsStrict(p1) || rect.ContainsStrict(p2);
        }


        private IEnumerable<Line> GetEdgesToOffset(PlanarFace plane, IEnumerable<Line> intersectionLines)
        {
            var offsetLines = new List<Line>();

            //var mainRect = Rectangle3dFactoty.Create(plane);
            //var rectLines = mainRect.ToLines();
            var planeLines = plane.GetEdges().OfType<Line>();
            var rectLines = new List<Rhino.Geometry.Line>();
            planeLines.ToList().ForEach(l => rectLines.Add(l.ToRhinoLine()));

            var intersectionLines1 = intersectionLines.OrderByDescending(l => l.Length).ToList();
            intersectionLines1.RemoveRange(0, 2);
            //intersectionLines
            var interRhinoLines = new List<Rhino.Geometry.Line>();
            intersectionLines1.ToList().ForEach(l => interRhinoLines.Add(l.ToRhinoLine()));


            foreach (var rLine in rectLines)
            {
                bool substracted = false;
                foreach (var interLine in interRhinoLines)
                {
                    {
                        var result = LineBooleanTools.Substract(rLine, interLine);
                        if (result.Count == 1 && result[0].Length == rLine.Length)
                        { continue; }
                        var revitLines = new List<Line>();
                        result.ForEach(l => revitLines.Add(l.ToXYZ()));
                        offsetLines.AddRange(revitLines);
                        substracted = true;
                    }
                }
                if (!substracted)
                { offsetLines.Add(rLine.ToXYZ()); }
            }

            return offsetLines;
        }

        IEnumerable<Rhino.Geometry.Line> TrySubstract(Rhino.Geometry.Line sourceLine, IEnumerable<Rhino.Geometry.Line> substractedLines)
        {
            var lines = new List<Rhino.Geometry.Line>();

            foreach (var line in substractedLines)
            {
                var result = LineBooleanTools.Substract(sourceLine, line);
                //if (result.Count == 0 || result.Count > 1) { return true; }
                if (result.Count == 1 && result.First().Length == sourceLine.Length)
                { continue; }
            }

            return lines;
        }


        //private IEnumerable<(PlanarFace, PlanarFace)> GetCoinicidents(IEnumerable<PlanarFace> faces1, IEnumerable<PlanarFace> faces2, out List<Line> lines1)
        //{
        //    var coincidentFaces = new List<(PlanarFace, PlanarFace)>();
        //    lines1 = new List<Line>();

        //    foreach (var f1 in faces1)
        //    {
        //        var rect1 = Rectangle3dFactoty.Create(f1);
        //        var edges1 = rect1.ToLines();
        //        foreach (var f2 in faces2)
        //        {
        //            var rect2 = Rectangle3dFactoty.Create(f2);
        //            var edges2 = rect2.ToLines();
        //            foreach (var e1 in edges1)
        //            {
        //                if (IsContains(f2, e1.ToXYZ()))
        //                { lines1.Add(e1.ToXYZ()); }
        //                //foreach (var e2 in edges2)
        //                //{
        //                //    if (e1.IsOverlapped(e2))
        //                //    { coincidentFaces.Add((f1, f2)); lines1.Add(e1.ToXYZ()); lines1.Add(e2.ToXYZ()); }
        //                //}
        //            }
        //            foreach (var e2 in edges2)
        //            {
        //                if (IsContains(f1, e2.ToXYZ()))
        //                { lines1.Add(e2.ToXYZ()); }
        //            }

        //        }
        //    }

        //    return coincidentFaces;
        //}
    }
}
