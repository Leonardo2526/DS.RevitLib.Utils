using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace DS.RevitLib.Utils.SelectionFilters
{
    /// <summary>
    /// Class for filer creation in link to select only elements with geometry.
    /// </summary> 
    public class ElementInLinkSelectionFilter<T> : ISelectionFilter where T : Element
    {
        private Document IntDoc;
        private bool IntLastCheckedWasFromLink = false;

        private Document IntLinkDoc = null;

        /// <summary>
        /// Instantiate an object for filer creation in link to select only elements with geometry.
        /// </summary>
        public ElementInLinkSelectionFilter(Document Doc)
        {
            IntDoc = Doc;
        }

        public Document LinkDocument
        {
            get { return IntLinkDoc; }
        }
        public bool LastCheckedWasFromLink
        {
            get { return IntLastCheckedWasFromLink; }
        }

        public bool AllowElement(Element elem)
        {
            return true;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            Element El = IntDoc.GetElement(reference);
            RevitLinkInstance RvtLnkInst = El as RevitLinkInstance;
            if (RvtLnkInst == null == false)
            {
                IntLastCheckedWasFromLink = true;

                Document LnkDoc = RvtLnkInst.GetLinkDocument();

                T ClsInst = LnkDoc.GetElement(reference.LinkedElementId) as T;
                if (ClsInst == null == false)
                {
                    var type = ClsInst.GetType().ToString();
                    if (type.Contains("Insulation") || type.Contains("Изоляция")) { return false; }
                    IntLinkDoc = RvtLnkInst.GetLinkDocument();
                    return true;
                }
            }
            else
            {
                IntLastCheckedWasFromLink = false;
                IntLinkDoc = null;
                T ClsInst = El as T;
                if (ClsInst == null == false)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
