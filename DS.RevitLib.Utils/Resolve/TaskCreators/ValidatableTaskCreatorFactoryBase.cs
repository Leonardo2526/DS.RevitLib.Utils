using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using Serilog;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Resolve.TaskCreators
{
    /// <summary>
    /// The base class to produce <see cref="ITaskCreator{TItem, TTask}"/>.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TTask"></typeparam>   
    public abstract class ValidatableTaskCreatorFactoryBase<TItem, TTask> : ITaskCreatorFactory<TItem, (TTask,TTask)>
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// 
        /// </summary>
        protected readonly UIDocument _uIDoc;

        /// <summary>
        /// Instantiate a factory to produce <see cref="ITaskCreator{TItem, TTask}"/>.
        /// </summary>
        /// <param name="uIDoc"></param>
        public ValidatableTaskCreatorFactoryBase(UIDocument uIDoc)
        {
            _uIDoc = uIDoc;
            _doc = _uIDoc.Document;
        }


        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> AvailableCategories { get; set; }

        /// <summary>
        /// <see cref="MEPCurve"/> to get collisions on point.
        /// </summary>
        public MEPCurve BaseMEPCurve { get; set; }

        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline ExternalOutline { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ITraceSettings TraceSettings { get; set; }

        #endregion

        /// <inheritdoc/>
        public abstract ITaskCreator<TItem, (TTask,TTask)> Create();

        /// <summary>
        /// Get <typeparamref name="TTask"/> validators.
        /// </summary>
        /// <returns></returns>
        protected abstract List<IValidator<TTask>> GetValidators();
    }
}