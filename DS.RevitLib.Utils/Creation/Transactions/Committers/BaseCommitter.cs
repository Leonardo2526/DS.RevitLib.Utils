using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public class BaseCommitter : Committer
    {
        /// <inheritdoc/>
        public override void Commit(Transaction transaction, bool commitTransaction = true)
        {
            if (transaction.HasStarted())
            {
                if (commitTransaction) { transaction.Commit(); }
                else { transaction.RollBack(); }                
            }
        }
    }
}
