using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Various;
using System;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Creation.Transactions
{
    /// <summary>
    /// Interface for factories to create and perform transactions.
    /// </summary>
    public interface ITransactionFactory
    {
        /// <summary>
        /// Current document.
        /// </summary>
        public Document Doc { get;}

        /// <summary>
        /// Create and peform transaction synchronously.
        /// </summary>
        /// <param name="operation">Specified transaction's body operation.</param>
        /// <param name="transactionName">Specified transaction's name.</param>
        /// <param name="commitTransaction"></param>
        /// <returns>Returns an object of transaction's operation.</returns>
        //T Create<T>(Func<T> operation, string transactionName);

        /// <summary>
        /// Create and perform transaction asynchronously.
        /// </summary>
        /// <param name="operation">Specified transaction's body operation.</param>
        /// <param name="transactionName">Specified transaction's name.</param>
        /// <param name="commitTransaction">Specifies whether commit started transactions or rollBack.</param>
        /// <returns>A value of transaction's <see cref="System.Threading.Tasks.Task"/> operation.</returns>
        Task<T> CreateAsync<T>(Func<T> operation, string transactionName, bool commitTransaction = true);

        /// <summary>
        /// Create and perform transaction asynchronously.
        /// </summary>
        /// <param name="operation">Specified transaction's body operation.</param>
        /// <param name="transactionName">Specified transaction's name.</param>
        /// <param name="commitTransaction">Specifies whether commit started transactions or rollBack.</param>
        /// <returns>Transaction's <see cref="System.Threading.Tasks.Task"/> operation.</returns>
        Task CreateAsync(Action operation, string transactionName, bool commitTransaction = true);
    }
}
