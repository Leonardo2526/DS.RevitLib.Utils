using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using QuickGraph;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The interface is used to create builder for graphs.
    /// </summary>
    public interface IMEPGraphBuilder
    {
        /// <summary>
        /// Create graph by <paramref name="element"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Create(Element element);

        /// <summary>
        /// Create graph by <paramref name="pointOnMEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="pointOnMEPCurve"></param>
        /// <returns></returns>
        IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Create(MEPCurve mEPCurve, XYZ pointOnMEPCurve);
    }
}