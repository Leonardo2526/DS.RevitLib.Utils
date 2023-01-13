using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transactions.Committers
{
    public interface ITransactionGroupCommitter
    {
        /// <summary>
        /// Perform operation with <paramref name="transactionGroup"/> to close it.
        /// </summary>
        /// <param name="transactionGroup"></param>
        public void Close(TransactionGroup transactionGroup);

        /// <summary>
        /// Perform operation with <paramref name="transactionGroup"/> to close it on <paramref name="taskEvent"/>.
        /// </summary>
        /// <param name="transactionGroup"></param>
        /// <param name="taskEvent"></param>
        public void Close(TransactionGroup transactionGroup, TaskComplition taskEvent);
    }
}
