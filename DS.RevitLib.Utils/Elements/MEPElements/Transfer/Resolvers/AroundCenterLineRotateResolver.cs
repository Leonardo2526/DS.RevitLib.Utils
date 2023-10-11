using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;

using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.Resolvers
{
    internal class AroundCenterLineRotateResolver : CollisionResolver
    {
        private int _successorUsageCount = 0;

        public AroundCenterLineRotateResolver(SolidModelExt operationElement, (Solid, Element) collision,
            IElementCollisionDetector detector, List<Element> excludedElements) :
            base(operationElement, collision, detector, excludedElements)
        {
        }

        private bool IsAlwaysVertical
        {
            get
            {
                var famIns = _operationElement.Element as FamilyInstance;
                if (famIns != null)
                {
                    var fam = famIns.Symbol.Family;
                    var param = fam?.get_Parameter(BuiltInParameter.FAMILY_ALWAYS_VERTICAL);
                    var s = param?.AsValueString();
                    return s == "Yes";
                }
                else { return false; }
            }
        }

        public override void Resolve()
        {
            XYZ axis = _operationElement.CentralLine.Direction;
            ResolvePosition(axis, _operationElement.CentralPoint);

            if (IsResolved)
            {
                return;
            }
            else
            {
                if (_successorUsageCount > 0)
                {
                    return;
                }
                _successorUsageCount++;
                _successor.Resolve();
                if (_successor.UnresolvedCollisions.Any())
                {
                    UnresolvedCollisions = _successor.UnresolvedCollisions;
                }
            }
        }

        public void ResolvePosition(XYZ axis, XYZ point)
        {
            //if(IsAlwaysVertical) { return; }

            for (int i = 0; i < 3; i++)
            {
                double angle = +90.DegToRad();
                Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, point);
                if (!AlwaysVerticalCheck(rotateTransform)) { continue; }

                _operationElement.Transform(rotateTransform);

                _detector.ExludedElements = _excludedElements;
                UnresolvedCollisions = _detector.GetCollisions(_operationElement.Solid);

                if (!UnresolvedCollisions.Any())
                {
                    IsResolved = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Check if rotation is satisfy to always vertical condition.
        /// </summary>
        /// <param name="rotateTransform"></param>
        /// <returns>
        /// <see langword="true"/> if isAlwaysVertical parameter is <see langword="false"/> 
        /// or <paramref name="rotateTransform"/> is satisfies to always vertical condition.
        /// <para>
        /// Otherwise returns <see langword="false"/>.
        /// </para>
        /// </returns>
        private bool AlwaysVerticalCheck(Transform rotateTransform)
        {
            if (!IsAlwaysVertical) { return true; }

            Basis basis = _operationElement.Basis.Clone();
            basis.Transform(rotateTransform);
            return basis.Y.IsAlmostEqualTo(XYZ.BasisZ);
        }
    }
}
