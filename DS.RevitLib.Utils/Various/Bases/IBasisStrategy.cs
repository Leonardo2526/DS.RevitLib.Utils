using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DS.RevitLib.Utils.Bases
{
    /// <summary>
    /// The interface is used to get basis vectors.
    /// </summary>
    public interface IBasisStrategy
    {
        /// <summary>
        /// First basis.
        /// </summary>
        XYZ BasisX { get; }

        /// <summary>
        /// Second basis.
        /// </summary>
        XYZ BasisY { get; }

        /// <summary>
        /// Third basis.
        /// </summary>
        XYZ BasisZ { get; }

        /// <summary>
        /// Central point of basis
        /// </summary>
        public XYZ Point { get; }

        /// <summary>
        /// Build strategy.
        /// </summary>
        /// <returns></returns>
        /// <param name="uiDoc"></param>
        IBasisStrategy Build(UIDocument uiDoc);

        /// <summary>
        /// Get basis vectors.
        /// </summary>
        /// <returns>
        /// Three <see cref="Autodesk.Revit.DB.XYZ"/> basis vectors.
        /// </returns>
        (XYZ basisX, XYZ basisY, XYZ basisZ) GetBasis();

      /// <summary>
      /// Set basis.
      /// </summary>
      /// <param name="basisX"></param>
      /// <param name="basisY"></param>
      /// <param name="basisZ"></param>
      /// <returns></returns>
        (XYZ basisX, XYZ basisY, XYZ basisZ) SetBasis(XYZ basisX, XYZ basisY, XYZ basisZ);
    }
}
