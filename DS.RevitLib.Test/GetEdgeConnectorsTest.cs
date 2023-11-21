using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Various.Selections;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class GetEdgeConnectorsTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;

        public AdjacencyGraph<IVertex, Edge<IVertex>> Graph { get; private set; }

        public GetEdgeConnectorsTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
        }

        public GetEdgeConnectorsTest BuildGraph()
        {
            Graph = CreateGraphByPoint();
            return this;
        }

        public GetEdgeConnectorsTest GetConnectorsLocation()
        {
            var point = GetPoint(out _).ToPoint3d();
            var edge = Graph.GetEdge(point, _doc);
            var (point1, point2) = edge.GetConnectorsLocation(_doc);

            var xYZ1 = point1.ToXYZ();
            var xYZ2 = point2.ToXYZ();

            xYZ1.Show(_doc, 0, _trfIn);
            xYZ2.Show(_doc, 0, _trfIn);
            return this;
        }

        public GetEdgeConnectorsTest GetConnectionSegment()
        {
            var point = GetPoint(out _).ToPoint3d();
            var edge = Graph.GetEdge(point, _doc);

            double offsetFromSourceCon = 100.MMToFeet();
            double offsetFromTargetCon = 300.MMToFeet();
            var segment = edge.GetConnectionSegment(_doc, offsetFromSourceCon, offsetFromTargetCon);

            var xYZ1 = segment.From.ToXYZ();
            var xYZ2 = segment.To.ToXYZ();

            xYZ1.Show(_doc, 0, _trfIn);
            xYZ2.Show(_doc, 0, _trfIn);
            return this;
        }

        private AdjacencyGraph<IVertex, Edge<IVertex>> CreateGraphByPoint()
        {
            XYZ point = GetPoint(out MEPCurve mEPCurve);

            var vertexBuilder = new GVertexBuilder(_doc);
            var edgeBuilder = new GEdgeBuilder();

            var facrory = new MEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                TransactionFactory = _trfIn,
                UIDoc = _uiDoc
            };

            return facrory.Create(mEPCurve, point);
        }

        private XYZ GetPoint(out MEPCurve mEPCurve)
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = true };
            mEPCurve = selector.Pick() as MEPCurve;
            return selector.Point;
        }
    }
}
