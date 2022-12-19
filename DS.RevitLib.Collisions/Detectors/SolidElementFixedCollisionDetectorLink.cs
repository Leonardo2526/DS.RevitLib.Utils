using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public class SolidElementFixedCollisionLinkDetector : FixedCollisionLinkDetector<Solid, Element>
    {
        private Dictionary<Element,Solid> _linkSolidsDict = new Dictionary<Element,Solid>();
        private readonly List<ElementId> _excludedElementsIds;

        /// <inheritdoc/>
        public SolidElementFixedCollisionLinkDetector(Document doc, KeyValuePair<RevitLinkInstance, List<Element>> checkLinkPair, 
            List<Element> exludedElements = null) : 
            base(doc, checkLinkPair, exludedElements)
        {
            _excludedElementsIds = _exludedObjects is null || !_exludedObjects.Any() ?
                new List<ElementId>() : _exludedObjects.Select(el => el.Id).ToList();
            _checkLinkObjects.Where(obj => !_excludedElementsIds.Contains(obj.Id)).ToList();
            _checkLinkObjects.ForEach(obj => _linkSolidsDict.Add(obj, GetTransformed(obj)));
        }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollision(Solid checkObject1)
        {
            var collisions = new List<IBestCollision>();
            foreach (KeyValuePair<Element, Solid> pair in _linkSolidsDict)
            {
                var intersectionSolidsResult = BooleanOperationsUtils.
                    ExecuteBooleanOperation(checkObject1, pair.Value, BooleanOperationsType.Intersect);
                if (intersectionSolidsResult.Volume > 0)
                {
                    collisions.Add(new SolidElementCollision(checkObject1, pair.Key));
                }
            }

            return collisions;
        }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollisions(List<Solid> checkObjects1)
        {

            var collisions = new List<IBestCollision>();
            checkObjects1.ForEach(obj => collisions.AddRange(GetCollision(obj)));
            return collisions;
        }

        private Solid GetTransformed(Element element)
        {
            Solid solid = ElementUtils.GetSolid(element);
            return SolidUtils.CreateTransformed(solid, _linkTransform);
        }

        //public void ShowLinkSolids()
        //{

        //    var trb = new TransactionBuilder(_doc);
        //    trb.Build(() =>
        //    {
        //        foreach (KeyValuePair<Element, Solid> item in _linkSolidsDict)
        //        {
        //            item.Value.ShowShape(_doc);
        //        }
        //    }, "show link solids");
        //}
    }
}
