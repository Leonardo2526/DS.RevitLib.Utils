using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.TransactionCommitter
{
    public abstract class Committer
    {
        public string ErrorMessages { get; protected set; }
        public string WarningMessages { get; protected set; }
        public abstract void Commit(Transaction transaction);
    }
}
