using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Impl
{
    public class ResolveProcessorBuilder
    {
        private readonly Document _doc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;

        public ResolveProcessorBuilder(Document doc, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph;
        }


        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        /// <summary>
        /// Visualizator to show <typeparamref name="TTask"/>.
        /// </summary>
        public IItemVisualisator<(IVertex, IVertex)> TaskVisualizator { get; set; }

        /// <summary>
        /// Visualizator to show <typeparamref name="TResult"/>.
        /// </summary>
        public IItemVisualisator<PointsList> ResultVisualizator { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }


        public ResolveProcessor<IMEPCollision, PointsList> Processor { get; private set; }


        public ResolveProcessor<IMEPCollision, PointsList> GetProcessor()
        {
            var factories = new List<IResolveFactory<IMEPCollision, PointsList>>();

            //add factories
            var fb1 = new PathFindFactoryBuilder(_doc, _graph)
            {
                IterationCategories = IterationCategories ?? GetIterationCategories(),
                Logger = Logger,
                TaskVisualizator = TaskVisualizator,
                ResultVisualizator = ResultVisualizator,
            };

            IResolveFactory<IMEPCollision, PointsList> f1 = fb1.Create();
           
            factories.Add(f1);

            var p = new ResolveProcessor<IMEPCollision, PointsList>(factories)
            {
                Logger = Logger
            };
            return p;

        }


        private Dictionary<BuiltInCategory, List<PartType>> GetIterationCategories()
        {
            var fittingPartTypes = new List<PartType>()
            {
                PartType.Elbow,
                PartType.Tee,
                PartType.TapPerpendicular,
                PartType.TapAdjustable,
                PartType.SpudPerpendicular,
                PartType.SpudAdjustable
            };
            var verificationCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                { BuiltInCategory.OST_DuctFitting, fittingPartTypes },
                { BuiltInCategory.OST_PipeFitting, fittingPartTypes },
            };

            return verificationCategories;
        }
    }
}
