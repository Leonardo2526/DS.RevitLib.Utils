using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.Symbols;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var famInstCreator = new FamInstCreator(document, _committer, _transactionPrefix);
            FamilyInstance famInst = famInstCreator.CreateFamilyInstane(familySymbol, new XYZ(0,0,0));

            if (baseElement is not null)
            {
                var parameters = MEPElementUtils.GetSizeParameters(baseElement);
                famInstCreator.SetSizeParameters(famInst, parameters);
            }

            var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(famInst);
            double length = famInstCon1.Origin.DistanceTo(famInstCon2.Origin);

            ElementUtils.DeleteElement(document, famInst);

            return length;
        }      
    }
}
