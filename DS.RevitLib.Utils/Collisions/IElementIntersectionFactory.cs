using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Collisons;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public interface IElementIntersectionFactory : IIntersectionFactory<Element>, IExclusion
    {
        /// <summary>
        /// Specifies whether allow insulation intersections.
        /// </summary>
        bool IsInsulationAccount { get; }

        /// <summary>
        /// Build factory to get intersections with active <see cref="Document"/> elements.
        /// </summary>
        /// <param name="checkModel2"></param>
        /// <returns>
        /// Factory that is ready to get inersections.
        /// </returns>
        IElementIntersectionFactory Build((Document, List<Element>) checkModel2);

        /// <summary>
        /// Build factory to get intersections with <see cref="RevitLinkInstance"/> elements.
        /// </summary>
        /// <param name="checkModel2"></param>
        /// <returns>
        /// Factory that is ready to get inersections.
        /// </returns>
        IElementIntersectionFactory Build((RevitLinkInstance, Transform, List<Element>) checkModel2);

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