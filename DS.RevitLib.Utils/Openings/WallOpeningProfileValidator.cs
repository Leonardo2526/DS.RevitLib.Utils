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
using System.Linq;

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

            if (!TryGetOpeningRectangle(wall, intersectionItem, out Rectangle3d openingRectangle))
            { return false; }
            _transactionFactory.CreateAsync(() => { openingRectangle.Show(_activeDoc); }, "ShowOpeningEdges");

            (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesCollection) = wall.GetFaces(_activeDoc, _geomOptions, true);
            var wallRectangle = GetWallRectangle(wallFaces, out var wallFace);
            _transactionFactory.CreateAsync(() => { wallRectangle.Show(_activeDoc); }, "ShowWallEdges");

            var insertsRectangles = GetInsertsRectangles(insertsFacesCollection, wall, wallFace, _insertsOffset);
            _transactionFactory.CreateAsync(() => { insertsRectangles.ForEach(r => r.Show(_activeDoc)); }, "ShowInsertsEdges");

            var jointRectangles = GetJointRectangels(wall, wallRectangle, _activeDoc, _jointsOffset, _geomOptions);
            _transactionFactory.CreateAsync(() => { jointRectangles.ForEach(r => r.Show(_activeDoc)); }, "ShowJointsEdges");

            return IsValidOpening(wallRectangle, insertsRectangles, jointRectangles, openingRectangle);
        }

        private IEnumerable<Rectangle3d> GetJointRectangels(Wall wall, Rectangle3d wallRectangle, Document doc, double jointsOffset, Options geomOptions)
        {
            var rectangles = new List<Rectangle3d>();

            var at = 3.DegToRad();
            var mainWallPlane = wallRectangle.Plane;
            var jointsIds = wall.GetJoints();
            var wallDoc = wall.Document;

            foreach (var id in jointsIds)
            {
                if (wallDoc.GetElement(id) is Wall jWall)
                {
                    (List<Face> jwallFaces, Dictionary<ElementId, List<Face>> jInsertsFacesDist) =
                        jWall.GetFaces(doc, geomOptions, false);
                    var pFaces = jwallFaces.OfType<PlanarFace>().OrderByDescending(w => w.Area).ToList();
                    pFaces.RemoveRange(0, 2);
                    var perpFaces = pFaces.Where(f => f.FaceNormal.ToVector3d().IsPerpendicularTo(Vector3d.ZAxis, at)).ToList();

                    var rect1 = Rectangle3dFactoty.Create(perpFaces[0]);
                    var rect2 = Rectangle3dFactoty.Create(perpFaces[1]);
                    var rect = mainWallPlane.DistanceTo(rect1.Center) < mainWallPlane.DistanceTo(rect2.Center) ?
                        rect1 : rect2;

                    var p1 = rect.Corner(0);
                    var p2 = rect.Corner(2);
                    rect = new Rectangle3d(mainWallPlane, p1, p2);
                    if (!rect.TryExtend(jointsOffset, out var exRectangle))
                    { throw new Exception(""); }
                    rectangles.Add(exRectangle);
                }
            }

            return rectangles;
        }

        private Rectangle3d GetWallRectangle(IEnumerable<Face> wallFaces, out PlanarFace wallFace)
        {
            wallFace = wallFaces.OfType<PlanarFace>().OrderByDescending(f => f.Area).First();
            var mainRectangle = Rectangle3dFactoty.Create(wallFace);

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
                    var rectangle = Rectangle3dFactoty.Create(item.Value);
                    rectangles.Add(rectangle);
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
                var message = $"Failed to get profile between wall (id = {wall.Id})";
                if (intersectionItem is MEPCurve mEPCurve)
                { message += $" and MEPCurve (id = {mEPCurve.Id})."; }
                _logger?.Warning(message);
                return false;
            }

            if (!Rectangle3dFactoty.TryCreate(profile, out openingRectangle))
            { _logger?.Error("Failed to create opening."); return false; }

            return true;
        }

        private bool IsValidOpening(
            Rectangle3d wallRectangle,
            IEnumerable<Rectangle3d> insertsRectangles, IEnumerable<Rectangle3d> jointsRectangles,
            Rectangle3d openingRectangle)
        {
            var oConers = new List<Point3d>()
            {
                openingRectangle.Corner(0),
                openingRectangle.Corner(1),
                openingRectangle.Corner(2),
                openingRectangle.Corner(3)
            };

            //check wall containment
            if (oConers.Any(c => wallRectangle.Contains(c) == PointContainment.Outside))
            { _validationResults.Add(new ValidationResult("Failed to check wall containment")); return false; }

            //check inserts containment
            foreach (var insertsRectangle in insertsRectangles)
            {
                if (oConers.Any(c => insertsRectangle.Contains(c) == PointContainment.Inside))
                { _validationResults.Add(new ValidationResult("Failed to check inserts containment")); return false; }
            }

            //check joints containment
            foreach (var jointsRectangle in jointsRectangles)
            {
                if (oConers.Any(c => jointsRectangle.Contains(c) == PointContainment.Inside))
                { _validationResults.Add(new ValidationResult("Failed to check joints containment")); return false; }
            }

            return true;
        }

    }
}
