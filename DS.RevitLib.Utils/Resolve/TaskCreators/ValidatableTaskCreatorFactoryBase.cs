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
    /// The base class to produce validatable <see cref="ITaskCreator{TTask}"/>.
    /// </summary>
    /// <typeparam name="TTask"></typeparam>   
    public abstract class ValidatableTaskCreatorFactoryBase<TTask> : ITaskCreatorFactory<(TTask,TTask)>
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
        /// Instantiate a factory to produce validatable <see cref="ITaskCreator{TTask}"/>.
        /// </summary>
        /// <param name="uIDoc"></param>
        public ValidatableTaskCreatorFactoryBase(UIDocument uIDoc)
        {
            _uIDoc = uIDoc;
            _doc = _uIDoc.Document;
        }


        #region Properties

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }


        #endregion

        /// <inheritdoc/>
        public abstract ITaskCreator<(TTask,TTask)> Create();

        /// <summary>
        /// Get <typeparamref name="TTask"/> validators.
        /// </summary>
        /// <returns></returns>
        protected abstract List<IValidator<TTask>> GetValidators();
    }
}