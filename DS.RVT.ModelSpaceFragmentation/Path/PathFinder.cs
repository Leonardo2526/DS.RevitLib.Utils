using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation;
using FrancoGustavo;
using System.Collections.Generic;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Various;
using System.Windows.Media.Media3D;
using DS.ClassLib.VarUtils.Points;
using DS.ClassLib.VarUtils.Directions;
using FrancoGustavo.Algorithm;
using System.Threading;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Geometry.Points;
using System;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using Transform = Rhino.Geometry.Transform;
using Rhino.Geometry;
using DS.RevitLib.Utils.Models;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        private int _tolerance = 3;

        public List<XYZ> PathCoords { get; set; }

        public List<PointPathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints,
            double minAngleDistance, CollisionDetectorByTrace collisionDetector, IDirectionFactory directionFactory,
            double step, double offset, IPoint3dConverter pointConverter,
            IPointVisualisator<Point3D> pointVisualisator = null)
        {
            var orths = new List<Vector3D>() { new Vector3D(1,0,0), new Vector3D(0,1,0), new Vector3D(0,0,1) };
            var basis = new OrthoNormBasis(orths[0], orths[1] , orths[2]);

            var pointsToTransform = new List<Point3d>();

            var uCS2startPoint = new Point3D(startPoint.X, startPoint.Y, startPoint.Z);
            uCS2startPoint = pointConverter.ConvertToUCS2(uCS2startPoint.Convert()).Convert();

            var uCS2endPoint = new Point3D(endPoint.X, endPoint.Y, endPoint.Z);
            uCS2endPoint = pointConverter.ConvertToUCS2(uCS2endPoint.Convert()).Convert();

            var xYZStartPoint = new XYZ(uCS2startPoint.X, uCS2startPoint.Y, uCS2startPoint.Z);
            var xYZEndPoint = new XYZ(uCS2endPoint.X, uCS2endPoint.Y, uCS2endPoint.Z);
            ElementInfo pointsInfo = new ElementInfo(basis, xYZStartPoint, xYZEndPoint);
            pointsInfo.GetPoints();

            var uCS2minPoint = new Point3D(ElementInfo.MinBoundPoint.X, ElementInfo.MinBoundPoint.Y, ElementInfo.MinBoundPoint.Z).Round(_tolerance);
            var uCS2maxPoint = new Point3D(ElementInfo.MaxBoundPoint.X, ElementInfo.MaxBoundPoint.Y, ElementInfo.MaxBoundPoint.Z).Round(_tolerance);

            List<PointPathFinderNode> path = new List<PointPathFinderNode>();

            //var negateBasis = stepBasis.Negate();
            //var orths = new List<Vector3D>() { stepBasis.X, stepBasis.Y, stepBasis.Z, negateBasis.X, negateBasis.Y, negateBasis.Z };

            //return null;
            var mHEstimate = 20;
            var fractPrec = 5;

            //HeuristicFormula formula = GetFormula(stepBasis);
            //HeuristicFormula formula = HeuristicFormula.DiagonalShortCut;
            HeuristicFormula formula = HeuristicFormula.Manhattan;

            var nodeBuilder = new NodeBuilder(formula, mHEstimate, uCS2startPoint, uCS2endPoint, step, orths, collisionDetector, offset, true, false);
            var mPathFinder = new TestPathFinder(uCS2maxPoint, uCS2minPoint, minAngleDistance, collisionDetector, nodeBuilder,
                pointConverter, fractPrec, pointVisualisator)
            {
                PunishAngles = new List<int>() {   },
                TokenSource = new CancellationTokenSource()
                //TokenSource = new CancellationTokenSource(10000)
            };

            var userDirectionFactory = directionFactory as UserDirectionFactory;
            if (userDirectionFactory == null) { return null; }

            var dirs1 = userDirectionFactory.Plane1_Directions;
            var dirs2 = userDirectionFactory.Plane2_Directions;
            var alldirs = userDirectionFactory.Directions;

            var pathDirs = dirs1;

            var moveVectors = new List<Vector3D>();
            foreach (var dir in pathDirs)
            {
                var (vector, angle) = dir.GetWithMinAngle(orths);
                var length = vector.Length / Math.Cos(angle.DegToRad());
                var v = Vector3D.Multiply(dir, length);
                //var v = new Vector3D(dir.X * projStep.X , dir.Y * projStep.Y, dir.Z * projStep.Z);
                moveVectors.Add(v);
            }

            path = mPathFinder.FindPath(
                   uCS2startPoint,
                    uCS2endPoint, pathDirs);
            if (path != null)
                return path;

            return path;
        }

        private Point3d TransformPoint(Point3d point, List<Transform> transforms)
        {
            foreach (var transform in transforms)
            {
                point.Transform(transform);
            }

            return point;
        }

        private HeuristicFormula GetFormula(OrthoBasis stepBasis)
        {
            HeuristicFormula formula;

            var main = new XYZ(stepBasis.X.X, stepBasis.X.Y, stepBasis.X.Z).RoundVector(_tolerance);
            if (
                 XYZUtils.Collinearity(main, XYZ.BasisX) ||
                 XYZUtils.Collinearity(main, XYZ.BasisY) ||
                 XYZUtils.Collinearity(main, XYZ.BasisZ))
            {
                formula = HeuristicFormula.Manhattan;
            }
            else
            {
                formula = HeuristicFormula.DiagonalShortCut;
            }

            return formula;
        }
    }
}
