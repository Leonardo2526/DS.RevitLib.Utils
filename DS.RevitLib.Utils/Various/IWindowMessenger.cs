namespace DS.RevitLib.Utils
{
    /// <summary>
    /// The interface is used to show messages.
    /// </summary>
    public interface IWindowMessenger
    {
        /// <summary>
        /// Show <paramref name="message"/> with specified window <paramref name="title"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void Show(string message, string title = null);
    }
}
