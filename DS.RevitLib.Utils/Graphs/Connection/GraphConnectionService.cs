using DS.GraphUtils.Entities;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.MEP.Models;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils;
using Autodesk.Revit.DB.DirectContext3D;
using DS.ClassLib.VarUtils.Connection;
using IVertexGraph = QuickGraph.IVertexAndEdgeListGraph<DS.GraphUtils.Entities.IVertex,
    QuickGraph.Edge<DS.GraphUtils.Entities.IVertex>>;

namespace DS.RevitLib.Utils.Graphs.Connection
{
    public class GraphConnectionService : ConnectionServiceBase<IVertexGraph>
    {
        private readonly Document _doc;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IConnectionService<MEPSystemModel> _mEPSystemConnectionService;

        public GraphConnectionService(
            Document doc,
            IElementCollisionDetector collisionDetector,
            ITransactionFactory transactionFactory,
            IConnectionService<MEPSystemModel> mEPSystemConnectionService)
        {
            _doc = doc;
            _collisionDetector = collisionDetector;
            _transactionFactory = transactionFactory;
            _mEPSystemConnectionService = mEPSystemConnectionService;
        }

        public override ISpecifyConnection<IVertexGraph> Build(
            IVertexGraph graph1,
            IVertexGraph graph2)
        {
            return new ConnectionSpecificator(
                graph1, graph2,
                _doc, _collisionDetector,
                _transactionFactory, _mEPSystemConnectionService);
        }

        private class ConnectionSpecificator : ConnectionSpecificatorBase
        {
            private IVertexGraph _graph1, _graph2;
            private readonly Document _doc;
            private readonly IElementCollisionDetector _collisionDetector;
            private readonly ITransactionFactory _transactionFactory;
            private readonly IConnectionService<MEPSystemModel> _mEPSystemConnectionService;
            private MEPSystemModel _model1;
            private MEPSystemModel _model2;


            public ConnectionSpecificator(
                IVertexGraph graph1,
            IVertexGraph graph2,
            Document doc,
            IElementCollisionDetector collisionDetector,
            ITransactionFactory transactionFactory,
            IConnectionService<MEPSystemModel> mEPSystemConnectionService) : base(graph1, graph2)
            {
                _graph1 = graph1;
                _graph2 = graph2;
                _doc = doc;
                _collisionDetector = collisionDetector;
                _transactionFactory = transactionFactory;
                _mEPSystemConnectionService = mEPSystemConnectionService;

                var edge1 = graph1.Edges.OfType<TaggedEdge<IVertex, int>>().FirstOrDefault();
                if (edge1 is null) { throw new ArgumentNullException(nameof(edge1)); }
                var e1 = doc.GetElement(new ElementId(edge1.Tag));
                var edge2 = graph2.Edges.OfType<TaggedEdge<IVertex, int>>().FirstOrDefault();
                if (edge2 is null) { throw new ArgumentNullException(nameof(edge2)); }
                var e2 = doc.GetElement(new ElementId(edge2.Tag));

                _model1 = new SimpleMEPSystemBuilder(e1).Build();
                _model2 = new SimpleMEPSystemBuilder(e2).Build();
            }

            public override IVertexGraph TryConnect()
            {
                var connectedModel = _mEPSystemConnectionService.Build(_model2, _model1).TryConnect();
                var e1 = connectedModel.AllElements.FirstOrDefault(e => e.IsValidObject) ?? throw new ArgumentNullException();
                var graph = new MEPGraphBuilder(_doc).Create(e1);

                return graph;
            }

            public override async Task<IVertexGraph> TryConnectAsync()
              => await _transactionFactory.CreateAsync(TryConnect, "ConnectGraphs");

        }
    }
}
