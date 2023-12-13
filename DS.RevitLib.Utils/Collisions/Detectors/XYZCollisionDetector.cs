using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Solids;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// An object to find collisions (intersections) between <see cref="Autodesk.Revit.DB.XYZ"/> and <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public class XYZCollisionDetector : IXYZCollisionDetector
    {
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly double _tolerance = 0.03;
        private MEPCurve _mEPCurveToCheckCollisions;

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
            Collisions.Clear();

            var collisionSolid = _mEPCurveToCheckCollisions is null ?
                 GetDefault(point) :
                 GetSolid(_mEPCurveToCheckCollisions, point, Transform, _tolerance);

            //var elementsToExclude = new List<Element>();
            //if (_mEPCurveToCheckCollisions != null)
            //{
            //    var connectedElements = ConnectorUtils.GetConnectedElements(_mEPCurveToCheckCollisions, true);
            //    elementsToExclude.Add(_mEPCurveToCheckCollisions);
            //    elementsToExclude.AddRange(connectedElements);
            //}

            //_elementCollisionDetector.ExcludedElements = elementsToExclude;
            var solidCollisions = _elementCollisionDetector.GetCollisions(collisionSolid);
            solidCollisions.ForEach(c => Collisions.Add((point, c.Item2)));

            return Collisions;
        }

        /// <inheritdoc/>
        public IXYZCollisionDetector SetMEPCurve(MEPCurve mEPCurveToCheckCollisions)
        {
            _mEPCurveToCheckCollisions = mEPCurveToCheckCollisions;
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

        private Solid GetDefault(XYZ point, double radius = 0.01)
        {
            return new SphereCreator(radius, point).CreateSolid();
        }
    }
}
