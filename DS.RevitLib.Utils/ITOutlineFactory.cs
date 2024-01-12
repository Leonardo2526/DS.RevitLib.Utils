using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Connections.PointModels;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    /// The interface is used to create <see cref="Autodesk.Revit.DB.Outline"/> from <typeparamref name="TItem"/>. 
    /// </summary>
    public interface ITOutlineFactory<TItem>
    {
        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Outline"/> of <paramref name="item"/> built by it's bounding box.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> of <paramref name="item"/>.       
        /// </returns>      
        Outline Create(TItem item);
    }
}