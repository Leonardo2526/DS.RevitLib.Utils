using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Various.Selections;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.GraphUtils.Entities;
using DS.ClassLib.VarUtils;
using Autodesk.Revit.UI;
using System.Reflection;
using DS.RevitLib.Utils.Extensions;
using Rhino.Geometry;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The object is used to select <see cref="IVertex"/> in <see cref="Document"/> and validate it.
    /// </summary>
    public class VertexValidatableSelector : XYZElementSelectorBase, IValidatableSelector<IVertex>
    {
        private int _index;

        /// <summary>
        /// Instansiate the object to select <see cref="IVertex"/> in <see cref="Document"/> and validate it.
        /// </summary>
        public VertexValidatableSelector(UIDocument uiDoc) : base(uiDoc)
        {
        }

        /// <inheritdoc/>
        public IEnumerable<IValidator<IVertex>> Validators { get; set; } = new List<IValidator<IVertex>>();

        /// <inheritdoc/>
        public IVertex Select()
        {
            _index++;

            (Element element, XYZ point) pointElement;
            try
            {
                pointElement = SelectElement(_index);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                pointElement = SelectPointOnElement(_index);
            }

            if (pointElement.element == null) { return null; }


            var vertex = pointElement.ToVertex(_index);

            return
                Validators.ToList().TrueForAll(v => v.IsValid(vertex)) ?
                vertex :
                null;
        }
    }
}
