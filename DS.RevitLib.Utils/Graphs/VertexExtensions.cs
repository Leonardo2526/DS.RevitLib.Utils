﻿using Autodesk.Revit.DB;
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

        /// <summary>
        /// Try get <see cref="Autodesk.Revit.DB.FamilyInstance"/> from <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.FamilyInstance"/> if <paramref name="vertex"/> is <see cref="TaggedGVertex{TTag}"/>.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static FamilyInstance TryGetFamilyInstance(this IVertex vertex, Document doc)
        {
            var tag = vertex is TaggedGVertex<int> tagged ?
                tagged.Tag :
                -1;
            return tag == -1 ?
                    null :
                    doc.GetElement(new ElementId(tag)) as FamilyInstance;
        }

        /// <summary>
        /// Specifies if <paramref name="types"/> contains <paramref name="vertex"/>'s <see cref="Autodesk.Revit.DB.FamilyInstance"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="types"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="vertex"/>'s type is <see cref="Autodesk.Revit.DB.FamilyInstance"/>
        /// and <paramref name="types"/> contains it.
        /// <para>     
        /// Otherwise <see langword="false"/>.</para>
        /// </returns>
        public static bool ContainsTypes(this IVertex vertex, IEnumerable<Type> types, Document doc)
        {
            if(types is null || types.Count() == 0 ) return false;

            var famInst = vertex.TryGetFamilyInstance(doc);
            if(famInst == null) { return  false; }

            var type = famInst.GetType();
            return types.Contains(type);
        }


        /// <summary>
        /// Specifies if <paramref name="categories"/> contains <paramref name="vertex"/>'s <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="categories"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="vertex"/>'s type is <see cref="Autodesk.Revit.DB.FamilyInstance"/>
        /// and <paramref name="categories"/> contains <paramref name="vertex"/>'s <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.
        /// <para>     
        /// Otherwise <see langword="false"/>.</para>
        /// </returns>
        public static bool ContainsCategories(this IVertex vertex, Dictionary<BuiltInCategory, List<PartType>> categories, Document doc)
        {
            if (categories is null || categories.Count() == 0) return false;

            var famInst = vertex.TryGetFamilyInstance(doc);
            if (famInst == null) { return false; }

            return famInst.IsCategoryElement(categories);
        }
    }
}
