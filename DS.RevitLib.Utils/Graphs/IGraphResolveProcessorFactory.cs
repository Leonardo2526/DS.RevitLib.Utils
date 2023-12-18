using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    public interface IGraphResolveProcessorFactory<TItem, TResult> : IResolveProcessorFactory<TItem, TResult>
    {
        IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> SourceGraph { get;  }
    }
}
