using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using QuickGraph;
using QuickGraph.Algorithms;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    internal class VertexPairTaskCreator : ITaskCreator<IMEPCollision, (IVertex, IVertex)>, IEnumerator<(IVertex, IVertex)>
    {
        private readonly Document _doc;
        private readonly IEnumerator<(IVertex, IVertex)> _vertexPairIterator;
        private readonly EdgePointIterator _edgePointIterator;
        private readonly MEPCurve _baseMEPCurve;
        private readonly List<Edge<IVertex>> _edges;
        private readonly AdjacencyGraph<IVertex, Edge<IVertex>> _targetGraph;
        private int _position = -1;
        private Queue<(IVertex, IVertex)> _priorityQueue;

        public VertexPairTaskCreator(
            Document doc,
            IEnumerator<(IVertex, IVertex)> vertexPairIterator,
            EdgePointIterator edgePointIterator,
            AdjacencyGraph<IVertex, Edge<IVertex>> targetGraph, MEPCurve baseMEPCurve)
        {
            _doc = doc;
            _vertexPairIterator = vertexPairIterator;
            _edgePointIterator = edgePointIterator;

            var root = targetGraph.Roots().FirstOrDefault();
            var outEdges = targetGraph.OutEdges(root).ToList();
            if (outEdges is null || outEdges.Count() != 2)
            { throw new InvalidOperationException(); }

            _edges = outEdges;
            _targetGraph = targetGraph;
            _baseMEPCurve = baseMEPCurve;
        }

        public (IVertex, IVertex) Current
        {
            get
            {
                if (_position == -1 || _position >= Tasks.Count)
                    throw new ArgumentException();
                return Tasks[_position];
            }
        }

        object IEnumerator.Current => Current;

        public List<(IVertex, IVertex)> Tasks { get; } = new List<(IVertex, IVertex)>();


        public ILogger Logger { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            while (_priorityQueue?.Count > 0)
            {
                AddTask(_priorityQueue.Dequeue());
                return true;
            }

            if (_vertexPairIterator.MoveNext())
            {
                AddTask(_vertexPairIterator.Current);
                return true;
            }

            return false;

            void AddTask((IVertex, IVertex) currentTask)
            {
                _position++;
                var task = (currentTask.Item1, currentTask.Item2);
                Tasks.Add(task);
            }
        }

        public void Reset()
        {
            _position = -1;
            _priorityQueue = GetPriorityQueue();
        }


        public (IVertex, IVertex) CreateTask(IMEPCollision item)
        {
            _priorityQueue ??= GetPriorityQueue();
            return MoveNext() ? Current : default;
        }


        private PriorityTaskQueue GetPriorityQueue()
        {
            _edgePointIterator.Set(_edges[0]);
            _edgePointIterator.MoveNext();
            var p1 = _edgePointIterator.Current.ToXYZ();

            _edgePointIterator.Set(_edges[1]);
            _edgePointIterator.MoveNext();
            var p2 = _edgePointIterator.Current.ToXYZ();

            if (p1 != null && _targetGraph.TryInsert(_baseMEPCurve, p1))
            { Logger?.Information($"Closest point {p1} was inserted to graph successfully."); }
            if (p2 != null && _targetGraph.TryInsert(_baseMEPCurve, p2))
            { Logger?.Information($"Closest point {p2} was inserted to graph successfully."); }

            //_targetGraph.PrintEdges();

            _vertexPairIterator.Reset();

            return new PriorityTaskQueue(_doc, _vertexPairIterator, _targetGraph, _baseMEPCurve);
        }
    }
}
