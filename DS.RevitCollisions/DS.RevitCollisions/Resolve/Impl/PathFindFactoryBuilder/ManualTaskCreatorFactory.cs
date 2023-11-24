using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Graphs;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using QuickGraph;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph.Algorithms;
using Serilog;

namespace DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder
{
    internal class ManualTaskCreatorFactory : ITaskCreatorFactory<IMEPCollision, (IVertex, IVertex)>
    {
        private readonly UIDocument _uIDoc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly XYZCollisionDetector _xYZCollisionDetector;
        private readonly Document _doc;
        private readonly IBidirectionalGraph<IVertex, Edge<IVertex>> _bdGraph;

        public ManualTaskCreatorFactory(UIDocument uIDoc,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            IElementCollisionDetector elementCollisionDetector)
        {
            _uIDoc = uIDoc;
            _graph = graph;
            _elementCollisionDetector = elementCollisionDetector;
            _xYZCollisionDetector = new XYZCollisionDetector(elementCollisionDetector);
            _doc = _uIDoc.Document;
            _bdGraph = graph.ToBidirectionalGraph();
        }

        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline ExternalOutline { get; set; }

        public Dictionary<BuiltInCategory, List<PartType>> AvailableCategories { get; set; }

        /// <summary>
        /// Maximum length from <see cref="IVertex"/> to graph root vertex.
        /// </summary>
        public double MaxLength { get; set; }

        /// <summary>
        /// Maximum vertices count from <see cref="IVertex"/> to graph root vertex.
        /// </summary>
        public int MaxVerticesCount { get; set; }     

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        public ITraceSettings TraceSettings { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }


        public ITaskCreator<IMEPCollision, (IVertex, IVertex)> Create()
        {
            var validators = new List<IValidator<IVertex>>();

            _xYZCollisionDetector.ElementClearance = TraceSettings.B;
            var collisionValidator = new VertexCollisionValidator(_doc, _bdGraph, _elementCollisionDetector, _xYZCollisionDetector);
            var limitsValidator = new VertexLimitsValidator(_doc)
            {
                BoundOutline = ExternalOutline,
                MaxVerticesCount = MaxVerticesCount,
                IsInsulationAccount = InsulationAccount,
                MaxLength = MaxLength,
                MinDistToFloor = TraceSettings.H,
                MinDistToCeiling = TraceSettings.B,
            };
            limitsValidator.Instansiate(_graph);
            limitsValidator.SetParent(_graph.Roots().First());

            var famInstCategoryValidator = new VertexFamInstCategoryValidator(_doc, AvailableCategories);
            var relationValidator = new VertexRelationValidator(_doc, _bdGraph)
            { InElementRelation = Relation.Child };
            var graphContainsValidator = new GraphContainsVertexValidator(_doc, _graph);

            validators.Add(collisionValidator);
            validators.Add(limitsValidator);
            validators.Add(famInstCategoryValidator); 
            validators.Add(relationValidator);
            validators.Add(graphContainsValidator);


            var pointer = new PairVertexPointer(_uIDoc)
            {
                Validators = validators,
                Messenger = Messenger,
                Logger = Logger,

            };
            return new ManualTaskCreator(pointer, _graph, _doc);
        }
    }
}
