using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using MoreLinq;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.VisualStyles;

namespace DS.RevitLib.Utils.Openings
{
    /// <summary>
    /// An object to validate <typeparamref name="TIntersectItem"/> opening profile on the <see cref="Wall"/>.
    /// </summary>
    /// <typeparam name="TIntersectItem"></typeparam>
    public class WallOpeningProfileValidator<TIntersectItem> : IValidator<(Wall, TIntersectItem)>
    {
        private static readonly Options _geomOptions = new()
        {
            ComputeReferences = false, // expensive, avoid if not needed
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = false
        };
        private readonly Document _activeDoc;
        private readonly IOpeningProfileCreator<TIntersectItem> _openingProfileCreator;
        private double _insertsOffset;
        private double _wallOffset;
        private ITransactionFactory _transactionFactory;
        private ILogger _logger;
        private double _jointsOffset;
        private List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Instantiate an object to validate <typeparamref name="TIntersectItem"/> opening profile on the <see cref="Wall"/>.
        /// </summary>
        /// <param name="activeDoc"></param>
        /// <param name="openingProfileCreator"></param>
        public WallOpeningProfileValidator(
            Document activeDoc,
            IOpeningProfileCreator<TIntersectItem> openingProfileCreator)
        {
            _activeDoc = activeDoc;
            _openingProfileCreator = openingProfileCreator;
        }


        #region Properties

        /// <summary>
        /// Wall clerance.
        /// </summary>
        public double WallOffset
        { get => _wallOffset; set => _wallOffset = -value; }

        /// <summary>
        /// Clerance for walls inserts.
        /// </summary>
        public double InsertsOffset
        { get => _insertsOffset; set => _insertsOffset = value; }

        /// <summary>
        /// Clerance for walls joints.
        /// </summary>
        public double JointsOffset
        { get => _jointsOffset; set => _jointsOffset = value; }

        /// <summary>
        /// A factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory
        { get => _transactionFactory; set => _transactionFactory = value; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger
        { get => _logger; set => _logger = value; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;


        #endregion


        /// <inheritdoc/>
        public bool IsValid((Wall, TIntersectItem) value)
        {
            _validationResults.Clear();

            var wall = value.Item1;
            var intersectionItem = value.Item2;
            if (!wall.TryGetBasis(_activeDoc, out var wallBasis)) { throw new Exception(); }

            var wall1Faces = GeometryElementsUtils.GetFaces(wall, _activeDoc);
            var w1PlanarFaces = wall1Faces.OfType<PlanarFace>().ToList();
            var wall1YFaces = w1PlanarFaces.FindAll(FaceFilter.YNormal(wall, _activeDoc));
            var wallFace = wall1YFaces[0];
            var mainPlane = wallFace.GetPlane();
            var mainWallRhinoPlane = mainPlane.ToRhinoPlane();
            var maxLoop = wallFace.GetOuterLoop();
            var mainFaceEdges = maxLoop.Select(x => x).OfType<Autodesk.Revit.DB.Line>();

            if (!TryGetOpeningRectangle(wall, intersectionItem, out Rectangle3d openingRectangle))
            { return true; }
            _transactionFactory.CreateAsync(() => { openingRectangle.Show(_activeDoc); }, "ShowOpeningEdges");

            if (!Rectangle3dFactoty.TryCreate(wallFace, out var wallRectangle))
            { throw new Exception(""); }
            _transactionFactory.CreateAsync(() => { wallRectangle.Show(_activeDoc); }, "ShowWallEdges");

            (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesCollection) = wall.GetFaces(_activeDoc, _geomOptions, true);
            var externalJointRectangels = GetExternalJointRectangels(wall, _activeDoc, wallFace, mainPlane, _wallOffset);
            _transactionFactory.CreateAsync(() => { externalJointRectangels.ForEach(r => r.Show(_activeDoc)); }, "ShowExternalJointEdges");

            var insertsRectangles = GetInsertsRectangles(insertsFacesCollection, wall, wallFace, _insertsOffset);
            _transactionFactory.CreateAsync(() => { insertsRectangles.ForEach(r => r.Show(_activeDoc)); }, "ShowInsertsEdges");

            var jointRectangles = GetJointRectangels(wall, wall1YFaces[0], wall1YFaces[1], _activeDoc, _jointsOffset, _geomOptions);
            _transactionFactory.CreateAsync(() => { jointRectangles.ForEach(r => r.Show(_activeDoc)); }, "ShowJointsEdges");

            return IsValidOpening(wallRectangle, externalJointRectangels, insertsRectangles, jointRectangles, openingRectangle);
        }

        private IEnumerable<Rectangle3d> GetJointRectangels(Wall wall, PlanarFace wallYFace1,
            PlanarFace wallYFace2, Document doc, double jointsOffset, Options geomOptions)
        {
            var rectangles = new List<Rectangle3d>();

            var at = 1.DegToRad();
            var jointsIds = wall.GetJoints();
            var wallJoints = wall.GetJoints(true);
            jointsIds = jointsIds.Where(id => !wallJoints.Contains(id));
            var wallDoc = wall.Document;

            foreach (var id in jointsIds)
            {
                if (wallDoc.GetElement(id) is Wall jWall)
                {
                    var jointFaces = GeometryElementsUtils.GetFaces(jWall, _activeDoc).OfType<PlanarFace>().ToList();
                    if (!jWall.TryGetBasis(_activeDoc, out var bBasis)) { throw new Exception(); }
                    var jointXFaces = jointFaces.Where(f =>
                    f.FaceNormal.ToVector3d().IsParallelTo(bBasis.Y, at) == 0 &&
                     f.FaceNormal.ToVector3d().IsPerpendicularTo(bBasis.Z, at)).ToList();

                    if (!Rectangle3dFactoty.TryCreate(jointXFaces[0], out var rect1))
                    { throw new Exception("Failed to create rectangle"); }
                    if (!Rectangle3dFactoty.TryCreate(jointXFaces[1], out var rect2))
                    { throw new Exception("Failed to create rectangle"); }
                    var e1 = GeometryElementsUtils.ToRevitLines(rect1.ToLines());
                    var rect = e1.Any(e => wallYFace1.Contains(e)) || e1.Any(e => wallYFace2.Contains(e)) ?
                       rect1 : rect2;

                    var p1 = rect.Corner(0);
                    var p2 = rect.Corner(2);
                    rect = new Rectangle3d(wallYFace1.GetPlane().ToRhinoPlane(), p1, p2);
                    if (!wallYFace1.TryProject(rect, true, out var projRect)) { throw new Exception(""); }

                    if (!projRect.TryExtend(jointsOffset, out var exRectangle))
                    { throw new Exception(""); }
                    rectangles.Add(exRectangle);
                }
            }

            return rectangles;
        }


        private IEnumerable<Rectangle3d> GetExternalJointRectangels(Wall wall,
                                                                    Document doc,
                                                                    PlanarFace mainFace,
                                                                    Autodesk.Revit.DB.Plane mainPlane,
                                                                    double wallOffset)
        {
            var rectangles = new List<Rhino.Geometry.Rectangle3d>();

            var (freeEdges, joinElementsEdges) = GeometryElementsUtils.GetSplitedEdges(wall, doc, true);
            var curvesValueResult = joinElementsEdges.Values.SelectMany(v => v).ToList();
            var edgesValueResult = curvesValueResult.OfType<Autodesk.Revit.DB.Line>();

            var origin = wall.GetCenterPoint();
            var mainOrigin = mainPlane.ProjectOnto(origin);

            //get projections
            var projectEdgesValueResult = new List<Autodesk.Revit.DB.Line>();
            foreach (var edge in edgesValueResult)
            {
                var proj = mainFace.Project(edge, true);
                if (proj != null)
                { projectEdgesValueResult.Add(proj); }
            }
            var adjancyRhinoEdges = GeometryElementsUtils.ToRhinoLines(projectEdgesValueResult);
            //_trf.CreateAsync(() => { projectEdgesValueResult.ForEach(r => r.Show(_doc)); }, "ShowInsertsEdges");
            //_uiDoc.RefreshActiveView();

            var maxLoop = mainFace.GetOuterLoop();
            var mainFaceEdges = maxLoop.Select(x => x).OfType<Autodesk.Revit.DB.Line>();
            var mainSubstractEdges = new List<Rhino.Geometry.Line>();

            var mainRhinoLines = GeometryElementsUtils.ToRhinoLines(mainFaceEdges);
            foreach (var rl in mainRhinoLines)
            {
                var subs = LineBooleanTools.Substract(rl, adjancyRhinoEdges.ToList());
                mainSubstractEdges.AddRange(subs);
            }

            var mainRhinoPlane = mainPlane.ToRhinoPlane();
            foreach (var edge in mainSubstractEdges)
            {
                var r = CreateRectangle(edge, mainOrigin.ToPoint3d(), mainRhinoPlane, -wallOffset);
                rectangles.Add(r);
                //break;
            }

            return rectangles;

            Rhino.Geometry.Rectangle3d CreateRectangle(
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
        }


        private Rectangle3d GetWallRectangle(PlanarFace wallFace)
        {
            if (!Rectangle3dFactoty.TryCreate(wallFace, out var mainRectangle))
            { throw new Exception(""); }
            if (_wallOffset == 0) { return mainRectangle; }

            if (!mainRectangle.TryExtend(_wallOffset, out var extendedMainRectangle))
            { throw new Exception(""); }

            return extendedMainRectangle;
        }

        private IEnumerable<Rectangle3d> GetInsertsRectangles(
            Dictionary<ElementId, List<Face>> insertsFacesCollection,
            Wall wall,
            Face wallFace,
            double insertsOffset)
        {
            var insertsFaces = insertsFacesCollection.Values.SelectMany(x => x).ToList();

            var edgesDict = GeometryElementsUtils.GetEdgesOnOpenings(wallFace, wall);
            var insertsRectangles = ToRectangles(edgesDict);

            return GetExtendedRectangles(insertsRectangles, insertsOffset);

            static List<Rectangle3d> ToRectangles(Dictionary<ElementId, List<Autodesk.Revit.DB.Curve>> edgesDict)
            {
                var rectangles = new List<Rectangle3d>();
                foreach (var item in edgesDict)
                {
                    if (Rectangle3dFactoty.TryCreate(item.Value, out var rectangle))
                    { rectangles.Add(rectangle); }
                }
                return rectangles;
            }

            static List<Rectangle3d> GetExtendedRectangles(List<Rectangle3d> rectangles, double offset)
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
        }

        private bool TryGetOpeningRectangle(Wall wall, TIntersectItem intersectionItem, out Rectangle3d openingRectangle)
        {
            openingRectangle = default;
            var profile = _openingProfileCreator.CreateProfile(wall, intersectionItem);
            if (profile is null)
            {
                var message = $"Failed to get opening profile in wall (id = {wall.Id})";
                if (intersectionItem is MEPCurve mEPCurve)
                { message += $" with MEPCurve (id = {mEPCurve.Id})."; }
                _logger?.Warning(message);
                return false;
            }

            if (!Rectangle3dFactoty.TryCreate(profile, out openingRectangle))
            { _logger?.Error("Failed to create opening."); return false; }

            return true;
        }

        private bool IsValidOpening(Rectangle3d wallRectangle,
           IEnumerable<Rectangle3d> externalJointRectangels,
           IEnumerable<Rectangle3d> insertsRectangles, IEnumerable<Rectangle3d> jointsRectangles,
           Rectangle3d openingRectangle)
        {
            //check wall frame containment
            var oConers = openingRectangle.GetCorners();
            if (oConers.Any(c => wallRectangle.Contains(c) == PointContainment.Outside))
            { _validationResults.Add(new ValidationResult("Failed to check wall frame containment")); return false; }


            //check external joints containment
            foreach (var externalJointRectangle in externalJointRectangels)
            {
                if (openingRectangle.Intersection(externalJointRectangle, false, out var intersectionRectangle))
                { _validationResults.Add(new ValidationResult("Failed to check external joints containment")); return false; }
            }

            //check internal joints containment
            foreach (var jointsRectangle in jointsRectangles)
            {
                if (openingRectangle.Intersection(jointsRectangle, false, out var intersectionRectangle))
                { _validationResults.Add(new ValidationResult("Failed to check internal joints containment")); return false; }
            }

            //check inserts containment
            foreach (var insertsRectangle in insertsRectangles)
            {
                if (openingRectangle.Intersection(insertsRectangle, false, out var intersectionRectangle))
                { _validationResults.Add(new ValidationResult("Failed to check inserts containment")); return false; }
            }

            return true;
        }
    }
}
