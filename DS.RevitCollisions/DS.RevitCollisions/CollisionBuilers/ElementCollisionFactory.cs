using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Solids.Models;
using Serilog;
using System.Collections.Generic;

namespace DS.RevitCollisions.CollisionBuilers
{
    /// <summary>
    /// The factory to build <see cref="Collision"/>s.
    /// </summary>
    public class ElementCollisionFactory : ICollisionFactory<Element, Element, Collision>
    {
        private readonly List<Collision> _collisions = new();

        public IEnumerable<Collision> Collisions => _collisions;

        /// <summary>
        /// Object to show collision.
        /// </summary>
        public ICollisionVisualizator<Collision> Visualizator { get; set; }

        /// <summary>
        /// Specifies minimum intersection solid volume.
        /// </summary>
        public double MinIntersectionVolume { get; set; }

        public bool ExcludeTraversableArchitecture { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }


        /// <inheritdoc/>
        public Collision CreateCollision(Element element1, Element element2)
        {

            //exclude colliisons
            if (ExcludeTraversableArchitecture && element1 is MEPCurve mEPCurve)
            {
                if (element2 is Wall wall && wall.IsTraversable(mEPCurve.Direction()))
                { return null; }
                if (element2 is Floor floor && mEPCurve.IsFloorTraversable())
                { return null; }
            }

            if (element1.IsConnected(element2)) { return null; }

            var intersectionSolid = CollisionUtils.GetIntersectionSolid(element1, element2,
                out Solid elem1Solid, out Solid elem2Solid);
            if (intersectionSolid == null ||
                intersectionSolid.Volume < MinIntersectionVolume)
            { return null; }

            var collision = CreateCollision(
                element1, elem1Solid,
                element2, elem2Solid,
                intersectionSolid, 
                Visualizator, _collisions.Count);

            if (collision != null)
            {
                _collisions.Add(collision);
                Logger?.Information($"Collision #{_collisions.Count} (id1: {collision.Item1.Id}, id2: {collision.Item2.Id}) " +
                    $"of type {collision.GetType().Name} was created.");
            }

            return collision;
        }

        private Collision CreateCollision(Element element1, Solid elem1Solid,
                   Element element2, Solid elem2Solid, Solid intersectionSolid, 
                   ICollisionVisualizator<Collision> visualizator, int count)
        {
            var model1 = GetModel(element1, elem1Solid);
            var model2 = GetModel(element2, elem2Solid);

            return GetCollision(model1, model2, intersectionSolid);

            AbstractElementModel GetModel(Element element, Solid solid)
            {
                AbstractElementModel model = null;

                switch (element)
                {
                    case MEPCurve mc:
                        {
                            model = new MEPCurveModel(mc, new SolidModel(solid));
                            break;
                        }
                    case Element:
                        {
                            model = new ElementModel(element1, new SolidModel(solid));
                            break;
                        }
                    default:
                        break;
                }

                return model;
            }

            Collision GetCollision(AbstractElementModel model1, AbstractElementModel model2, Solid intersectionSolid)
            {
                Collision collision = null;

                switch (model1)
                {
                    case MEPCurveModel mModel1:
                        {
                            collision = new MEPCollision(mModel1, model2, intersectionSolid)
                            {
                                Visualizator = visualizator,
                                Id = count
                            };
                            break;
                        }
                    case ElementModel eModel1:
                        {
                            collision = new ElementCollision(eModel1, model2, intersectionSolid)
                            {
                                Visualizator = visualizator,
                                Id = count
                            };
                            break;
                        }
                }

                return collision;
            }
        }


    }
}
