using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Extensions;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods for vertices.
    /// </summary>
    public static class VertexExtensions
    {
        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.XYZ"/> point that specified <paramref name="vertex"/> location.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="doc"></param>
        /// <returns>
        ///  Convereted to <see cref="Autodesk.Revit.DB.XYZ"/> tag property of <paramref name="vertex"/> if it's <see cref="Point3d"/>.
        /// <para>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> built by tag property of <paramref name="vertex"/> if tag specified on 
        /// <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s <see cref="Autodesk.Revit.DB.ElementId"/>.
        /// </para>
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static XYZ GetLocation(this IVertex vertex, Document doc)
        {
            XYZ xYZPoint = null;
            switch (vertex)
            {
                case TaggedGVertex<Point3d> point:
                    { xYZPoint = point.Tag.ToXYZ(); }
                    break;
                case TaggedGVertex<int> famInsVertex:
                    { xYZPoint = GetLocation(famInsVertex.Tag); }
                    break;
                default: break;
            }

            return xYZPoint;

            XYZ GetLocation(int tag)
            {
                var famInst = doc.GetElement(new ElementId(tag)) as FamilyInstance;
                return famInst.GetLocation();
            }
        }
    }
}
