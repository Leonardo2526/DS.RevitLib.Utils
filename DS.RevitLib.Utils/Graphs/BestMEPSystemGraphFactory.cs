using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Graphs;
using DS.ClassLib.VarUtils.GridMap;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using MoreLinq.Extensions;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    public class BestMEPSystemGraphFactory : MEPSystemGraphFactoryBase<AdjacencyGraph<GVertex, Edge<GVertex>>>
    {
        private readonly IVertexBuilder<GVertex, TaggedGVertex<int>> _vertexBuilder;
        private readonly IEdgeBuilder<GVertex, Edge<GVertex>> _edgeBuilder;

        public BestMEPSystemGraphFactory(Document doc,
            IVertexBuilder<GVertex, TaggedGVertex<int>> vertexBuilder,
            IEdgeBuilder<GVertex, Edge<GVertex>> edgeBuilder) : base(doc)
        {
            _vertexBuilder = vertexBuilder;
            _edgeBuilder = edgeBuilder;
        }

        public UIDocument UIDoc { get; set; }
        public ITransactionFactory TransactionFactory { get; set; }


        public override AdjacencyGraph<GVertex, Edge<GVertex>> Create(Element element)
        {
            _graph = new AdjacencyGraph<GVertex, Edge<GVertex>>();

            var open = new Stack<TaggedGVertex<int>>();
            _vertexBuilder.Instansiate(open, _graph);

            var firstTaggedVertex = _vertexBuilder.TryGetVertex(element) as TaggedGVertex<int>;
            if (firstTaggedVertex == null)
            { return _graph = CreateUntaggedGraph(element as MEPCurve); }

            _graph.AddVertex(firstTaggedVertex);

            open.Push(firstTaggedVertex);
            while (open.Count > 0)
            {
                var v1 = open.Pop();
                if (v1.Tag == 768048)
                { }
                while (true)
                {
                    var v2 = _vertexBuilder.TryGetVertex(v1);
                    if (v2 == null) { break; }
                    //return _graph;
                    if (v2 is TaggedGVertex<int> tagged)
                    {
                        var foundInOpen = open.FirstOrDefault(v => v.Tag == tagged.Tag);
                        if (foundInOpen != null)
                        { v2 = foundInOpen; }
                        else
                        { open.Push(tagged); }
                    };

                    _graph.AddVertex(v2);

                    var edge = _edgeBuilder.GetEdge(v1, v2, _vertexBuilder.EdgeTag);
                    _graph.AddEdge(edge);
                }
            }

            return _graph;
        }

        private AdjacencyGraph<GVertex, Edge<GVertex>> CreateUntaggedGraph(MEPCurve mEPCurve)
        {
            var untagged = new AdjacencyGraph<GVertex, Edge<GVertex>>();
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);

            var v1 = new TaggedGVertex<Point3d>(0, freeCons[0].Origin.ToPoint3d());
            var v2 = new TaggedGVertex<Point3d>(1, freeCons[1].Origin.ToPoint3d());

            untagged.AddVertex(v1);
            untagged.AddVertex(v2);

            var edge = _edgeBuilder.GetEdge(v1, v2, mEPCurve.Id.IntegerValue);
            untagged.AddEdge(edge);

            return untagged;
        }
    }
}
