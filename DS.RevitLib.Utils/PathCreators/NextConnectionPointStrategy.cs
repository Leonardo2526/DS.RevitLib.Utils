using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Points;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    internal class NextConnectionPointStrategy : INextConnectionPointStrategy
    {
        private readonly Document _doc;
        private int _tolerance = 5;
        private int _cTolerance = 3;
        private bool _inverse;

        public NextConnectionPointStrategy(Document doc)
        {
            _doc = doc;
        }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Graph { get; set; }

        /// <summary>
        /// Path find tolerance.
        /// </summary>
        public int Tolerance { get => _tolerance; set => _tolerance = value; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public (Point3d point, Vector3d dir) GetPoint(Element element, XYZ point)
        {
            (Point3d p, Vector3d v) = Graph is null ?
                GetWithDefaultPoint(element, point, _inverse) :
                GetWithGraph(element, point, Graph, _inverse);
          
            _inverse = true;
            return (p.Round(_tolerance), v.Round(_tolerance));
        }

        (Point3d point, Vector3d dir) GetWithGraph(Element element, XYZ point,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, bool inverse)
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

            var mEPCurve = firstEdge.TryGetMEPCurve(_doc);
            if (mEPCurve == null)
            {
                Logger?.Warning("Failed to find MEPCurve on connection point.");
                return (default, default); 
            }

            var dir = - firstEdge.Direction(_doc);

            (Point3d sourcePoint1, Point3d targetPoint1) = firstEdge.GetConnectorsLocation(_doc);
            var location = vertex.GetLocation(_doc).ToPoint3d();
            var anp = location.DistanceTo(targetPoint1) < 0.001 ? default : targetPoint1;

            dir = inverse ? -dir : dir;

            return (anp, dir);
        }

        private (Point3d point, Vector3d dir) GetWithDefaultPoint(Element element, XYZ point, bool inverse)
        {
            var dir = GetDirection(new ConnectionPoint(element, point), out var anp, inverse);
            return (anp, dir);
        }

        //private (Point3d point, Vector3d dir) GetWithDefaultPoint(Element element, XYZ point)
        //{
        //    MEPCurve mc = element as MEPCurve;
        //    mc ??= element.GetBestConnected().OfType<MEPCurve>().FirstOrDefault();
        //    var dir = mc is null ? Vector3d.Zero : mc.Direction().ToVector3d();

        //    return (Point3d.Origin, dir);
        //}

        private Vector3d GetDirection(
               ConnectionPoint connectionPoint1,
               out Point3d aNP, bool inverse = false)
        {
            var mc = connectionPoint1.Element is MEPCurve curve ? curve : connectionPoint1.GetMEPCurve();

            var isManualDir = false;
            XYZ dirXYZ = connectionPoint1.Direction;
            if(dirXYZ == null)
            {
                dirXYZ= connectionPoint1.GetDirection(null, null, out bool isManual, null, new Autodesk.Revit.UI.UIDocument(_doc));
                if(dirXYZ == null) { aNP = default; return default; }
                isManualDir = isManual;
            }
            
            if (inverse && !isManualDir) { dirXYZ = dirXYZ.Negate(); }
            //_visualisator.ShowVectorByDirection(connectionPoint1.Point, dirXYZ);

            Vector3d dir = dirXYZ.ToVector3d();

            var dTolerance = Math.Pow(0.1, _cTolerance);
            var cons = ConnectorUtils.GetConnectors(mc);
            var line = mc.GetCenterLine();
            var conPoints = new List<XYZ>();
            cons.ForEach(c => conPoints.Add(line.Project(c.Origin).XYZPoint));
            conPoints = conPoints.OrderBy(c => c.DistanceTo(connectionPoint1.Point)).
                Where(c => c.DistanceTo(connectionPoint1.Point) > dTolerance).ToList();

            if (mc.Id != connectionPoint1.Element.Id) { conPoints.RemoveAt(0); }
            var checkDir = inverse ? dirXYZ.Negate() : dirXYZ;

            var aTolerance = 3.DegToRad();
            Func<XYZ, bool> func = (c) => Math.Round((connectionPoint1.Point - c).Normalize().AngleTo(checkDir)) == 0;
            var foundCon = conPoints.FirstOrDefault(func);
            if (foundCon == null)
            { aNP = default; }
            else
            {
                aNP = foundCon.ToPoint3d();
                //_pointVisualisator.Show(aNP);
            }

            return dir.Round(_tolerance);
        }

    }
}
