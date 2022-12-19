using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Transactions;

namespace DS.RevitLib.Solids
{
    public interface ISolidCreator
    {
        Solid CreateSolid();

        void ShowSolid(Document doc, AbstractTransactionBuilder transactionBuilder = null);
    }
}
