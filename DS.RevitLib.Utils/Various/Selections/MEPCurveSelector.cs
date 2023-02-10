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
    public class MEPCurveSelector : SelectorBase<MEPCurve>
    {
        /// <inheritdoc/>
        public MEPCurveSelector(UIDocument uiDoc) : base(uiDoc)
        {
        }

        /// <inheritdoc/>
        protected override ISelectionFilter Filter => new ElementSelectionFilter<MEPCurve>() { AllowLink = AllowLink};

        /// <inheritdoc/>
        protected override ISelectionFilter FilterInLink => new ElementInLinkSelectionFilter<MEPCurve>(_doc);

        /// <inheritdoc/>
        public override MEPCurve Pick(string statusPrompt = null, string promptSuffix = null)
        {
            Reference reference = _uiDoc.Selection.
                PickObject(ObjectType.Element, Filter, GetStatusPrompt(statusPrompt, promptSuffix));
            var element = _doc.GetElement(reference);
            return element is RevitLinkInstance ? 
                PickInLink(element as RevitLinkInstance) as MEPCurve : 
                element as MEPCurve;
        }

        /// <inheritdoc/>
        public override void Set(List<MEPCurve> elements)
        {
            _uiDoc.Selection.SetElementIds(elements.Select(obj => obj.Id).ToList());
        }
    }
}
