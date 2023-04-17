using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Face"/>.
    /// </summary>
    /// 
    public static class FaceExtensions
    {
        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Face"/> from <paramref name="faceArray"/> by <paramref name="point"/> on it.
        /// </summary>
        /// <param name="faceArray"></param>
        /// <param name="point"></param>
        /// <returns>Returns first <see cref="Autodesk.Revit.DB.Face"/> from <paramref name="faceArray"/> if it contains <paramref name="point"/>.
        /// <para>
        /// Otherwise returns <see langword="null"></see>.
        /// </para>
        /// </returns>
        public static Face GetFace(this FaceArray faceArray, XYZ point)
        {
            for (int i = 0; i < faceArray.Size; i++)
            {
                Face face = faceArray.get_Item(i);
                var projPoint = face.Project(point).XYZPoint;
                if ((projPoint - point).IsZeroLength())
                {
                    return face;
                }
            }
            return null;
        }
    }
}
