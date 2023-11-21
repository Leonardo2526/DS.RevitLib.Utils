using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    internal class PathFindVertexPairResolver : ITaskResolver<(IVertex, IVertex), PointsList>
    {
        public IEnumerable<PointsList> Results => throw new NotImplementedException();

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }


        /// <summary>
        /// Propagates notification that operations should be canceled.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        public PointsList TryResolve((IVertex, IVertex) task)
        {
            var resultPoints = new PointsList()
            {
                new Point3d(1,0,0)
            };

            return resultPoints;
        }

        public Task<PointsList> TryResolveAsync((IVertex, IVertex) task)
        {
            throw new NotImplementedException();
        }
    }
}
