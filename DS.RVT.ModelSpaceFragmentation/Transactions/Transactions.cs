using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.ModelSpaceFragmentation
{
    class TransactionCreator
    {
        readonly Document Doc;

        public TransactionCreator(Document doc)
        {
            Doc = doc;
        }

        public bool RefreshOnCommit { get; set; } = false;

        public void Create(ITransaction transaction)
        {
            transaction.Create(Doc);

            if (RefreshOnCommit)
            {
                UIDocument uIDocument = new UIDocument(Doc);
                uIDocument.RefreshActiveView();
            }
        }

    }



   

   

}
