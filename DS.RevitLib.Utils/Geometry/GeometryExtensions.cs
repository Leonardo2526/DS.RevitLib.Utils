﻿using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    /// Extensions methods for geomtry primitives.
    /// </summary>
    public static class GeometryExtensions
    {
        /// <summary>
        /// Convert <paramref name="rectangle"/> to list of <see cref="Autodesk.Revit.DB.Line"/>.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns>
        /// <paramref name="rectangle"/> segments converted to list of <see cref="Autodesk.Revit.DB.Line"/>.
        /// </returns>
        public static List<Autodesk.Revit.DB.Line> ToRevitLines(this Rectangle3d rectangle)
        {
            var rlines = new List<Autodesk.Revit.DB.Line>();
            var rgLines = rectangle.ToLines();
            rgLines.ForEach(l => rlines.Add(Autodesk.Revit.DB.Line.CreateBound(l.From.ToXYZ(), l.To.ToXYZ())));
            return rlines;
        }

        /// <summary>
        /// Show <paramref name="rectangle"/> in <paramref name="doc"/>.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="doc"></param>
        public static void Show(this Rectangle3d rectangle, Document doc)
        {
            var rlines = ToRevitLines(rectangle);
            rlines.ForEach(obj => obj.Show(doc));
        }
    }
}
