﻿using Autodesk.Revit.DB;
using Revit.Async;
using System;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transactions
{
    /// <summary>
    /// Interface for transaction creation.
    /// </summary>
    public abstract class AbstractTransactionBuilder
    {

        /// <summary>
        /// Create the new instance to build transaction.       
        /// </summary>
        /// <param name="doc"></param>
        public AbstractTransactionBuilder(Document doc)
        {
            Doc = doc;
        }

        /// <summary>
        /// Document to commit transactions.
        /// </summary>
        public Document Doc { get; }

        /// <summary>
        /// Build new transaction.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        /// <returns>Returns object of transacion.</returns>
        public abstract T Build<T>(Func<T> operation, string transactionName, bool commitTransaction = true);

        /// <summary>
        /// Build transaction.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        public abstract void Build(Action operation, string transactionName, bool commitTransaction = true);

        /// <summary>
        /// Build transaction asynchronously.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        /// <param name="commitTransaction"></param>
        /// <returns></returns>
        public abstract Task BuilAsync(Action operation, string transactionName, bool commitTransaction = true);


        /// <summary>
        /// Build transaction asynchronously with <see cref="RevitTask"/>.
        /// </summary>
        /// <param name="operation">Transaction action.</param>
        /// <param name="transactionName"></param>
        /// <returns></returns>
        public async Task BuildRevitTask(Action operation, string transactionName, bool commitTransaction = true)
        {
            await RevitTask.RunAsync(() => Build(operation, transactionName));
        }
    }
}
