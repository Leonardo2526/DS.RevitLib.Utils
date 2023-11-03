using DS.ClassLib.VarUtils.Graphs;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{

    public class CompareGTaggedVertex : IEqualityComparer<TaggedGVertex<int>>
    {
        public bool Equals(TaggedGVertex<int> x, TaggedGVertex<int> y)
        {
            return x.Tag == y.Tag;
        }

        public int GetHashCode(TaggedGVertex<int> obj)
        {
            return obj.Tag.GetHashCode();
        }
    }


    public class CompareGGVertexByLocation : IEqualityComparer<GVertex>
    {
        public bool Equals(GVertex x, GVertex y)
        {
            var pointVertex1 = x as TaggedGVertex<Point3d>;
            var pointVertex2 = y as TaggedGVertex<Point3d>;
            if(pointVertex1 == null || pointVertex2 == null) { return true; }

            return pointVertex1.Tag.DistanceTo(pointVertex2.Tag) < 0.001;
        }

        public int GetHashCode(GVertex obj)
        {
            return obj.GetHashCode();
        }
    }

    public class CompareGTaggedEdge : IEqualityComparer<TaggedEdge<GVertex, int>>
    {
        public bool Equals(TaggedEdge<GVertex, int> x, TaggedEdge<GVertex, int> y)
        {
            var equalVerex = (x.Source.Id == y.Source.Id && x.Target.Id == y.Target.Id)
                || (x.Target.Id == y.Source.Id && x.Source.Id == y.Target.Id);
            return (x.Tag == y.Tag && equalVerex);
        }

        public int GetHashCode(TaggedEdge<GVertex, int> obj)
        {
            return obj.Tag.GetHashCode();
        }
    }
}
