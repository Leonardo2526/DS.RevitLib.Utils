using Autodesk.Private.InfoCenter;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Util methods for <see cref="GeometryObject"/>.
    /// </summary>
    public static class GeometryElementsUtils
    {
        /// <summary>
        ///  Creates a new circle as geometric <see cref="Arc"/> object based on <paramref name="centerPoint"/>.
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="normal">Normal vector to circle plane.</param>
        /// <param name="radius">Circle radius</param>
        /// <returns></returns>
        public static Arc CreateCircle(XYZ centerPoint, XYZ normal, double radius)
        {
            XYZ gen = XYZUtils.GenerateXYZ();
            XYZ xAxis = normal.CrossProduct(gen).Normalize();
            XYZ yAxis = normal.CrossProduct(xAxis).Normalize();
            return Arc.Create(centerPoint, radius, 0, 2 * Math.PI, xAxis, yAxis);
        }

        /// <summary>
        /// Show <see cref="Arc"/>.
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="doc"></param>
        /// <param name="transactionBuilder">Defaul builder to create <see cref="Autodesk.Revit.DB.Transaction"/>.</param>
        public static void Show(Arc arc, Document doc, AbstractTransactionBuilder transactionBuilder = null)
        {
            transactionBuilder ??= new TransactionBuilder(doc);
            transactionBuilder.Build(() =>
            {
                var creator = new ModelCurveCreator(doc);
                creator.Create(arc);
            }, "Show Arc");
        }

        /// <summary>
        /// Get all EdgeArrays from solid.
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="onlyPlanar"></param>
        /// <returns>Returns all EdgeArrays of <paramref name="faces"/>.</returns>
        public static List<EdgeArray> GetEdgeArrays(IEnumerable<Face> faces, bool onlyPlanar = false)
        {
            var edgeArrays = new List<EdgeArray>();
            foreach (Face face in faces)
            {
                if (onlyPlanar && face is not PlanarFace) { continue; }
                for (int i = 0; i < face.EdgeLoops.Size; i++)
                {
                    EdgeArray edgeArray = face.EdgeLoops.get_Item(i);
                    edgeArrays.Add(edgeArray);
                }
            }
            return edgeArrays;
        }

        /// <summary>
        /// Get all curves from solid.
        /// </summary>
        /// <param name="edgeArrays"></param>
        /// <returns>Returns all curves from edges of <paramref name="edgeArrays"/>.</returns>
        public static List<Curve> GetCurves(IEnumerable<EdgeArray> edgeArrays)
        {
            var curves = new List<Curve>();
            foreach (EdgeArray edgeArray in edgeArrays)
            {
                for (int i = 0; i < edgeArray.Size; i++)
                {
                    Edge edge = edgeArray.get_Item(i);
                    var curve = edge.AsCurve();
                    curves.Add(curve);
                }
            }

            return curves;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeArray"></param>
        /// <param name="offset"></param>
        /// <param name="referenceVector"></param>
        /// <returns></returns>
        public static List<Curve> GetOffsetCurves(EdgeArray edgeArray, double offset, XYZ referenceVector)
        {
            var curves = new List<Curve>();
            for (int i = 0; i < edgeArray.Size; i++)
            {
                Edge edge = edgeArray.get_Item(i);
                var curve = edge.AsCurve();
                curve = offset == 0 ?
                    curve :
                    curve.CreateOffset(offset, referenceVector);
                curves.Add(curve);
            }
            //return curves;
            List<Line> lines = curves.OfType<Line>().ToList();


            return lines.Any() ?
                new LinesConnector(lines).Connect().Cast<Curve>().ToList() :
                curves;
        }

        /// <summary>
        /// Get <see cref="Curve"/>s from <paramref name="wallFace"/> that belong to opening with specified <paramref name="openingId"/>.
        /// </summary>
        /// <param name="wallFace"></param>
        /// <param name="wall"></param>
        /// <param name="openingId"></param>
        /// <returns>
        /// Opening <see cref="Curve"/>s.
        /// <para>
        /// Empty list if no <see cref="Curve"/>s belong to opening with <paramref name="openingId"/>.
        /// </para>
        /// </returns>
        public static List<Curve> GetEdgesOnOpening(Face wallFace, Wall wall, ElementId openingId)
        {
            var edgesOnOpening = new List<Curve>();
            var mainEdgeArrays = GetEdgeArrays(new List<Face>() { wallFace });

            for (int i = 0; i < mainEdgeArrays.Count; i++)
            {
                EdgeArray edgeArray = mainEdgeArrays[i];
                var curves = new List<Curve>();
                for (int j = 0; j < edgeArray.Size; j++)
                {
                    Edge edge = edgeArray.get_Item(j);
                    if (wall.GetGeneratingElementIds(edge).Any(x => x == openingId))
                    {
                        var curve = edge.AsCurve();
                        curves.Add(curve);
                    }
                }
                if (curves.Count > 0)
                { edgesOnOpening.AddRange(curves); }
            }

            return edgesOnOpening;
        }

        /// <summary>
        /// Get <see cref="Curve"/>s from <paramref name="wallFace"/> that belong to any opening.
        /// </summary>
        /// <param name="wallFace"></param>
        /// <param name="wall"></param>
        /// <returns>
        /// Openings <see cref="Curve"/>s.
        /// <para>
        /// Empty dictionary if no openings exists or no <see cref="Curve"/>s belong to any opening.
        /// </para>
        /// </returns>
        public static Dictionary<ElementId, List<Curve>> GetEdgesOnOpenings(Face wallFace, Wall wall)
        {
            var dict = new Dictionary<ElementId, List<Curve>>();

            var insertsIds = wall.FindInserts(true, false, true, true);
            foreach (var inId in insertsIds)
            {
                var curves = GetEdgesOnOpening(wallFace, wall, inId);
                dict.Add(inId, curves);
            }

            return dict;
        }

        /// <summary>
        /// Convert <paramref name="lines"/> to <see cref="Rhino.Geometry.Line"/>s.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static IEnumerable<Rhino.Geometry.Line> ToRhinoLines(IEnumerable<Line> lines)
        {
            var result = new List<Rhino.Geometry.Line>();
            lines.ToList().ForEach(l => result.Add(l.ToRhinoLine()));
            return result;
        }

        /// <summary>
        /// Convert <paramref name="lines"/> to <see cref="Line"/>s.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static IEnumerable<Line> ToRevitLines(IEnumerable<Rhino.Geometry.Line> lines)
        {
            var result = new List<Line>();
            lines.ToList().ForEach(l => result.Add(l.ToXYZ()));
            return result;
        }

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Face"/>s of <paramref name="element"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// List of <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Face"/>s.
        /// <para>
        /// Empty list if failed to get <see cref="Autodesk.Revit.DB.Face"/>s.
        /// </para>
        /// </returns>
        public static IEnumerable<Face> GetFaces(Element element, Document activeDoc)
        {
            var result = new List<Face>();
            Solid solid = element.GetSolidInLink(activeDoc);
            result.AddRange(from Face item in solid.Faces
                            select item);
            return result;
        }

        /// <summary>
        /// Get edges from all <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Face"/>s.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// List of <paramref name="element"/>'s edges.
        /// <para>
        /// Empty list if failed to get edges.
        /// </para>
        /// </returns>
        public static IEnumerable<Curve> GetEdges(Element element, Document activeDoc)
        {
            var curves = new List<Curve>();
            var faces = GetFaces(element, activeDoc).ToList();
            faces.ForEach(f => curves.AddRange(f.GetEdges()));
            return curves;
        }

        /// <summary>
        /// Get edges from <paramref name="wall"/>'s <see cref="Autodesk.Revit.DB.Face"/>s and split them on similar with adjancies(joints) 
        /// and not.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="doc">Active document</param>
        /// <param name="onlyExternalEdges"></param>
        /// <returns>
        ///  <paramref name="wall"/>'s edges splitted on two collections.
        /// <para>
        /// Empty collection if no edges exist.
        /// </para>
        /// </returns>
        public static
            (IEnumerable<Curve> freeEdges,
            Dictionary<ElementId, List<Curve>> joinElementsEdges)
            GetSplitedEdges(Wall wall,
            Document doc,
            bool onlyExternalEdges = false)
        {
            var freeEdges = new List<Curve>();
            var jointElementsEdges = new Dictionary<ElementId, List<Curve>>();

            var wallEdges = wall.GetEdges(doc).OfType<Line>();

            //get all faces of joints
            var jointElementsPlanarFaces = new Dictionary<ElementId, IEnumerable<PlanarFace>>();
            var jointsIds = new List<ElementId>();
            var wallJoints = wall.GetJoints(onlyExternalEdges);
            jointsIds.AddRange(wallJoints);
            var floorJoints = JoinGeometryUtils.GetJoinedElements(wall.Document, wall);
            jointsIds.AddRange(floorJoints);
            jointsIds.ForEach(j => jointElementsPlanarFaces.Add(j, GetFaces(wall.Document.GetElement(j), doc).OfType<PlanarFace>()));

            foreach (var wallEdge in wallEdges)
            {
                bool found = false;
                foreach (var kv in jointElementsPlanarFaces)
                {
                    var joinId = kv.Key;
                    var jointFaces = kv.Value;
                    if (jointFaces.Any(jf => jf.Contains(wallEdge)))
                    {
                        found = true;
                        if (!jointElementsEdges.TryGetValue(joinId, out var wallJointEdges))
                        { jointElementsEdges.Add(joinId, new List<Curve>() { wallEdge }); }
                        else
                        { wallJointEdges.Add(wallEdge); }
                    }
                }
                if (!found)
                { freeEdges.Add(wallEdge); }
            }
            return (freeEdges, jointElementsEdges);
        }
    }
}
