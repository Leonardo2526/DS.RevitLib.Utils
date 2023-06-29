using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public abstract class Committer
    {
        public string ErrorMessages { get; set; }
        public string WarningMessages { get; set; }

        /// <summary>
        /// Perform commit.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="commitTransaction">Specify whether commit started transaction or rollback.</param>
        public abstract void Commit(Transaction transaction, bool commitTransaction = true);
    }
}
