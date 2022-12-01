using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Events;
using DS.RevitLib.Utils.Transactions.Committers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public class CommitOnApply : ITransactionGroupCommitter
    {
        public void Close(TransactionGroup transactionGroup)
        {
            throw new NotImplementedException();
        }

        public void Close(TransactionGroup transactionGroup, TaskComplition taskEvent)
        {
            Debug.IndentLevel = 1;
            string trgName = $"TransactionGroup {transactionGroup.GetName()}";
            if (!transactionGroup.HasStarted())
            {
                Debug.WriteLine($"{trgName} was not closed due to it hasn't been started.");
                return;
            }

            if (taskEvent.EventType == EventType.Apply)
            {
                transactionGroup.Assimilate();
                Debug.WriteLine($"{trgName} was committed");
            }
            else
            {
                transactionGroup.RollBack();
                Debug.WriteLine($"{trgName} was rolled");
            }
        }
    }
}
