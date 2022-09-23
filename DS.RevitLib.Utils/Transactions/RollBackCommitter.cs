using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
