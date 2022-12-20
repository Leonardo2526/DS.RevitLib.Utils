using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Collisions
{
    /// <summary>
    /// An object to detect collisions (intersections) between <see cref="Solid"/>'s 
    /// and <see cref="Autodesk.Revit.DB.Element"/>'s in Revit model. 
    /// <para>It uses <see cref="Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/> by each <see cref="Solid"/> to detect collisions faster.</para>
    /// </summary>
    public class SolidElementCollisionDetectorFactory : ICollisionDetector
    {
        private readonly Document _doc;
        private readonly List<Element> _docElements;
        private readonly Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;

        /// <summary>
        /// Instantiate an object to detect collisions in <see cref="Document"/>.
        /// </summary>
        public SolidElementCollisionDetectorFactory(Document doc)
        {
            _doc = doc;
            (_docElements, _linkElementsDict) = new ElementsExtractor(_doc).GetAll();
        }

        /// <inheritdoc/>
        public List<IBestCollision> Collisions { get; } = new List<IBestCollision>();

        /// <summary>
        /// Get collisions between <paramref name="checkObjects1"/> and objects in 
        /// <see cref="Document"/> and all its loaded <see cref="RevitLinkInstance"/>'s.
        /// </summary>
        /// <param name="checkObjects1"></param>
        /// <param name="checkObjects2ToExclude"></param>
        /// <returns>Returns collisions list. Returns empty list if no collisions were detected.</returns>
        public List<SolidElementCollision> GetCollisions(List<Solid> checkObjects1, List<Element> checkObjects2ToExclude = null)
        {
            //get colliisons in model
            var modelDetector = new SolidElementStaticCollisionDetector(_doc, _docElements, checkObjects2ToExclude);
            var modelCollisions = modelDetector.GetCollisions(checkObjects1).Cast<SolidElementCollision>().ToList();
            Collisions.AddRange(modelCollisions);

            //get colliisons in links
            if (_linkElementsDict is not null && _linkElementsDict.Any())
            {
                foreach (var item in _linkElementsDict)
                {
                    var linkDetector = new SolidElementStaticCollisionDetector(item.Key, item.Value, checkObjects2ToExclude);
                    //linkDetector.ShowLinkSolids();
                    //_uiDoc.RefreshActiveView();
                    var linkCollisions = linkDetector.GetCollisions(checkObjects1).Cast<SolidElementCollision>().ToList();
                    Collisions.AddRange(linkCollisions);
                }
            }

            return Collisions.Cast<SolidElementCollision>().ToList();
        }
    }
}
