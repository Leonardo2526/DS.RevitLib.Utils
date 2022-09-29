using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    /// <summary>
    /// Interface for classes to connect elements.
    /// </summary>
    public interface IConnectionStrategy
    {
        /// <summary>
        /// Elements used to for connection.
        /// </summary>
        public FamilyInstance ConnectionElement { get; }

        /// <summary>
        /// Connect elements.
        /// </summary>
        /// <returns>Returns true if connection was successfull.</returns>
        public bool Connect();

        /// <summary>
        /// Check if connection is available.
        /// </summary>
        /// <returns>Returns true if connection is available with sizes and position of current elements.</returns>
        public bool IsConnectionAvailable();
    }
}
