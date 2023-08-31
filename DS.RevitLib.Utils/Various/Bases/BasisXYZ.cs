using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Basis;

namespace DS.RevitLib.Utils.Various.Bases
{
    /// <summary>
    /// An object that represents basis of <see cref="Autodesk.Revit.DB.XYZ"/>'s.
    /// </summary>
    public struct BasisXYZ : IBasis<XYZ>
    {
        /// <summary>
        /// Instansiate an object that represents basis of <see cref="Autodesk.Revit.DB.XYZ"/>'s.
        /// </summary>
        /// <param name="basisX"></param>
        /// <param name="basisY"></param>
        /// <param name="basisZ"></param>
        public BasisXYZ(XYZ basisX, XYZ basisY, XYZ basisZ)
        {
            X = basisX; Y = basisY; Z = basisZ;
        }

        /// <summary>
        /// Instansiate an object that represents basis of <see cref="Autodesk.Revit.DB.XYZ"/>'s.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="basisX"></param>
        /// <param name="basisY"></param>
        /// <param name="basisZ"></param>
        public BasisXYZ(XYZ origin, XYZ basisX, XYZ basisY, XYZ basisZ)
        {
            Origin = origin; X = basisX; Y = basisY; Z = basisZ;
        }

        /// <inheritdoc/>
        public XYZ X { get; set; }

        /// <inheritdoc/>
        public XYZ Y { get; set; }

        /// <inheritdoc/>
        public XYZ Z { get; set; }

        /// <summary>
        /// Basis origin point.
        /// </summary>
        public XYZ Origin { get; set; }
    }
}
