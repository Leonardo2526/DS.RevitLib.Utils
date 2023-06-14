using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public class RollBackCommitter : Committer
    {
        public override void Commit(Transaction transaction, bool commitTransaction = true)
        {
            if (transaction.HasStarted())
            {
                if (commitTransaction) { transaction.Commit(); }
                else { transaction.RollBack(); }

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
