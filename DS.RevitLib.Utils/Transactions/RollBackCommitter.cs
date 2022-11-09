using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public class RollBackCommitter : Committer
    {
        public override void Commit(Transaction transaction)
        {
            if (transaction.HasStarted())
            {
                transaction.Commit();
                var name = transaction.GetName();
                if (name.Contains("RolledBack"))
                {
                    ErrorMessages += name + "\n";
                }
                if (name.Contains("Warning"))
                {
                    WarningMessages += name + "\n";
                }
            }
        }
    }
}
