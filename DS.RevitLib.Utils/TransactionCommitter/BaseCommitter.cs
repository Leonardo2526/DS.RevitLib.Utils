using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public class BaseCommitter : Committer
    {
        public override void Commit(Transaction transaction)
        {
            if (transaction.HasStarted())
            {
                transaction.Commit();
                var name = transaction.GetName();
            }
        }
    }
}
