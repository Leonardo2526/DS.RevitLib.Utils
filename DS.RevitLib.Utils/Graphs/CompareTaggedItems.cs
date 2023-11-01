using DS.ClassLib.VarUtils.Graphs;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{

    public class CompareTaggedVertex : IEqualityComparer<TaggedLVertex<int>>
    {
        public bool Equals(TaggedLVertex<int> x, TaggedLVertex<int> y)
        {
            return x.Tag == y.Tag;
        }

        public int GetHashCode(TaggedLVertex<int> obj)
        {
            return obj.Tag.GetHashCode();
        }
    }

    public class CompareVertexByLocation : IEqualityComparer<LVertex>
    {
        public bool Equals(LVertex x, LVertex y)
        {
            return x.Location.DistanceTo(y.Location) < 0.001;
        }

        public int GetHashCode(LVertex obj)
        {
            return obj.GetHashCode();
        }
    }

    public class CompareTaggedEdge : IEqualityComparer<TaggedEdge<LVertex, int>>
    {
        public bool Equals(TaggedEdge<LVertex, int> x, TaggedEdge<LVertex, int> y)
        {
            var equalVerex = (x.Source.Id == y.Source.Id && x.Target.Id == y.Target.Id)
                || (x.Target.Id == y.Source.Id && x.Source.Id == y.Target.Id);
            return (x.Tag == y.Tag && equalVerex);
        }

        public int GetHashCode(TaggedEdge<LVertex, int> obj)
        {
            return obj.Tag.GetHashCode();
        }
    }
}
