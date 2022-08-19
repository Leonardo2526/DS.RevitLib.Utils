using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.Alignments
{
    internal interface IAlignment
    {
        public Element OperationElement { get; }
        public Element TargetElement { get; }

        /// <summary>
        /// Align OperationElement and TargetElement.
        /// </summary>
        /// <returns>Returns aligned operation element.</returns>
        public Element Align();

    }
}
