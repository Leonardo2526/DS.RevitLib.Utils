using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Various.Selections;
using QuickGraph;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class SegmentFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;
        private GraphVisulisator _graphVisualisator;

        private static Func<IVertex, Point3d> GetLocation(Document doc)
     => (v) => v.GetLocation(doc).ToPoint3d();

        public SegmentFactoryTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
        }


        public AdjacencyGraph<IVertex, Edge<IVertex>> Graph { get; private set; }


        public SegmentFactoryTest BuildGraph()
        {
            Graph = CreateGraphByPoint();

            _graphVisualisator = new GraphVisulisator(_uiDoc)
            {
                ShowElementIds = false,
                ShowVerticesIds = true,
                ShowDirecionts = true
            };

            return this;
        }

        public SegmentFactoryTest GetSegements()
        {
            var factory = new ElementIntersectionFactory(_doc);
            var elementCollisionDetector = new ElementCollisionDetector(_doc, factory);

            var sf = new SegmentFactory(_doc, elementCollisionDetector)
            {
                MinDistanceToElements = 200.MMToFeet(),
                MinDistanceToConnector = 100.MMToFeet(),
                MinDistanceFromSource = 300.MMToFeet()
            };

            var point = GetPoint(out _).ToPoint3d();
            Graph.TryFindEdges(point, GetLocation(_doc), out var edges);
            var edge = edges.FirstOrDefault();

            var segments = sf.GetFreeSegments(edge).ToList();
            var lines = new List<Autodesk.Revit.DB.Line>();
            segments.ForEach(s => lines.Add(s.ToXYZ()));

            _trfIn.CreateAsync(() => lines.ForEach(s => s.Show(_doc)), "ShowEdge");

            var closestToSource = segments.FirstOrDefault().From.ToXYZ();
            _trfIn.CreateAsync(() => closestToSource.Show(_doc), "ShowClosest");
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
