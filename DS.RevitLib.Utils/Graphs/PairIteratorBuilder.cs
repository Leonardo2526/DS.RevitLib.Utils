﻿using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
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
        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private GraphVertexIterator _iterator;
        private VertexPairIterator _pairIterator;

        /// <summary>
        /// Instansiate an object to iterate through graph.
        /// </summary>
        public PairIteratorBuilder(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Start iteration vertex index.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Only input spuds and tees with specified relation will be iterated.
        /// </summary>
        public Relation InElementRelation { get; set; }

        /// <summary>
        /// Available vertex iteration categories.
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> AvailableCategories { get; set; }


        /// <inheritdoc/>
        public IEnumerator<(IVertex, IVertex)> Create(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _graph = graph;

            CreateIterator(graph)
                .WithCategoryValidator()
                .WithRelationValidator();

            return _pairIterator;
        }



        private PairIteratorBuilder CreateIterator(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            var algorithm = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);
            _iterator = new GraphVertexIterator(algorithm)
            {
                StartIndex = StartIndex
            };

            _pairIterator = new VertexPairIterator(_iterator, graph);

            return this;
        }

        private PairIteratorBuilder WithCategoryValidator()
        {
            if (AvailableCategories != null && AvailableCategories.Count > 0)
            {
                var catValidator = new VertexCategoryValidator(_doc, AvailableCategories);
                _iterator.Validators.Add(catValidator);
            }

            return this;
        }

        private PairIteratorBuilder WithRelationValidator()
        {
            if (InElementRelation != Relation.Default)
            {
                var relationValidator = new VertexRelationValidator(_doc, _graph)
                {
                    InElementRelation = InElementRelation
                };
                _iterator.Validators.Add(relationValidator);
            }

            return this;
        }
    }
}
