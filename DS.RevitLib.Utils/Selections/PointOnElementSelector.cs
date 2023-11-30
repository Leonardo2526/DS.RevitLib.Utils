using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Various.Selections
{
    /// <inheritdoc/>
    public class PointOnElementSelector : SelectorBase<Element>
    {
        /// <inheritdoc/>
        public PointOnElementSelector(UIDocument uiDoc) : base(uiDoc)
        {
        }

        /// <inheritdoc/>
        public XYZ Point { get; private set; }

        /// <summary>
        /// Element to pick points.
        /// </summary>
        public Element SelectionElement { get; set; }

        /// <inheritdoc/>
        protected override ISelectionFilter Filter => new PointOnElementSelectionFilter(SelectionElement);

        /// <inheritdoc/>
        protected override ISelectionFilter FilterInLink => new ElementInLinkSelectionFilter<Element>(_doc);

        /// <inheritdoc/>
        public override Element Pick(string statusPrompt = null, string promptSuffix = null)
        {
            Element element;
            try
            {
                Reference reference = _uiDoc.Selection.
                    PickObject(ObjectType.PointOnElement, Filter, GetStatusPrompt(statusPrompt, promptSuffix));
                Point = reference.GlobalPoint;
                element = _doc.GetElement(reference);
            }
            catch (Exception)
            {return null;}

            return element is RevitLinkInstance ?
                PickInLink(element as RevitLinkInstance) :
                element;
        }
    }
}
