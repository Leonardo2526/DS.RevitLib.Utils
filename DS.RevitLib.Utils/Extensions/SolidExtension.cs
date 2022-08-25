using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    public static class SolidExtension
    {
        /// <summary>
        /// Get the transformed solid from the coordinate space of the box to the model coordinate space.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Solid TransformToModelSpace(this Solid solid)
        {
            Transform transform = solid.GetBoundingBox().Transform;
            return SolidUtils.CreateTransformed(solid, transform);
        }
    }
}
