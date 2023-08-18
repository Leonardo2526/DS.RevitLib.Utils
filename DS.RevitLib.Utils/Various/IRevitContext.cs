using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Various
{
    /// <summary>
    /// An object that represents Revit context.
    /// </summary>
    public interface IRevitContext
    {
        /// <summary>
        /// Specifies whether current <see cref="Document"/>'s state is in Revit context.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if transactions are available. Otherwise returns <see langword="false"/>.</returns>
        public bool IsRevitContext { get; }
    }
}
