using DS.GraphUtils.Entities;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    public class EdgePointIterator : IEnumerator<Point3d>
    {
        private readonly SegmentFactory _segmentFactory;
        private Queue<Line> _segmentsQueue = new Queue<Line>();
        private int _position = -1;

        public EdgePointIterator(SegmentFactory segmentFactory)
        {
            _segmentFactory = segmentFactory;
        }

        public List<Point3d> Points { get; } = new List<Point3d>();

        public ILogger Logger { get; set; }

        public Point3d Current
        {
            get
            {
                if (_position == -1 || _position >= Points.Count)
                    throw new ArgumentException();
                return Points[_position];
            }
        }

        object IEnumerator.Current => throw new NotImplementedException();

        public void Set(IEdge<IVertex> edge)
        {
            var segments = _segmentFactory.GetFreeSegments(edge).ToList();
            if (segments.Count > 0)
            {
                Reset();
                segments.ForEach(_segmentsQueue.Enqueue);
            }
            else
            {
                Logger?.Warning($"Failed to get free segements from {edge}.");
            }
        }



        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            while (_segmentsQueue.Count > 0)
            {
                var currentSegment = _segmentsQueue.Dequeue();
                Points.Add(currentSegment.From);
                _position++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _position = -1;
            _segmentsQueue.Clear();
            Points.Clear();
        }
    }
}
