using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using System;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class ParameterSetter : AbstractCreator
    {
        private readonly Element _element;

        public ParameterSetter(Element element, Committer committer = null, string transactionPrefix = "") :
            base(committer, transactionPrefix)
        {
            _element = element;
        }

        /// <summary>
        /// Set new value to element's parameter
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurve with swaped parameters.</returns>
        public Element SetValue(Parameter parameter, double value)
        {
            if (parameter is null)
            {
                return _element;
            }
            using (Transaction transNew = new Transaction(_element.Document, _transactionPrefix + "SetParameter"))
            {
                try
                {
                    transNew.Start();

                    parameter.Set(value);
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer?.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
                WarningMessages += _committer?.WarningMessages;
            }
            return _element;
        }
    }
}
