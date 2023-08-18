using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;
using System;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Solids.Models;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public class ElementCollision : Collision<Element, Element>
    {
        private Solid _intersectionSolid;
        private Solid _intersectionSolidWithInsulation;

        /// <inheritdoc/>
        public ElementCollision(Element object1, Element object2) : base(object1, object2)
        {
        }

        /// <summary>
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// </summary>
        public double MinVolume { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Solid IntersectionSolid
        {
            get
            {
                if (_intersectionSolid == null)
                {
                    _intersectionSolid = Solids.SolidUtils.
                        GetIntersection(ElementUtils.GetSolid(Object1), ElementUtils.GetSolid(Object2), MinVolume);
                }
                return _intersectionSolid;
            }
        }

        /// <summary>
        /// Intersection <see cref="Solid"/> with insulation account.
        /// </summary>
        public Solid IntersectionSolidWithInsulation
        {
            get
            {
                return _intersectionSolidWithInsulation is null ?
                   _intersectionSolidWithInsulation = GetIntersectionSolidWithInsulation() :
                    _intersectionSolidWithInsulation;
            }
        }

        /// <summary>
        /// Get intersection solid with <see cref="Object1"/> and <see cref="Object2"/> insulations.
        /// </summary>
        /// <returns>
        /// Intersection <see cref="Autodesk.Revit.DB.Solid"/> 
        /// with <see cref="Object1"/> and <see cref="Object2"/> insulations if thay have it.
        /// <para>
        /// Otherwise returns only intersection <see cref="Autodesk.Revit.DB.Solid"/>.
        /// </para>
        /// </returns>
        public Solid GetIntersectionSolidWithInsulation()
        {
            var solids = new List<Solid>()
            {IntersectionSolid};

            var rIns = Object1.GetInsulation();
            if (rIns is not null && rIns.IsValidObject)
            { solids.Add(rIns.Solid()); }

            var sIns = Object2.GetInsulation();
            if (sIns is not null && sIns.IsValidObject)
            { solids.Add(sIns.Solid()); }

            return DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(solids);
        }

        /// <summary>
        /// Specifies if current collision is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// <see langword="true"/> if Object1.Id and Object2.Id are equals.
        /// <para>
        /// <see langword="true"/> if <see cref="IntersectionSolid"/>'s are equals.
        /// </para>
        /// <para>
        /// Otherwise returns <see langword="false"/>.       
        /// </para>
        /// </returns>
        public override bool Equals(object obj)
        {
            var collision = obj as ElementCollision;
            if (collision == null) return false;
            bool comparator1()
            {
                return EqualityComparer<ElementId>.Default.Equals(Object1.Id, collision.Object1.Id) &&
                    EqualityComparer<ElementId>.Default.Equals(Object2.Id, collision.Object2.Id);
            }

            bool comparator2()
            {
                return EqualityComparer<ElementId>.Default.Equals(Object1.Id, collision.Object2.Id) &&
                   EqualityComparer<ElementId>.Default.Equals(Object2.Id, collision.Object1.Id);
            }

            bool comparator3()
            {
                XYZ deltaCenter = IntersectionSolid.ComputeCentroid() - collision.IntersectionSolid.ComputeCentroid();
                var deltaVolume = IntersectionSolid.Volume - collision.IntersectionSolid.Volume;
                return deltaCenter.IsZeroLength() && Math.Round(deltaVolume, 5) == 0;
            }

            return comparator1() || comparator2() || comparator3();
        }
    }
}
