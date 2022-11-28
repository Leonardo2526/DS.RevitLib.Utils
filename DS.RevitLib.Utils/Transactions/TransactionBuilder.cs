using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transactions;
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
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        /// <summary>
        /// Create the new instance to build transaction.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="committer"></param>
        /// <param name="transactionPrefix"></param>
        public TransactionBuilder(Document doc, Committer committer = null, string transactionPrefix = null)
        {
            _doc = doc;
            _committer = committer is null ? new BaseCommitter() : committer;
            _transactionPrefix = string.IsNullOrEmpty(transactionPrefix) ? null : transactionPrefix + "_";
        }

        /// <summary>
        /// Messages with errors prevented to commit transaction.
        /// </summary>
        public string ErrorMessages { get; protected set; }

        /// <summary>
        /// Messages with warnings after committing transaction.
        /// </summary>
        public string WarningMessages { get; protected set; }

        /// <inheritdoc/>
        public override Element Build(Func<Element> operation, string transactionName)
        {
            Element result = default;
            var trName = _transactionPrefix + transactionName;
            try
            {
                Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transNew = new(_doc, _transactionPrefix + transactionName))
                {
                    transNew.Start();
                    result = operation.Invoke();

                    _committer.Commit(transNew);
                    ErrorMessages += _committer?.ErrorMessages;
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
        public override void Build(Action operation, string transactionName)
        {
            var trName = _transactionPrefix + transactionName;
            try
            {
                Debug.WriteLine($"Trying to commit transaction '{trName}'...");
                using (Transaction transaction = new(_doc, trName))
                {
                    transaction.Start();
                    operation.Invoke();

                    _committer.Commit(transaction);
                    ErrorMessages += _committer?.ErrorMessages;
                    Debug.WriteLine($"Transaction '{trName}' is committed successfully!");
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                Debug.WriteLine($"Transaction '{trName}' was canceled.");
            }
        }

    }
}
