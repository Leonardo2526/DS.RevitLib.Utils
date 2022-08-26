using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions
{
    public class CollisionSearchClient<T,P>
    {
        private readonly CollisionSearch<T,P> _collisionSearch;

        public CollisionSearchClient(CollisionSearch<T,P> collisionSearch)
        {
            _collisionSearch = collisionSearch;
        }

        public List<P> GetCollisions()
        {
            return _collisionSearch.GetCollisions();
        }
    }
}
