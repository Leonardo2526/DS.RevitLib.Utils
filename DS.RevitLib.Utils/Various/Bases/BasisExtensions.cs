using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Various.Bases
{
    /// <summary>
    /// An object that represents extension methods for some basis.
    /// </summary>
    public static class BasisExtensions
    {
        /// <summary>
        /// Convert <paramref name="basis"/> to <see cref="BasisXYZ"/>.
        /// </summary>
        /// <param name="basis"></param>
        /// <returns>
        /// New <see cref="BasisXYZ"/> built by <paramref name="basis"/> coordinates.
        /// </returns>
        public static BasisXYZ ToXYZ(this Basis3d basis) =>
            new BasisXYZ(basis.Origin.ToXYZ(), basis.X.ToXYZ(), basis.Y.ToXYZ(), basis.Z.ToXYZ());

        /// <summary>
        /// Convert <paramref name="basis"/> to <see cref="Basis3d"/>.
        /// </summary>
        /// <param name="basis"></param>
        /// <returns>
        /// New <see cref="Basis3d"/> built by <paramref name="basis"/> coordinates.
        /// </returns>
        public static Basis3d ToBasis3d(this BasisXYZ basis) =>
            new Basis3d(basis.Origin.ToPoint3d(), basis.X.ToVector3d(), basis.Y.ToVector3d(), basis.Z.ToVector3d());

        /// <summary>
        /// Show <paramref name="basis"/> in Revit model.
        /// </summary>
        /// <param name="basis"></param>
        /// <param name="uiDoc"></param>
        /// <param name="labelSize"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="refresh"></param>
        public static void Show(this Basis3d basis, UIDocument uiDoc, double labelSize = 0,
            ITransactionFactory transactionFactory = null, bool refresh = false)
        {
            var point3dVisualisator =
             new Point3dVisualisator(uiDoc, null, labelSize, transactionFactory, refresh);

            var origin = basis.Origin;

            point3dVisualisator.LabelSize = labelSize == 0 ? 150.MMToFeet() : labelSize;
            point3dVisualisator.ShowVector(origin, origin + basis.X);

            point3dVisualisator.LabelSize = labelSize == 0 ? 100.MMToFeet() : labelSize;
            point3dVisualisator.ShowVector(origin, origin + basis.Y);

            point3dVisualisator.LabelSize = labelSize == 0 ? 50.MMToFeet() : labelSize;
            point3dVisualisator.ShowVector(origin, origin + basis.Z);
        }

        /// <summary>
        /// Computes a change of basis transformation. A basis change is essentially a remapping of geometry from one coordinate system to another.
        /// </summary>
        /// <param name="sourceBasis"></param>
        /// <param name="targetBasis"></param>
        /// <returns>
        /// An ordered sequence of <see cref="Transform"/>'s to change <paramref name="sourceBasis"/> to <paramref name="targetBasis"/>.
        /// </returns>
        public static List<Transform> GetTransforms(this Basis3d sourceBasis, Basis3d targetBasis)
        {
            sourceBasis = sourceBasis.Round();
            targetBasis = targetBasis.Round();

            sourceBasis = sourceBasis.ToRightandedOrthonormal();
            targetBasis = targetBasis.ToRightandedOrthonormal();

            Rhino.Geometry.Transform transform = sourceBasis.GetTransform(targetBasis);
            if (!transform.GetEulerZYZ(out double alpha1, out double beta1, out double gamma1))
            { return new List<Transform>();}

            double alpha = alpha1.RadToDeg();
            double beta = beta1.RadToDeg();
            double gamma = gamma1.RadToDeg();

            if (alpha == double.NegativeInfinity)
            { throw new ArgumentException("Failed to transform sourceBasis to targetBasis."); }

            BasisXYZ basisXYZ = sourceBasis.ToXYZ();

            var xYZTransforms = new List<Transform>
            {
                Transform.CreateRotationAtPoint(basisXYZ.Z, -alpha1, basisXYZ.Origin),
                Transform.CreateRotationAtPoint(basisXYZ.Y, -beta1, basisXYZ.Origin),
                Transform.CreateRotationAtPoint(basisXYZ.Z, -gamma1, basisXYZ.Origin)
            };
            var translation = Transform.CreateTranslation((targetBasis.Origin.ToXYZ() - basisXYZ.Origin));
            xYZTransforms.Add(translation);

            return xYZTransforms;
        }
    }
}
