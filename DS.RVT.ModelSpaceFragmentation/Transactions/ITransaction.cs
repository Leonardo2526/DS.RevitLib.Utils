using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.ModelSpaceFragmentation
{
    interface ITransaction
    {
        void Create(Document doc);
    }
}
