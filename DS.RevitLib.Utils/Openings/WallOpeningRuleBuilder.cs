using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.ClassLib.VarUtils;
using Rhino.UI;
using DS.RevitLib.Utils.Creation.Transactions;
using System.Security.Cryptography;
using System.Drawing;
using Serilog;
using MoreLinq;

namespace DS.RevitLib.Utils.Openings
{
    public class WallOpeningRuleBuilder 
    //public class WallOpeningRuleBuilder : IWallOpeningRuleBuilder
    {
        private static readonly Options _geomOptions = new()
        {
            ComputeReferences = true, // expensive, avoid if not needed
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = false
        };
        private readonly Document _activeDoc;
        private readonly MEPCurve _mEPCurve;
        private readonly IOpeningProfileCreator _openingProfileCreator;
        private double _insertsOffset;
        private double _wallOffset;
        private ITransactionFactory _transactionFactory;
        private ILogger _logger;

        public WallOpeningRuleBuilder(
            Document activeDoc, 
            MEPCurve mEPCurve, 
            IOpeningProfileCreator openingProfileCreator)
        {
            _activeDoc = activeDoc;
            _mEPCurve = mEPCurve;
            _openingProfileCreator = openingProfileCreator;
        }


        public double WallOffset
        { get => _wallOffset; set => _wallOffset = -value; }

        public double InsertsOffset
        { get => _insertsOffset; set => _insertsOffset = value; }

        public ITransactionFactory TransactionFactory
        { get => _transactionFactory; set => _transactionFactory = value; }

        public ILogger Logger
        { get => _logger; set => _logger = value; }


        public bool GetRule(Wall wall)
        {
            if(!TryGetOpeningRectangle(wall, _mEPCurve, out Rectangle3d openingRectangle)) 
            { return false; }

            (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesCollection) = wall.GetFaces(_activeDoc, _geomOptions, true);
            var wallRectangle = GetWallRectangle(wallFaces, _activeDoc, _geomOptions, out var wallFace);
            _transactionFactory.CreateAsync(() => { wallRectangle.Show(_activeDoc); }, "ShowWallEdges");

            var insertsRectangles = GetInsertsRectangles(insertsFacesCollection , wall, wallFace, _insertsOffset);
            _transactionFactory.CreateAsync(() => { insertsRectangles.ForEach(r => r.Show(_activeDoc)); }, "ShowInsertsEdges");

            _transactionFactory.CreateAsync(() => { openingRectangle.Show(_activeDoc); }, "ShowOpeningEdges");

            return  IsValidOpening(wallRectangle, insertsRectangles, openingRectangle);
        }

        public Func<(Solid, Element), bool> GetRuleFunc()
        {
            bool func((Solid, Element) f)
            {
                if (f.Item2 is not Wall wall) { return false; }
                return GetRule(wall);
            }
            return func;
        }


        private Rectangle3d GetWallRectangle(IEnumerable<Face> wallFaces, Document doc, Options geomOptions, out PlanarFace wallFace)
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

        private bool TryGetOpeningRectangle(Wall wall, MEPCurve mEPCurve, out Rectangle3d openingRectangle)
        {
            openingRectangle = default;
            var profile = _openingProfileCreator.CreateProfile(wall, mEPCurve);
            if(profile is null) 
            { 
                _logger?.Warning($"Failed to get profile between wall (id = {wall.Id}) and MEPCurve (id = {mEPCurve.Id}).");
                return false;
            }

            if (!Rectangle3dFactoty.TryCreate(profile, out openingRectangle))
            { _logger?.Error("Failed to create opening."); return false; }

            return true;
        }

        private bool IsValidOpening(
            Rectangle3d wallRectangle, 
            IEnumerable<Rectangle3d> insertsRectangles, 
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
            if(oConers.Any(c => wallRectangle.Contains(c) == PointContainment.Outside)) 
            { return false; }

            //check inserts containment
            foreach (var insertsRectangle in insertsRectangles)
            {
                if (oConers.Any(c => insertsRectangle.Contains(c) == PointContainment.Inside))
                { return false; }
            }

            return true;
        }


    }
}
