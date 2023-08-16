using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Elements;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// An object to detect collisions (intersections) between <see cref="Solid"/>'s 
    /// and <see cref="Autodesk.Revit.DB.Element"/>'s in Revit model. 
    /// <para>It uses <see cref="Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/> by each <see cref="Solid"/> to detect collisions faster.</para>
    /// </summary>
    public class SolidElementCollisionDetectorFactory : CollisionDetectorFactory<Solid, Element>, ISolidCollisionDetector
    {
        private readonly Document _doc;
        private readonly SolidElementCollisionDetector _modelDetector;
        private readonly List<SolidElementCollisionDetector> _linkDetectors;

        /// <summary>
        /// Instantiate an object to detect collisions in <see cref="Document"/>.
        /// </summary>
        public SolidElementCollisionDetectorFactory(Document doc, 
            List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = null)
        {
            _doc = doc;
            _modelDetector = new SolidElementCollisionDetector(_doc, docElements);
            _linkDetectors = linkElementsDict is null ? null : GetLinkDetectors(linkElementsDict);
        }       

        /// <summary>
        /// Get collisions between <paramref name="checkObject1"/> and objects in 
        /// <see cref="Document"/> and all loaded <see cref="RevitLinkInstance"/>'s.
        /// </summary>
        /// <param name="checkObject1"></param>
        /// <param name="checkObjects2ToExclude"></param>
        /// <returns>Returns collisions list. Returns empty list if no collisions were detected.</returns>
        public override List<ICollision> GetCollisions(Solid checkObject1, List<Element> checkObjects2ToExclude = null)
        {
            Collisions = new List<ICollision>();
            _modelDetector.MinVolume = MinVolume;
            //get colliisons in model
            var modelCollisions = _modelDetector.GetCollisions(checkObject1, checkObjects2ToExclude).Cast<SolidElementCollision>().ToList();
            Collisions.AddRange(modelCollisions);

            //get colliisons in links
            if (_linkDetectors is not null && _linkDetectors.Any()) 
            { 
                foreach (var linkDetector in _linkDetectors)
                {
                    linkDetector.MinVolume = MinVolume;
                    var linkCollisions = linkDetector.GetCollisions(checkObject1, checkObjects2ToExclude).Cast<SolidElementCollision>().ToList();
                    Collisions.AddRange(linkCollisions);
                }            
            }

            return Collisions;
        }

        private List<SolidElementCollisionDetector> GetLinkDetectors(Dictionary<RevitLinkInstance, List<Element>> linkElementsDict)
        {
            var detectors = new List<SolidElementCollisionDetector>();

            //get colliisons in links
            if (linkElementsDict is not null && linkElementsDict.Any())
            {
                foreach (var item in linkElementsDict)
                {
                    var linkDetector = new SolidElementCollisionDetector(item.Key, item.Value);
                    detectors.Add(linkDetector);
                }
            }

            return detectors;
        }
    }
}
