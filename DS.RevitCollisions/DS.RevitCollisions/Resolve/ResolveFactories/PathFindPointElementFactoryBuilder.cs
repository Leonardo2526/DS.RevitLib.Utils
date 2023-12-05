using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.Resolve.TaskCreators;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.ResolveFactories
{

    /// <inheritdoc/>
    public class PathFindPointElementFactoryBuilder :
        FactoryBuilderBase<((Element, XYZ), (Element, XYZ)), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        private ITransactionFactory _transactionFactory;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly XYZPathFinder _pathFinder;
        private readonly MEPCurve _baseMEPCurve;
        private readonly MEPCurve _basisMEPCurve1;
        private readonly MEPCurve _basisMEPCurve2;

        /// <summary>
        /// Instansiate a builder to create <see cref="IResolveFactory{TResult}"/> to find path
        /// between any (<see cref="Autodesk.Revit.DB.Element"/>, <see cref="Autodesk.Revit.DB.XYZ"/>) tuples
        /// in active <see cref="Document"/>.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="collisionDetector"></param>
        /// <param name="pathFinder"></param>
        /// <param name="baseMEPCurve"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        public PathFindPointElementFactoryBuilder(
            UIDocument uiDoc,
            IElementCollisionDetector collisionDetector,
            XYZPathFinder pathFinder,
            MEPCurve baseMEPCurve, MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2 = null)
        {
            _uiDoc = uiDoc;
            _collisionDetector = collisionDetector;
            _doc = uiDoc.Document;
            _pathFinder = pathFinder;
            _baseMEPCurve = baseMEPCurve;
            _basisMEPCurve1 = basisMEPCurve1;
            _basisMEPCurve2 = basisMEPCurve2;
        }

        #region Properties

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }

        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline ExternalOutline { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        public ITraceSettings TraceSettings { get; set; }

        public ITransactionFactory TransactionFactory { get; set; }

        #endregion



        /// <inheritdoc/>
        protected override ITaskCreator<((Element, XYZ), (Element, XYZ))> BuildTaskCreator()
        => new ManualXYZElementTaskCreatorFactory(_uiDoc, _collisionDetector)
        {
            AvailableCategories = IterationCategories,
            ExternalOutline = ExternalOutline,
            InsulationAccount = InsulationAccount,
            TraceSettings = TraceSettings,
            Messenger = Messenger,
            Logger = Logger,
            BaseMEPCurve = _baseMEPCurve
        }.Create();


        /// <inheritdoc/>
        protected override ITaskResolver<((Element, XYZ), (Element, XYZ)),
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> BuildTaskResover() =>
            new PathFindResolver(_pathFinder, _doc, _collisionDetector,
            _baseMEPCurve, _basisMEPCurve1, _basisMEPCurve2)
            {
                Logger = Logger,
            };



    }
}
