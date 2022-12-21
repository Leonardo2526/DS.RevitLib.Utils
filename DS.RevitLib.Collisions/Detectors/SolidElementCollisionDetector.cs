using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public class SolidElementCollisionDetector : CollisionDetector<Solid, Element>
    {
        /// <inheritdoc/>
        public SolidElementCollisionDetector(Document doc, List<Element> checkObjects2) :
            base(doc, checkObjects2)
        { }

        /// <inheritdoc/>
        public SolidElementCollisionDetector(RevitLinkInstance revitLink,
            List<Element> checkLinkObjects) :
            base(revitLink, checkLinkObjects)
        { }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollisions(Solid checkObject1, List<Element> exludedCheckObjects2 = null)
        {
            Solid checkSolid = GetCheckSolid(checkObject1);

            List<Element> elements = new ElementsIntersection(_checkObjects2, _checkObjects2Doc).
                GetIntersectedElements(checkSolid, exludedCheckObjects2);

            var collisions = new List<IBestCollision>();
            elements.ForEach(obj => collisions.Add(new SolidElementCollision(checkSolid, obj)));

            return collisions;
        }

        /// <summary>
        /// Transform solid if it belongs to transformed <see cref="RevitLinkInstance"/>.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Returns the input <paramref name="solid"/> if no link or transform in link is exist.</returns>
        private Solid GetCheckSolid(Solid solid)
        {
            if (_revitLink is not null)
            {
                var linkTransform = _revitLink.GetTotalTransform();
                return linkTransform.AlmostEqual(Transform.Identity) ?
                    solid : SolidUtils.CreateTransformed(solid, linkTransform.Inverse);
            }

            return solid;
        }

    }
}
