using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Points;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    internal class NextConnectionPointStrategy : INextConnectionPointStrategy
    {
        private readonly Document _doc;

        public NextConnectionPointStrategy(Document doc)
        {
            _doc = doc;
        }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Graph { get; set; }

        /// <summary>
        /// Converter to transform geometry objects between UCS.
        /// </summary>
        public IPoint3dConverter PointConverter { get; set; }


        public (Point3d point, Vector3d dir) GetPoint(Element element, XYZ point)
        {
            (Point3d p, Vector3d v) = Graph is null ? 
                GetWithDefaultPoint(element, point) :
                GetWithGraph(element, point, Graph);

            p = PointConverter is null ? p : PointConverter.ConvertToUCS2(p);
            v = PointConverter is null ? v : PointConverter.ConvertToUCS2(v);

            return (p, v);
        }

        ( Point3d point, Vector3d dir) GetWithGraph(Element element, XYZ point, 
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            var pointElement = (element.Id.IntegerValue, point.ToPoint3d());
            if (!graph.TryFindItems(pointElement, _doc, out var vertex, out var foundEdge) 
                || vertex == null)
            { throw new ArgumentException("Graph doesn't contains point element."); }

            graph.TryGetOutEdges(vertex, out var outEdges);
            var firstEdge = outEdges.FirstOrDefault();
            if (firstEdge == null)
            {
                graph.ToBidirectionalGraph().TryGetInEdges(vertex, out var inEdges);
                firstEdge = inEdges.FirstOrDefault();
            }

            var mEPCurve = firstEdge.TryGetMEPCurve(_doc) ??
                throw new ArgumentNullException("Failed to find MEPCurve on connection point.");

            var dir = firstEdge.Direction(_doc);

            (Point3d sourcePoint1, Point3d targetPoint1) = firstEdge.GetConnectorsLocation(_doc);
            var location = vertex.GetLocation(_doc).ToPoint3d();
            var anp = location.DistanceTo(targetPoint1) < 0.001 ? default : targetPoint1;

            return (anp, dir);
        }

        private (Point3d point, Vector3d dir) GetWithDefaultPoint(Element element, XYZ point)
        {
            MEPCurve mc = element.GetBestConnected().OfType<MEPCurve>().FirstOrDefault();
            var dir = mc is null ? Vector3d.Zero : mc.Direction().ToVector3d();

            return (Point3d.Origin, dir);
        }
    }
}
