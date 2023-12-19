using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// An object to iterate through graph.
    /// </summary>
    public class PairIteratorBuilder : IVertexPairIteratorBuilder
    {
        private readonly Document _doc;
        private readonly IVertexValidatorSet _validators;
        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private GraphVertexIterator _iterator;
        private VertexPairIterator _pairIterator;

        /// <summary>
        /// Instansiate an object to iterate through graph.
        /// </summary>
        public PairIteratorBuilder(Document doc, IVertexValidatorSet validators)
        {
            _doc = doc;
            _validators = validators;
        }

        /// <summary>
        /// Start iteration vertex index.
        /// </summary>
        public int StartIndex { get; set; }

        /// <inheritdoc/>
        public IEnumerator<(IVertex, IVertex)> Create(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _graph = graph;

            var algorithm = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);
            _iterator = new GraphVertexIterator(algorithm)
            {
                StartIndex = StartIndex
            };

            _validators.ToList().ForEach(_iterator.Validators.Add);

            return new VertexPairIterator(_iterator, graph);
        }
    }
}
