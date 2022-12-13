using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public abstract class Committer
    {
        public string ErrorMessages { get; protected set; }
        public string WarningMessages { get; protected set; }
        public abstract void Commit(Transaction transaction);
    }
}
