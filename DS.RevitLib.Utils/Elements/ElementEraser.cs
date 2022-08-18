using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.MEP
{
    public class ElementEraser
    {
        Document Doc;

        public ElementEraser(Document doc)
        {
            Doc = doc;
        }

        public void DeleteElement(Element element)
        {
            ICollection<ElementId> selectedIds = new List<ElementId>
            {
                element.Id
            };

            using (Transaction transNew = new Transaction(Doc, "DeleteElement"))
            {
                try
                {
                    transNew.Start();
                    Doc.Delete(selectedIds);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }


        }
    }
}
