using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions
{
    public interface IExclusion
    {
        /// <summary>
        /// Categories to exclude.
        /// </summary>
        List<BuiltInCategory> ExculdedCategories { get; set; }

        /// <summary>
        /// Types to exclude.
        /// </summary>
        public List<Type> ExculdedTypes { get; set; } 

        /// <summary>
        /// Elements to exclude.
        /// </summary>
        public List<Element> ExcludedElements { get; set; }

        /// <summary>
        /// <see cref="Autodesk.Revit.DB.ElementId"/>s to exclude.
        /// </summary>
        public IEnumerable<ElementId> ExcludedIds { get; set; }
    }
}
