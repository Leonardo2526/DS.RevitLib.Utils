using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PickedElement : IElement
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;

        public PickedElement(UIDocument uidoc, Document doc)
        {
            Uidoc = uidoc;
            Doc = doc;
        }

        public Element GetElement()
        {
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element,
               "Select element that will be checked for intersection with all elements");
            return Doc.GetElement(reference);
        }
    }
}
