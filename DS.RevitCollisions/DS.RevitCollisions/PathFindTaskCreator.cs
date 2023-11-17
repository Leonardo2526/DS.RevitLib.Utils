using Autodesk.Revit.Creation;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.ResolveTasks;
using DS.GraphUtils.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    internal class PathFindTaskCreator : IEnumerator<IResolveTask>
    {
        private readonly IEnumerator<(IVertex, IVertex)> _vertexPairIterator;
        private Queue<(IVertex, IVertex)> _initialTasksQueue;
        private int _position = -1;

        public PathFindTaskCreator(IEnumerator<(IVertex, IVertex)> vertexPairIterator)
        {
            _vertexPairIterator = vertexPairIterator;
        }

        public IResolveTask Current
        {
            get
            {
                if (_position == -1 || _position >= Tasks.Count)
                    throw new ArgumentException();
                return Tasks[_position];
            }
        }

        object IEnumerator.Current => this.Current;

        public List<IResolveTask> Tasks { get; } = new List<IResolveTask>();

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
                var task = new PathFindVertexTask(currentTask.Item1, currentTask.Item2);
                Tasks.Add(task);
            }
        }

        public void Reset()
        {
            _position = -1;
        }
    }
}
