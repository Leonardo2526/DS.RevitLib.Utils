using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Various.Selections
{
    /// <inheritdoc/>
    public class VertexSelectors : XYZElementSelectors
    {
        /// <inheritdoc/>
        public VertexSelectors(UIDocument uiDoc) : base(uiDoc)
        {
        }

        /// <summary>
        /// Select <see cref="IVertex"/> on <see cref="Autodesk.Revit.DB.Element"/>.
        /// </summary>
        /// <returns></returns>
        public IVertex SelectVertexOnElement() => SelectElement().ToVertex(-1);

        /// <summary>
        /// Select <see cref="IVertex"/> on <see cref="Autodesk.Revit.DB.XYZ"/>.
        /// </summary>
        /// <returns></returns>
        public IVertex SelectVertexOnElementPoint() => SelectPointOnElement().ToVertex(-1);
    }
}