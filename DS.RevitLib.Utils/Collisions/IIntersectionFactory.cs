using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Collisons;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public interface IElementIntersectionFactory : IIntersectionFactory<Element>
    {
        /// <summary>
        /// Elements to exclude intersections.
        /// </summary>
        List<Element> ExludedElements { get; set; }

        /// <summary>
        /// Build factory.
        /// </summary>
        /// <param name="checkModel2"></param>
        /// <returns>
        /// Factory that is ready to get inersections.
        /// </returns>
        IElementIntersectionFactory Build((Document, List<Element>) checkModel2);

        /// <summary>
        /// Get collisions of <paramref name="checkObject"/>.
        /// </summary>
        List<Element> GetIntersections(Element checkObject);

        /// <summary>
        /// Get collisions of <paramref name="checkObject"/>.
        /// </summary>
        List<Element> GetIntersections(Solid checkObject);
    }
}