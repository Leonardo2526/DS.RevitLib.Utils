using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Collisons;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.PathCreators.AlgorithmBuilder;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators.AlgorithmVertexBuilder
{
    public class XYZVertexPathFinderFactory
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public XYZVertexPathFinderFactory(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }


        public IEnumerable<IBasisStrategy> BasisStrategies { get; set; } =
            new List<IBasisStrategy>()
            {
                new OneMEPCurvesBasisStrategy(),
                new TwoMEPCurvesBasisStrategy()
            };

        public IDirectionValidator DirectionValidator { get; set; } = new CCSModel();

        public ITraceSettings TraceSettings { get; set; } = new TraceSettings();

        public IAlgorithmBuilder AlgorithmBuilder { get; set; }

        public IElementsExtractor ElementsExtractor { get; set; }      

        public XYZVertexPathFinder PathFinder { get; private set; }

        /// <summary>
        /// Token to cancel finding path operation.
        /// </summary>
        public CancellationTokenSource ExternalToken { get; set; } = new CancellationTokenSource();

        public XYZVertexPathFinder GetInstance()
        {
            AlgorithmBuilder ??=
                new PathAlgorithmVertexBuilder(_uiDoc, TraceSettings, BasisStrategies, DirectionValidator);
            ElementsExtractor ??= new GeometryElementsExtractor(_doc);           

            PathFinder = new XYZVertexPathFinder(_uiDoc, TraceSettings, AlgorithmBuilder)
            {
                ElementsExtractor = ElementsExtractor,
                ExternalToken = ExternalToken
                 
            };

            return PathFinder;
        }

    }

}
