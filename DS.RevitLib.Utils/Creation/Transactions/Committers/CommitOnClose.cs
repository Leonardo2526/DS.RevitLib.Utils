using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
    public class CommitOnClose : ITransactionGroupCommitter
    {
        public void Close(TransactionGroup transactionGroup)
        {
            throw new NotImplementedException();
        }

        public void Close(TransactionGroup transactionGroup, TaskComplition taskEvent)
        {
            string trgName = $"TransactionGroup {transactionGroup.GetName()}";
            if (!transactionGroup.HasStarted())
            {
                Debug.WriteLine($"{trgName} was not closed due to it hasn't been started.");
                return;
            }

            switch (taskEvent.EventType)
            {
                case EventType.Onward:
                    transactionGroup.RollBack();
                    Debug.WriteLine($"{trgName} was rolled");
                    break;
                case EventType.Backward:
                    transactionGroup.RollBack();
                    Debug.WriteLine($"{trgName} was rolled");
                    break;
                case EventType.Rollback:
                    transactionGroup.RollBack();
                    Debug.WriteLine($"{trgName} was rolled");
                    break;
                case EventType.Apply:
                    transactionGroup.Assimilate();
                    Debug.WriteLine($"{trgName} was committed");
                    break;
                case EventType.Close:
                    transactionGroup.Assimilate();
                    Debug.WriteLine($"{trgName} was committed");
                    break;
                default:
                    break;
            }

        }
    }
}
