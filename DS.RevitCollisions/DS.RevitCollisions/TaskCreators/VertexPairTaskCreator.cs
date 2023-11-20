using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCollisions
{
    internal class VertexPairTaskCreator : IMEPCollisionTaskCreator<(IVertex, IVertex)>, IEnumerator<(IVertex, IVertex)>
    {
        private readonly IEnumerator<(IVertex, IVertex)> _vertexPairIterator;
        private Queue<(IVertex, IVertex)> _initialTasksQueue;
        private int _position = -1;

        public VertexPairTaskCreator(IEnumerator<(IVertex, IVertex)> vertexPairIterator)
        {
            _vertexPairIterator = vertexPairIterator;
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

        public IEnumerable<(IVertex, IVertex)> InitialTasks
        {
            get { return _initialTasksQueue; }
            set
            {
                _initialTasksQueue = new Queue<(IVertex, IVertex)>();
                value.ToList().ForEach(_initialTasksQueue.Enqueue);
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            while (_initialTasksQueue?.Count > 0)
            {
                AddTask(_initialTasksQueue.Dequeue());
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
        }

        public (IVertex, IVertex) CreateTask(IMEPCollision item)
        {
            MoveNext();
            return Current;
        }
    }
}
