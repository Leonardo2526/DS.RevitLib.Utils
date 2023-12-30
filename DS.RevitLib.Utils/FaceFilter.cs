using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// An object that represents filters for <see cref="Autodesk.Revit.DB.Face"/>.
    /// </summary>
    public static class FaceFilter
    {
        private static readonly double _at = 1.DegToRad();

        /// <summary>
        /// Get <see cref="Predicate{T}"/> to filter <see cref="PlanarFace"/>s with normals parallel to <paramref name="wall"/>'s XBasis.
        /// </summary>
        /// <param name="wall"></param>
        /// <returns>
        /// <see cref="Predicate{T}"/> to filter <paramref name="wall"/>'s faces.
        /// <para>
        /// <see langword="null"/> if failed to get <paramref name="wall"/>'s basis.
        /// </para>
        /// </returns>
        public static Predicate<PlanarFace> XNormal(Wall wall)
            => !wall.TryGetBasis(out var basis) ? 
            null : 
            (f => f.FaceNormal.ToVector3d().IsParallelTo(basis.X, _at) != 0);

        /// <summary>
        /// Get <see cref="Predicate{T}"/> to filter <see cref="PlanarFace"/>s with normals parallel to <paramref name="wall"/>'s YBasis.
        /// </summary>
        /// <param name="wall"></param>
        /// <returns>
        /// <see cref="Predicate{T}"/> to filter <paramref name="wall"/>'s faces.
        /// <para>
        /// <see langword="null"/> if failed to get <paramref name="wall"/>'s basis.
        /// </para>
        /// </returns>
        public static Predicate<PlanarFace> YNormal(Wall wall)
            => !wall.TryGetBasis(out var basis) ?
            null :
            (f => f.FaceNormal.ToVector3d().IsParallelTo(basis.Y, _at) != 0);

        /// <summary>
        /// Get <see cref="Predicate{T}"/> to filter <see cref="PlanarFace"/>s with normals parallel to <paramref name="wall"/>'s ZBasis.
        /// </summary>
        /// <param name="wall"></param>
        /// <returns>
        /// <see cref="Predicate{T}"/> to filter <paramref name="wall"/>'s faces.
        /// <para>
        /// <see langword="null"/> if failed to get <paramref name="wall"/>'s basis.
        /// </para>
        /// </returns>
        public static Predicate<PlanarFace> ZNormal(Wall wall)
            => !wall.TryGetBasis(out var basis) ?
            null :
            (f => f.FaceNormal.ToVector3d().IsParallelTo(basis.Z, _at) != 0);

    }
}
