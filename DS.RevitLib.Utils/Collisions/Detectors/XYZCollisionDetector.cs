using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Solids;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// An object to find collisions (intersections) between <see cref="Autodesk.Revit.DB.XYZ"/> and <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public class XYZCollisionDetector : IXYZCollisionDetector
    {
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly double _tolerance = 0.03;
        private MEPCurve _mEPCurveOnPoint;

        /// <summary>
        /// Instantiate a new object to find collisions (intersections) between 
        /// <see cref="Autodesk.Revit.DB.XYZ"/> and <see cref="Autodesk.Revit.DB.Element"/>'s.
        /// </summary>
        /// <param name="elementCollisionDetector"></param>
        public XYZCollisionDetector(IElementCollisionDetector elementCollisionDetector)
        {
            _elementCollisionDetector = elementCollisionDetector;
        }

        /// <inheritdoc/>
        public List<(XYZ, Element)> Collisions { get; } = new List<(XYZ, Element)>();


        /// <summary>
        /// Clearance between elements or its isolations.
        /// </summary>
        public double ElementClearance { get; set; }

        /// <summary>
        /// Transform for cheked point's <see cref="Solid"/>.
        /// </summary>
        public Transform Transform { get; set; }

        /// <inheritdoc/>
        public List<(XYZ, Element)> GetCollisions(XYZ point)
        {
            var collisionSolid = _mEPCurveOnPoint is null ?
                 GetDefault(point) :
                 GetSolid(_mEPCurveOnPoint, point, Transform, _tolerance);

            var elementsToExclude = new List<Element>();
            if (_mEPCurveOnPoint != null)
            {
                var connectedElements = ConnectorUtils.GetConnectedElements(_mEPCurveOnPoint, true);
                elementsToExclude.Add(_mEPCurveOnPoint);
                elementsToExclude.AddRange(connectedElements);
            }

            _elementCollisionDetector.ExcludedElements = elementsToExclude;
            var solidCollisions = _elementCollisionDetector.GetCollisions(collisionSolid);
            solidCollisions.ForEach(c => Collisions.Add((point, c.Item2)));

            return Collisions;
        }

        /// <inheritdoc/>
        public IXYZCollisionDetector SetMEPCurve(MEPCurve mEPCurve)
        {
            _mEPCurveOnPoint = mEPCurve;
            return this;    
        }

        private Solid GetSolid(MEPCurve mEPCurve, XYZ point, Transform transform, double tolerance)
        {
            var collisionSolid = new MEPCurveSolidCreator(mEPCurve, null, ElementClearance - tolerance)
            {
                IsInsulationAccount = _elementCollisionDetector.IsInsulationAccount
            }
            .CreateSolid(point);

            return transform == null ?
                collisionSolid :
                Autodesk.Revit.DB.SolidUtils.CreateTransformed(collisionSolid, transform);
        }

        private Solid GetDefault(XYZ point, double radius = 0.001)
        {
            return new SphereCreator(radius, point).CreateSolid();
        }
    }
}
