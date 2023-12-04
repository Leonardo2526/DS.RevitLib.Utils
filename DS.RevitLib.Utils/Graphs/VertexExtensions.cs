using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.GridMap;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods for vertices.
    /// </summary>
    public static class VertexExtensions
    {
        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.XYZ"/> point that specified <paramref name="vertex"/> location.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="doc"></param>
        /// <returns>
        ///  Convereted to <see cref="Autodesk.Revit.DB.XYZ"/> tag property of <paramref name="vertex"/> if it's <see cref="Point3d"/>.
        /// <para>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> built by tag property of <paramref name="vertex"/> if tag specified on 
        /// <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s <see cref="Autodesk.Revit.DB.ElementId"/>.
        /// </para>
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static XYZ GetLocation(this IVertex vertex, Document doc)
        {
            XYZ xYZPoint = null;
            switch (vertex)
            {
                case TaggedGVertex<Point3d> point:
                    { xYZPoint = point.Tag.ToXYZ(); }
                    break;
                case TaggedGVertex<int> famInsVertex:
                    { xYZPoint = GetLocation(famInsVertex.Tag); }
                    break;
                default: break;
            }

            return xYZPoint;

            XYZ GetLocation(int tag)
            {
                var famInst = doc.GetElement(new ElementId(tag)) as FamilyInstance;
                return famInst.GetLocation();
            }
        }

        /// <summary>
        /// Try get <see cref="Autodesk.Revit.DB.FamilyInstance"/> from <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.FamilyInstance"/> if <paramref name="vertex"/> is <see cref="TaggedGVertex{TTag}"/>.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static FamilyInstance TryGetFamilyInstance(this IVertex vertex, Document doc)
        {
            var tag = vertex is TaggedGVertex<int> tagged ?
                tagged.Tag :
                -1;
            return tag == -1 ?
                    null :
                    doc.GetElement(new ElementId(tag)) as FamilyInstance;
        }

        /// <summary>
        /// Specifies if <paramref name="types"/> contains <paramref name="vertex"/>'s <see cref="Autodesk.Revit.DB.FamilyInstance"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="types"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="vertex"/>'s type is <see cref="Autodesk.Revit.DB.FamilyInstance"/>
        /// and <paramref name="types"/> contains it.
        /// <para>     
        /// Otherwise <see langword="false"/>.</para>
        /// </returns>
        public static bool ContainsTypes(this IVertex vertex, IEnumerable<Type> types, Document doc)
        {
            if (types is null || types.Count() == 0) return false;

            var famInst = vertex.TryGetFamilyInstance(doc);
            if (famInst == null) { return false; }

            var type = famInst.GetType();
            return types.Contains(type);
        }


        /// <summary>
        /// Specifies if <paramref name="categories"/> contains <paramref name="vertex"/>'s <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="categories"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="vertex"/>'s type is <see cref="Autodesk.Revit.DB.FamilyInstance"/>
        /// and <paramref name="categories"/> contains <paramref name="vertex"/>'s <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.
        /// <para>     
        /// Otherwise <see langword="false"/>.</para>
        /// </returns>
        public static bool ContainsCategories(this IVertex vertex, Dictionary<BuiltInCategory, List<PartType>> categories, Document doc)
        {
            if (categories is null || categories.Count() == 0) return false;

            var famInst = vertex.TryGetFamilyInstance(doc);
            if (famInst == null) { return false; }

            return famInst.IsCategoryElement(categories);
        }

        /// <summary>
        /// Get input <see cref="Autodesk.Revit.DB.Element"/>'s of <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="bdGraph"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s of input edges 
        /// (<see cref="TaggedEdge{TVertex, TTag}"/>) or input vertices (<see cref="TaggedGVertex{TTag}"/>) if input edge is untagged.
        /// <para>
        /// Empty list if no input <see cref="Autodesk.Revit.DB.Element"/>'s was found.
        /// </para>
        /// </returns>
        public static IEnumerable<Element> GetInElements(this IVertex vertex,
            IBidirectionalGraph<IVertex, Edge<IVertex>> bdGraph, Document doc)
        {
            var inElements = new List<Element>();

            bdGraph.TryGetInEdges(vertex, out var inEdges);
            if (inEdges is null) return inElements;

            foreach (var inEdge in inEdges)
            {
                Element inElement = null;
                if (inEdge is TaggedEdge<IVertex, int> tagged)
                {
                    inElement = doc.GetElement(new ElementId(tagged.Tag));
                }
                else if (inEdge is Edge<IVertex> untagged)
                {
                    if (untagged.Source is TaggedGVertex<int> taggedVertex)
                    { inElement = doc.GetElement(new ElementId(taggedVertex.Tag)); }
                }

                if (inElement != null)
                { inElements.Add(inElement); }
            }

            return inElements;
        }

        /// <summary>
        /// Get output <see cref="Autodesk.Revit.DB.Element"/>'s of <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="graph"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s of output edges 
        /// (<see cref="TaggedEdge{TVertex, TTag}"/>) or output vertices (<see cref="TaggedGVertex{TTag}"/>) if output edge is untagged.
        /// <para>
        /// Empty list if no output <see cref="Autodesk.Revit.DB.Element"/>'s was found.
        /// </para>
        /// </returns>
        public static IEnumerable<Element> GetOutElements(this IVertex vertex,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            var outElements = new List<Element>();

            graph.TryGetOutEdges(vertex, out var outEdges);
            if (outEdges is null) return outElements;

            foreach (var inEdge in outEdges)
            {
                Element inElement = null;
                if (inEdge is TaggedEdge<IVertex, int> tagged)
                {
                    inElement = doc.GetElement(new ElementId(tagged.Tag));
                }
                else if (inEdge is Edge<IVertex> untagged)
                {
                    if (untagged.Source is TaggedGVertex<int> taggedVertex)
                    { inElement = doc.GetElement(new ElementId(taggedVertex.Tag)); }
                }

                if (inElement != null)
                { outElements.Add(inElement); }
            }

            return outElements;
        }

        /// <summary>
        /// Get all input and output <see cref="Autodesk.Revit.DB.Element"/>'s of <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="bdGraph"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s edges 
        /// (<see cref="TaggedEdge{TVertex, TTag}"/>) or vertices (<see cref="TaggedGVertex{TTag}"/>) if edge is untagged.
        /// <para>
        /// Empty list if no <see cref="Autodesk.Revit.DB.Element"/>'s was found.
        /// </para>
        /// </returns>
        public static IEnumerable<Element> GetElements(this IVertex vertex,
          IBidirectionalGraph<IVertex, Edge<IVertex>> bdGraph, Document doc)
        {
            var elements = new List<Element>();

            var inElements = GetInElements(vertex, bdGraph, doc);
            var outElements = GetOutElements(vertex, bdGraph, doc);
            elements.AddRange(inElements);
            elements.AddRange(outElements);

            return elements;
        }

        /// <summary>
        /// Try to get <see cref="Autodesk.Revit.DB.Element"/> by <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="graph"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.FamilyInstance"/> if <paramref name="vertex"/> built by it.
        /// <para>
        /// <see cref="MEPCurve"/> from <paramref name="graph"/>'s edge if <paramref name="vertex"/> 
        /// built by <see cref="Point3d"/> and exits edge that has center line that contains this point.
        /// </para>
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static Element TryGetElementFromGraphAndDoc(this IVertex vertex,
        IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            Element element = null;

            switch (vertex)
            {
                case TaggedGVertex<int> taggedInt:
                    {
                        var famInst = taggedInt.TryGetFamilyInstance(doc);
                        if (famInst != null)
                        { element = famInst; }
                        break;
                    }
                case TaggedGVertex<Point3d> taggedPoint:
                    {
                        var edge = graph.TryGetEdge(taggedPoint.Tag, doc);
                        if (edge != null)
                        { element = doc.GetElement(new ElementId(edge.Tag)); }
                        break;
                    }
                default:
                    break;
            }

            return element;
        }

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.ElementId"/> from <paramref name="vertex"/>'s tag.
        /// <para>
        /// <paramref name="vertex"/> must be of <see cref="TaggedGVertex{TTag}"/> <see cref="Type"/>.
        /// </para>
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.ElementId"/> if <paramref name="vertex"/>'s tag contains <see cref="int"/> value.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static ElementId TryGetTagId(this IVertex vertex, Document doc)
        {
            Element element = null;
            switch (vertex)
            {
                case TaggedGVertex<int>:
                    {
                        element = TryGetFamilyInstance(vertex, doc);
                        break;
                    }
                case TaggedGVertex<(int, Point3d)> taggedIntPoint:
                    {
                        element = doc.GetElement(new ElementId(taggedIntPoint.Tag.Item1));
                        break;
                    }
                default:
                    break;
            }

            return element?.Id;
        }

        /// <summary>
        /// Convert <paramref name="vertex"/> to 
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>).
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>)
        /// if it was succefull to get <see cref="Autodesk.Revit.DB.Element"/> 
        /// and <see cref="Autodesk.Revit.DB.XYZ"/> from <paramref name="vertex"/>.
        /// <para>
        /// Otherwise (<see langword="null"/>, <see langword="null"/>).
        /// </para>
        /// </returns>
        public static (Element, XYZ) ToXYZElement(this IVertex vertex, Document doc)
        {
            (Element element, XYZ point) pointElement = (null, null);

            switch (vertex)
            {
                case TaggedGVertex<(int, Point3d)> taggedIntPoint:
                    {
                        var element = doc.GetElement(new ElementId(taggedIntPoint.Tag.Item1));
                        if (element != null)
                        { pointElement = (element, taggedIntPoint.Tag.Item2.ToXYZ()); }
                        break;
                    }
                case TaggedGVertex<int> taggedInt:
                    {
                        var element = doc.GetElement(new ElementId(taggedInt.Tag));
                        var lp = vertex.GetLocation(doc);
                        if (element != null)
                        { pointElement = (element, lp); }
                        break;
                    }
                default:
                    break;
            }

            return pointElement;
        }

        /// <summary>
        /// Convert <paramref name="graph"/>'s <paramref name="vertex"/> to 
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>).
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="graph"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>)
        /// if it was succefull to get <see cref="Autodesk.Revit.DB.Element"/> 
        /// and <see cref="Autodesk.Revit.DB.XYZ"/> from <paramref name="vertex"/>.
        /// <para>
        /// Otherwise (<see langword="null"/>, <see langword="null"/>).
        /// </para>
        /// </returns>
        public static (Element, XYZ) ToGraphXYZElement(this IVertex vertex,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            (Element element, XYZ point) pointElement = (null, null);

            switch (vertex)
            {
                case TaggedGVertex<Point3d> taggedPoint:
                    {
                        var elems = GetElements(vertex, graph.ToBidirectionalGraph(), doc);
                        var mc = elems.FirstOrDefault(e => e is MEPCurve);
                        if (mc != null)
                        { pointElement = (mc, taggedPoint.Tag.ToXYZ()); }
                        break;
                    }
                case TaggedGVertex<int> taggedInt:
                    {
                        var famInst = vertex.TryGetFamilyInstance(doc);
                        var lp = vertex.GetLocation(doc);
                        if (famInst != null)
                        { pointElement = (famInst, lp); }
                        break;
                    }
                default:
                    break;
            }

            return pointElement;
        }

        /// <summary>
        /// Convert <paramref name="vertex"/> to <see cref="IVertex"/> from <paramref name="graph"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="graph"></param>
        /// <returns>
        /// <see cref="IVertex"/> if <paramref name="vertex"/> tag id was found through <paramref name="graph"/>'s vertices tags.
        /// <para>
        /// Otherwise default value.
        /// </para>
        /// </returns>
        public static IVertex ToGraphVertex(this IVertex vertex,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            vertex = vertex is TaggedGVertex<(int, Point3d)> taggedIntPointVertex ?
             taggedIntPointVertex.ToVertexPoint(graph.VertexCount) :
             vertex;

            vertex.TryFindTaggedVertex(graph, out var foundVertex);
            return foundVertex;
        }

       
    }
}
