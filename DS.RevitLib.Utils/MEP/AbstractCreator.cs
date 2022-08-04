using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    public abstract class AbstractCreator
    {
        protected readonly Document Doc;
        protected readonly Element _element;
        protected readonly string _transactionPrefix;
        protected readonly Committer _committer;

        public AbstractCreator(Element element, Committer committer = null, string transactionPrefix = "")
        {
            Doc = element.Document;
            _element = element;

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
