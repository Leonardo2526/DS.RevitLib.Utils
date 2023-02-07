using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override Element Select(string statusPrompt = null, string promptSuffix = null)
        {           
            Reference reference = _uiDoc.Selection.
                PickObject(ObjectType.Element, GetStatusPrompt(statusPrompt, promptSuffix));
            return _doc.GetElement(reference);
        }
    }
}
