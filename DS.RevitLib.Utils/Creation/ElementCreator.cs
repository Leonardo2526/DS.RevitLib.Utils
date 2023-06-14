using Autodesk.Revit.DB;
using DS.RevitLib.Utils.TransactionCommitter;
using System;

namespace DS.RevitLib.Utils.Elements.Creators
{
    public class ElementCreator : AbstractCreator
    {

        public ElementCreator(
          Committer committer = null, string transactionPrefix = "") :
          base(committer, transactionPrefix)
        { }

        /// <summary>
        /// Rotate element around it's axis by angle.
        /// </summary>
        /// <param name="operationElement"></param>
        /// <param name="axis"></param>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>Returns rotated element.</returns>
        public Element Rotate(Element operationElement, Line axis, double angle)
        {
            Document doc = operationElement.Document;
            using (var transNew = new Transaction(doc, _transactionPrefix + "Rotate " + operationElement.Id))
            {
                try
                {
                    transNew.Start();
                    operationElement.Location.Rotate(axis, angle);
                }
                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer?.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            return operationElement;
        }

    }
}
