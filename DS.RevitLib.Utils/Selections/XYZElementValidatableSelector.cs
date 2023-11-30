using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Various.Selections
{
    /// <summary>
    /// The object is used to select 
    /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) in <see cref="Document"/>
    /// and validate them.
    /// </summary>
    public class XYZElementValidatableSelector : XYZElementSelectorBase, IValidatableSelector<(Element, XYZ)>
    {
        private int _index;

        /// <inheritdoc/>
        public XYZElementValidatableSelector(UIDocument uiDoc) : base(uiDoc)
        {
        }

        /// <inheritdoc/>
        public IEnumerable<IValidator<(Element, XYZ)>> Validators { get; set; } =
            new List<IValidator<(Element, XYZ)>>();

        /// <inheritdoc/>
        public (Element, XYZ) Select()
        {
            (Element element, XYZ point) pointElement;
            _index++;
            try
            {
                pointElement = SelectElement(_index);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                pointElement = SelectPointOnElement(_index);
            }

            if (pointElement.element == null) { return (null, null); }

            return 
                Validators.ToList().TrueForAll(v => v.IsValid(pointElement)) ? 
                pointElement : 
                (null, null);
        }
    }
}
