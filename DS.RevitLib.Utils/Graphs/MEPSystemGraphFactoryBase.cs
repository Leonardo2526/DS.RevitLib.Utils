using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// An object that represents factory to create <typeparamref name="TGraph"/>.
    /// </summary>
    /// <typeparam name="TGraph"></typeparam>
    public abstract class MEPSystemGraphFactoryBase<TGraph> : IGraphFactory<TGraph, Element>
    {
        /// <summary>
        /// Current active Revit <see cref="Document"/>.
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// Created graph.
        /// </summary>
        protected TGraph _graph;

        /// <summary>
        /// Instantiate an object that represents factory to create <typeparamref name="TGraph"/>.
        /// </summary>
        public MEPSystemGraphFactoryBase(Document doc)
        {
            _doc = doc;
        }

        /// <inheritdoc/>
        public TGraph Graph => _graph;

        /// <inheritdoc/>
        public abstract TGraph Create(Element element);

        /// <summary>
        /// Create <typeparamref name="TGraph"/> by <paramref name="pointOnMEPCurve"/> root.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="pointOnMEPCurve"></param>
        /// <returns>
        /// A new <typeparamref name="TGraph"/>.
        /// </returns>
        public abstract TGraph Create(MEPCurve mEPCurve, XYZ pointOnMEPCurve);
    }
}
