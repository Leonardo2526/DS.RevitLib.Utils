using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitLib.Utils.Various;
using Serilog;

namespace DS.RevitLib.Utils.Resolve.TaskCreators
{
    /// <summary>
    /// An object for manual create tasks to resolve collision.
    /// </summary>
    public class ManualXYZElementTaskCreator : ITaskCreator<object, ((Element, XYZ), (Element, XYZ))>
    {
        private readonly IValidatableSelector<(Element, XYZ)> _selector;

        /// <summary>
        /// Instantiate an object for manual create tasks to resolve collision.
        /// </summary>
        /// <param name="selector"></param>
        public ManualXYZElementTaskCreator(IValidatableSelector<(Element, XYZ)> selector)
        {
            _selector = selector;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public ((Element, XYZ), (Element, XYZ)) CreateTask(object item)
        {
            ((Element, XYZ), (Element, XYZ)) task = ((null, null), (null, null));

            var v1 = _selector.Select();
            if (v1.Item1 == null) { return task; }

            var v2 = _selector.Select();
            if (v2.Item1 == null) { return task; }

            task = (v1, v2);
            return task;
        }
    }
}
