using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public class BaseCommitter : Committer
    {
        public override void Commit(Transaction transaction)
        {
            if (transaction.HasStarted())
            {
                transaction.Commit();
            }
        }
    }
}
