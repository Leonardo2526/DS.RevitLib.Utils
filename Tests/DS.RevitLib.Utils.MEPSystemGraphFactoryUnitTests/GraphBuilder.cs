using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEPSystemGraphFactoryUnitTests
{
    internal class GraphBuilder
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public GraphBuilder(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }

        public AdjacencyGraph<GVertex, Edge<GVertex>> CreateGraph(ElementId elementId)
        {
            var e1 = _doc.GetElement(elementId);

            var vertexBuilder = new GVertexBuilder(_doc);
            var edgeBuilder = new GEdgeBuilder();
            var factory = new BestMEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                UIDoc = _uiDoc
            };

            return factory.Create(e1);
        }
    }
}
