using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    /// <summary>
    /// Class to get realtion between elements.
    /// </summary>
    /// <typeparam name="T">Type of base element.</typeparam>
    public abstract class NewElementRelationBuilder<T>
    {
        protected readonly T _baseElement;

        /// <summary>
        /// Initiate a new instance to get realtion between elements.
        /// </summary>
        /// <param name="baseElement">Base reference Element to get relations.</param>
        protected NewElementRelationBuilder(T baseElement)
        {
            _baseElement = baseElement;
        }

        /// <summary>
        /// Get relation between baseElement and input element.
        /// </summary>
        /// <param name="element">Element to check relation with baseElement</param>
        /// <returns>Returns relation of input element to baseElement.</returns>
        public abstract Relation GetRelation(Element element);
    }
}
