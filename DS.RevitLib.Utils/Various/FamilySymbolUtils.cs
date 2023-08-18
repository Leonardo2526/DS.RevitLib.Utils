using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;

namespace DS.RevitLib.Utils
{
    public class FamilySymbolUtils : AbstractCreator
    {

        public FamilySymbolUtils(Committer committer = null, string transactionPrefix = "") :
        base(committer, transactionPrefix)
        { }

        /// <summary>
        /// Get length of family instance created by family symbol with base element as size base for set parameters.
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="document"></param>
        /// <param name="baseElement"></param>
        /// <returns>Return length between main connectors of created family instance.</returns>
        public double GetLength(FamilySymbol familySymbol, Document document, Element baseElement = null)
        {
            var famInstCreator = new FamInstTransactions(document, _committer, _transactionPrefix);
            FamilyInstance famInst = famInstCreator.
                CreateFamilyInstane(familySymbol, new XYZ(0, 0, 0), null, baseElement, CopyParameterOption.Sizes);

            var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(famInst);
            double length = famInstCon1.Origin.DistanceTo(famInstCon2.Origin);

            ElementUtils.DeleteElement(document, famInst);

            return length;
        }
    }
}
