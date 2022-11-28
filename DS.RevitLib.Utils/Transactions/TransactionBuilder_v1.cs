using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transactions;
using System;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// Class for transaction creation.
    /// </summary>
    /// <typeparam name="T">T is output result of building.</typeparam>
    public class TransactionBuilder_v1<T> : AbstractTransactionBuilder
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
        public TransactionBuilder_v1(Document doc, Committer committer = null, string transactionPrefix = null)
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

        /// <summary>
        /// Build new transaction.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        /// <returns>Returns object of transacion.</returns>
        public T Build(Func<T> operation, string transactionName)
        {
            T result = default;
            using (Transaction transNew = new(_doc, _transactionPrefix + transactionName))
            {
                transNew.Start();
                result = operation.Invoke();

                _committer?.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            return result;
        }

        /// <summary>
        ///  Build some transaction operation
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transactionName"></param>
        public override void Build(Action operation, string transactionName)
        {
            using (Transaction transNew = new(_doc, _transactionPrefix + transactionName))
            {
                transNew.Start();
                operation.Invoke();

                _committer?.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }
        }

        public override Element Build(Func<Element> operation, string transactionName)
        {
            return Build(operation, transactionName);
        }
    }
}
