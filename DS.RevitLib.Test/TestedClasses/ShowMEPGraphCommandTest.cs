using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.GraphUtils.Entities;
using DS.GraphUtils.Entities.Command;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Graphs.Commands;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class ShowMEPGraphCommandTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _transactionFactory;
        private readonly ContextTransactionFactory _transactionFactoryOut;

        public ShowMEPGraphCommandTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _transactionFactory = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _transactionFactoryOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
        }

        public IEnumerable<Point3d> Points0 = new List<Point3d>()
        {
            new Point3d(),
            new Point3d(1, 0, 0),
            new Point3d(1, 1, 0),
            new Point3d(2, 1, 0)
        };


        public  void Run1()
        {
            var graph = CreateGraph();
            var mEPGraph = ShowGraph(graph);
            mEPGraph.PrintEdgesVerticesTags();
            //return;
            _uiDoc.RefreshActiveView();
            mEPGraph.DeleteFromDoc(_doc, _transactionFactory).Wait();
            _uiDoc.RefreshActiveView();
        }

        public void Run2()
        {
            var graph = CreateGraph();
            var lineGraph = ShowLineGraph(graph);
            lineGraph.PrintEdgesVerticesTags();
        }

        public void Run3()
        {
            var graph = CreateGraph();
            var lineGraph = ShowLineGraph(graph);
            lineGraph.PrintEdgesVerticesTags();
            _uiDoc.RefreshActiveView();
            lineGraph.DeleteFromDoc(_doc, _transactionFactory).Wait();
            _uiDoc.RefreshActiveView();
            return;
        }

        public  IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> CreateGraph()
        {
            var factory = new SimpleChainGraphFactory(Points0);
            return factory.CreateGraph();
        }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> ShowGraph(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            MEPCurve baseMEPCurve;
            try
            {
                baseMEPCurve = new MEPCurveSelector(_uiDoc) { AllowLink = false }.
                    Pick("Выберите базовый линейный элемент внутри удаляемого диапазона.");
            }
            catch (Exception)
            { return null; }
          
            var command = new ShowMEPGraphCommand(graph, _uiDoc, baseMEPCurve)
            {
                TransactionFactory = _transactionFactory,
                ShowUntaggedVertices = false
            };
            //var result = command.ShowVerticesAsync().Result;
            //var result = command.ShowEdgeAsync().Result;
            var result = command.ShowGraphAsync().Result;

            return result;
        }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> ShowLineGraph(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {           
            var command = new ShowLineGraphCommand(graph, _uiDoc)
            {
                TransactionFactory = _transactionFactory, 
                ShowDirecionts = true, 
                ShowEdgeTags = true, 
                ShowVertexIds = false, 
                ShowVertexTags = true
            };
            //var result = command.ShowVerticesAsync().Result;
            //var result = command.ShowEdgeAsync().Result;
            var result = command.ShowGraphAsync().Result;

            return result;
        }
    }
}
