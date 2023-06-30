using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transactions;
using DS.RevitLib.Utils.Various;
using Revit.Async;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// Class for transaction creation.
    /// </summary>
    public class TransactionBuilder : AbstractTransactionBuilder
    {
        private readonly bool _isRevitContext;
        private Committer _committer;

        /// <summary>
        /// Create the new instance to build transaction.       
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="committer"></param>
        /// <param name="transactionPrefix"></param>
        /// <param name="isRevitContext">Specifies if asynchronous transactions will be performed with <see cref="RevitTask"/> or not.</param>
        public TransactionBuilder(Document doc, Committer committer = null, string transactionPrefix = null, bool isRevitContext = true) : 
            base(doc)
        {
            _isRevitContext = isRevitContext;
            _committer = committer is null ? new BaseCommitter() : committer;
            Prefix = string.IsNullOrEmpty(transactionPrefix) ? null : transactionPrefix + "_";
        }     

        /// <summary>
        /// Prefis used for transactions names.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Messages with errors prevented to commit transaction.
        /// </summary>
        public string ErrorMessages => _committer.ErrorMessages;

        /// <summary>
        /// Messages with warnings after committing transaction.
        /// </summary>
        public string WarningMessages => _committer.WarningMessages;

        /// <inheritdoc/>
        public override T Build<T>(Func<T> operation, string transactionName, bool commitTransaction = true)
        {
            T result = default;
            var trName = Prefix + transactionName;

            //Debug.WriteLine($"Trying to commit transaction '{trName}'...");
            using (Transaction transNew = new(Doc, Prefix + transactionName))
            {
                transNew.Start();
                result = operation.Invoke();

                _committer.Commit(transNew, commitTransaction);
            }

            return result;
        }

        public T BuildCatch<T>(Func<T> operation, string transactionName, bool commitTransaction = true)
        {
            T result = default;
            var trName = Prefix + transactionName;
            try
            {
                //Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transNew = new(Doc, Prefix + transactionName))
                {
                    transNew.Start();
                    result = operation.Invoke();

                    _committer.Commit(transNew, commitTransaction);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                Debug.WriteLine($"Transaction '{trName}' was canceled.");
            }

            return result;
        }

        /// <inheritdoc/>
        public override void Build(Action operation, string transactionName, bool commitTransaction = true)
        {
            var trName = Prefix + transactionName;

            //Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transaction = new(Doc, trName))
                {
                    transaction.Start();
                    operation.Invoke();

                    _committer.Commit(transaction, commitTransaction);
                    //Debug.WriteLine($"Transaction '{trName}' is committed successfully!");
                }
        }

        /// <summary>
        /// Build transaction asynchronously.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        /// <param name="commitTransaction"></param>
        /// <returns></returns>
        public override async Task BuilAsync(Action operation, string transactionName, bool commitTransaction = true)
        {
            var action = () => Build(operation, transactionName, commitTransaction);
            if (_isRevitContext) { action(); }
            else { await RevitTask.RunAsync(() => action()); }
        }

        /// <summary>
        /// Build transaction asynchronously.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        /// <param name="commitTransaction"></param>
        /// <returns>Returns object of transaction.</returns>
        public async Task<T> BuilAsync<T>(Func<T> operation, string transactionName, bool commitTransaction = true)
        {
            T func() => Build(operation, transactionName, commitTransaction);
            return _isRevitContext ? func() : await RevitTask.RunAsync(func);
        }

        public void BuildCatch(Action operation, string transactionName, bool commitTransaction = true)
        {
            var trName = Prefix + transactionName;
            try
            {
                Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transaction = new(Doc, trName))
                {
                    transaction.Start();
                    operation.Invoke();

                    _committer.Commit(transaction, commitTransaction);
                    Debug.WriteLine($"Transaction '{trName}' is committed successfully!");
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                Debug.WriteLine($"Transaction '{trName}' was canceled.");
            }
        }

        /// <summary>
        /// Clear all <see cref="_committer"/> messages.
        /// </summary>
        public void ClearMessages()
        {
            _committer.ErrorMessages = null;
            _committer.WarningMessages = null;
        }
    }
}
