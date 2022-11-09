using DS.RevitLib.Utils.TransactionCommitter;
using System;

namespace DS.RevitLib.Utils
{
    public abstract class AbstractCreator
    {
        protected readonly Committer _committer;
        protected readonly string _transactionPrefix;

        public AbstractCreator(Committer committer = null, string transactionPrefix = null)
        {
            if (committer is null)
            {
                _committer = new BaseCommitter();
            }
            else
            {
                _committer = committer;
            }
            if (!String.IsNullOrEmpty(transactionPrefix))
            {
                _transactionPrefix = transactionPrefix + "_";
            }
        }

        public string ErrorMessages { get; protected set; }
        public string WarningMessages { get; protected set; }

    }
}
