using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using System;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// Class for transaction creation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TransactionBuilder<T>
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

        /// <summary>
        /// Build new transaction.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public T Build(Func<T> operation)
        {
            T result = default;
            using (Transaction transNew = new(_doc, _transactionPrefix + $"{operation}"))
            {
                try
                {
                    transNew.Start();
                    result = operation.Invoke();
                }
                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer?.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            return result;
        }

        /// <summary>
        /// Build some transaction operation
        /// </summary>
        /// <param name="operation"></param>
        public void Build(Action operation)
        {
            using (Transaction transNew = new(_doc, _transactionPrefix + $"{operation}"))
            {
                try
                {
                    transNew.Start();
                    operation.Invoke();
                }
                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer?.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }
        }
    }
}
