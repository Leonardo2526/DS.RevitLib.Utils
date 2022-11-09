using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using System;

namespace ClassLibrary1
{
    public class TransactionBuilder<T>
    {
        private readonly Document _doc;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        public TransactionBuilder(Document doc, Committer committer = null, string transactionPrefix = null)
        {
            _doc = doc;
            _committer = committer is null ? new BaseCommitter() : committer;
            _transactionPrefix = string.IsNullOrEmpty(transactionPrefix) ? null : transactionPrefix + "_";
        }

        public string ErrorMessages { get; protected set; }
        public string WarningMessages { get; protected set; }

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
    }
}
