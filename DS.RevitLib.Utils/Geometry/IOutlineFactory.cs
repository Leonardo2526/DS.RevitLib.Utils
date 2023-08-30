using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    /// The interface is used to create <see cref="Autodesk.Revit.DB.Outline"/>. 
    /// </summary>
    public interface IOutlineFactory
    {
        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Outline"/> by manual points picking.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> if points picking was successful.
        /// <para>
        /// Otherwise returns <see langword="null"/>.
        /// </para>
        /// </returns>
        Outline CreateManual(UIDocument uiDoc);

        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Outline"/> by <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> if <paramref name="point"/> is valid.
        /// <para>
        /// Otherwise returns <see langword="null"/>.
        /// </para>
        /// </returns>     
        Outline Create(XYZ point);

        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Outline"/> by <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> if <paramref name="startPoint"/> and <paramref name="endPoint"/> are valid.
        /// <para>
        /// Otherwise returns <see langword="null"/>.
        /// </para>
        /// </returns>      
        Outline Create(XYZ startPoint, XYZ endPoint);
    }
}