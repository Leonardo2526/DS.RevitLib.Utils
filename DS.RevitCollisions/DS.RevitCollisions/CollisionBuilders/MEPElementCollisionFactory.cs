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
    public class MEPElementCollisionFactory : ICollisionFactory<MEPCurve, Element, MEPCollision>
    {
        private readonly List<MEPCollision> _collisions = new();

        public IEnumerable<MEPCollision> Collisions => _collisions;

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
        public MEPCollision CreateCollision(MEPCurve element1, Element element2)
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

        private MEPCollision CreateCollision(MEPCurve element1, Solid elem1Solid,
                   Element element2, Solid elem2Solid, Solid intersectionSolid, 
                   ICollisionVisualizator<Collision> visualizator, int count)
        {
            var model1 = new MEPCurveModel(element1, new SolidModel(elem1Solid));
            var model2 = new ElementModel(element2, new SolidModel(elem2Solid));

            return new MEPCollision(model1, model2, intersectionSolid)
            {
                Visualizator = visualizator,
                Id = count
            };
        }


    }
}
