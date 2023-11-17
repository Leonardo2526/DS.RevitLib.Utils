using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    internal class FirstPathFindCreator : IEnumerator<IResolveTask>
    {
        private readonly IEnumerator<(IVertex, IVertex)> _vertexPairIterator;
        private List<IResolveTask> _tasks = new List<IResolveTask>();
        private int _position = -1;

        public FirstPathFindCreator(IEnumerator<(IVertex, IVertex)> vertexPairIterator)
        {
            _vertexPairIterator = vertexPairIterator;
        }

        public IResolveTask Current
        {
            get
            {
                if (_position == -1 || _position >= _tasks.Count)
                    throw new ArgumentException();
                return _tasks[_position];
            }
        }

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
