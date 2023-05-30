using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Various
{
    /// <inheritdoc/>
    public class ElementSelector : SelectorBase<Element>
    {
        /// <inheritdoc/>
        public ElementSelector(UIDocument uiDoc) : base(uiDoc)
        {
        }

        /// <inheritdoc/>
        protected override ISelectionFilter Filter => new ElementSelectionFilter<Element>() { AllowLink = AllowLink};

        /// <inheritdoc/>
        protected override ISelectionFilter FilterInLink => new ElementInLinkSelectionFilter<MEPCurve>(_doc);

        /// <inheritdoc/>
        public override Element Pick(string statusPrompt = null, string promptSuffix = null)
        {
            Reference reference = _uiDoc.Selection.
                PickObject(ObjectType.Element, Filter, GetStatusPrompt(statusPrompt, promptSuffix));
            var element = _doc.GetElement(reference);
            return element is RevitLinkInstance ? 
                PickInLink(element as RevitLinkInstance) : 
                element;
        }
    }
}
