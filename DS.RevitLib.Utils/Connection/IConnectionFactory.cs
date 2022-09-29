namespace DS.RevitLib.Utils.Connection
{
    /// <summary>
    /// Interface for classes of connection factories.
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// Connect elements.
        /// </summary>
        /// <returns>Returns true if connection was successfull.</returns>
        public bool Connect();
    }
}
