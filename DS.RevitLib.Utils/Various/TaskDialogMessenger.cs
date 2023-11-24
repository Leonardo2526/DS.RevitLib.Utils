using Autodesk.Revit.UI;

namespace DS.RevitLib.Utils.Various
{
    /// <summary>
    /// The object to show messages.
    /// </summary>
    public class TaskDialogMessenger : IWindowMessenger
    {
        private readonly TaskDialog _taskDialog;
        private readonly string _title = "TaskDialogMessenger";

        /// <summary>
        /// Instansiate a messenger thar show messages with <see cref="TaskDialog"/>.
        /// </summary>
        public TaskDialogMessenger()
        {
            _taskDialog = new TaskDialog(_title);
        }


        /// <inheritdoc/>
        public void Show(string message, string title = null)
        {
            _taskDialog.Title = title ?? _title;
            _taskDialog.MainInstruction = message;
            _taskDialog.Show();
        }
    }
}
