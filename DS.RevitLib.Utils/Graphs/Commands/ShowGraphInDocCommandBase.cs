using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.GraphUtils.Entities.Command;
using DS.RevitLib.Utils.Creation.Transactions;
using QuickGraph;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs.Commands
{
    /// <summary>
    /// An object that represent base commands to show graph in <see cref="Document"/>.
    /// </summary>
    /// <typeparam name="TSourceVertex"></typeparam>
    public abstract class ShowGraphInDocCommandBase<TSourceVertex> : 
        ShowGraphCommandBase<TSourceVertex>,
        IShowGraphAsyncCommand<TSourceVertex>, 
        IItemVisualisator<IVertexAndEdgeListGraph<TSourceVertex, Edge<TSourceVertex>>> 
        where TSourceVertex : IVertex
    {
        protected readonly UIDocument _uiDoc;
        protected readonly Document _doc;
        protected ITransactionFactory _transactionFactory;

        /// <summary>
        /// Instansiate an object that represent base commands to show graph in <see cref="Document"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="uiDoc"></param>
        protected ShowGraphInDocCommandBase(IVertexAndEdgeListGraph<TSourceVertex, Edge<TSourceVertex>> graph,
            UIDocument uiDoc) : base(graph)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }

        /// <summary>
        /// Instansiate an object that represent base commands to show graph in <see cref="Document"/>.
        /// </summary>
        /// <param name="uiDoc"></param>
        protected ShowGraphInDocCommandBase(
            UIDocument uiDoc) : base()
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }

        /// <summary>
        /// Specified whether to show verticies tags or not.
        /// </summary>
        public bool ShowVertexIds { get; set; }

        /// <summary>
        /// Specified whether to show verticies tags or not.
        /// </summary>
        public bool ShowVertexTags { get; set; }

        /// <summary>
        /// Specified whether to show edges tags or not.
        /// </summary>
        public bool ShowEdgeTags { get; set; }

        /// <summary>
        /// A factory to commit <see cref="Autodesk.Revit.DB.Transaction"/>s.
        /// </summary>
        public ITransactionFactory TransactionFactory
        { get => _transactionFactory; set => _transactionFactory = value; }


        /// <inheritdoc/>
        public abstract Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> ShowGraphAsync();


        /// <inheritdoc/>
        public async Task<Edge<IVertex>> ShowAsync(Edge<TSourceVertex> edge)
            => await _transactionFactory?.CreateAsync(() => Show(edge), "show edge");

        /// <inheritdoc/>
        public async Task<IVertex> ShowAsync(TSourceVertex vertex)
             => await _transactionFactory?.CreateAsync(() => Show(vertex), "show edge");

        /// <inheritdoc/>
        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> ShowEdgesAsync()
       => await _transactionFactory?.CreateAsync(() => ShowEdges(), "show edges");

        /// <inheritdoc/>
        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> ShowVerticesAsync()
        => await _transactionFactory?.CreateAsync(() => ShowVertices(), "show vertices");

        /// <inheritdoc/>
        public void Show(IVertexAndEdgeListGraph<TSourceVertex, Edge<TSourceVertex>> graph)
        {
            SetGraph(graph);
            ShowGraph();
        }

        /// <inheritdoc/>
        public async Task ShowAsync(IVertexAndEdgeListGraph<TSourceVertex, Edge<TSourceVertex>> graph)
        {
            SetGraph(graph);
            await ShowGraphAsync();
        }

    }
}
