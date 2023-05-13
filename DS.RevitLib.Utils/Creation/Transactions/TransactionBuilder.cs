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
        private readonly Document _doc;
        private readonly bool _isRevitContext;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        /// <summary>
        /// Create the new instance to build transaction.       
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="committer"></param>
        /// <param name="transactionPrefix"></param>
        /// <param name="isRevitContext">Specifies if asynchronous transactions will be performed with <see cref="RevitTask"/> or not.</param>
        public TransactionBuilder(Document doc, Committer committer = null, string transactionPrefix = null, bool isRevitContext = true)
        {
            _doc = doc;
            _isRevitContext = isRevitContext;
            _committer = committer is null ? new BaseCommitter() : committer;
            _transactionPrefix = string.IsNullOrEmpty(transactionPrefix) ? null : transactionPrefix + "_";
        }

        /// <summary>
        /// Messages with errors prevented to commit transaction.
        /// </summary>
        public string ErrorMessages { get; set; }

        /// <summary>
        /// Messages with warnings after committing transaction.
        /// </summary>
        public string WarningMessages { get; set; }

        /// <inheritdoc/>
        public override T Build<T>(Func<T> operation, string transactionName, bool commitTransaction = true)
        {
            T result = default;
            var trName = _transactionPrefix + transactionName;

            //Debug.WriteLine($"Trying to commit transaction '{trName}'...");
            using (Transaction transNew = new(_doc, _transactionPrefix + transactionName))
            {
                transNew.Start();
                result = operation.Invoke();

                _committer.Commit(transNew, commitTransaction);
            }
            ErrorMessages += _committer?.ErrorMessages;

            return result;
        }

        public T BuildCatch<T>(Func<T> operation, string transactionName, bool commitTransaction = true)
        {
            T result = default;
            var trName = _transactionPrefix + transactionName;
            try
            {
                //Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transNew = new(_doc, _transactionPrefix + transactionName))
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
            finally { ErrorMessages += _committer?.ErrorMessages; }

            return result;
        }

        /// <inheritdoc/>
        public override void Build(Action operation, string transactionName, bool commitTransaction = true)
        {
            var trName = _transactionPrefix + transactionName;

            //Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transaction = new(_doc, trName))
                {
                    transaction.Start();
                    operation.Invoke();

                    _committer.Commit(transaction, commitTransaction);
                    //Debug.WriteLine($"Transaction '{trName}' is committed successfully!");
                }
            ErrorMessages += _committer?.ErrorMessages;
        }

        /// <summary>
        /// Build transaction asynchronously.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        /// <param name="commitTransaction"></param>
        /// <returns></returns>
        public async Task BuilAsync(Action operation, string transactionName, bool commitTransaction = true)
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
            var trName = _transactionPrefix + transactionName;
            try
            {
                Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transaction = new(_doc, trName))
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
            finally { ErrorMessages += _committer?.ErrorMessages; }
        }

    }
}
