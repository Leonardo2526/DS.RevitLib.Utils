using Autodesk.Revit.DB;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Util methods for <see cref="GeometryObject"/>.
    /// </summary>
    public static class GeometryElementsUtils
    {
        /// <summary>
        ///  Creates a new circle as geometric <see cref="Arc"/> object based on <paramref name="centerPoint"/>.
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="normal">Normal vector to circle plane.</param>
        /// <param name="radius">Circle radius</param>
        /// <returns></returns>
        public static Arc CreateCircle(XYZ centerPoint, XYZ normal, double radius)
        {
            XYZ gen = XYZUtils.GenerateXYZ();
            XYZ xAxis = normal.CrossProduct(gen).Normalize();
            XYZ yAxis = normal.CrossProduct(xAxis).Normalize();
            return Arc.Create(centerPoint, radius, 0, 2 * Math.PI, xAxis, yAxis);
        }

        /// <summary>
        /// Show <see cref="Arc"/>.
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="doc"></param>
        /// <param name="transactionBuilder">Defaul builder to create <see cref="Autodesk.Revit.DB.Transaction"/>.</param>
        public static void Show(Arc arc, Document doc, AbstractTransactionBuilder transactionBuilder = null)
        {
            transactionBuilder ??= new TransactionBuilder(doc);
            transactionBuilder.Build(() =>
            {
                var creator = new ModelCurveCreator(doc);
                creator.Create(arc);
            }, "Show Arc");
        }

        /// <summary>
        /// Get all EdgeArrays from solid.
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="onlyPlanar"></param>
        /// <returns>Returns all EdgeArrays of <paramref name="faces"/>.</returns>
        public static List<EdgeArray> GetEdgeArrays(IEnumerable<Face> faces, bool onlyPlanar = false)
        {
            var edgeArrays = new List<EdgeArray>();
            foreach (Face face in faces)
            {
                if (onlyPlanar && face is not PlanarFace) { continue; }
                for (int i = 0; i < face.EdgeLoops.Size; i++)
                {
                    EdgeArray edgeArray = face.EdgeLoops.get_Item(i);
                    edgeArrays.Add(edgeArray);
                }
            }
            return edgeArrays;
        }

        /// <summary>
        /// Get all curves from solid.
        /// </summary>
        /// <param name="edgeArrays"></param>
        /// <returns>Returns all curves from edges of <paramref name="edgeArrays"/>.</returns>
        public static List<Curve> GetCurves(IEnumerable<EdgeArray> edgeArrays)
        {
            var curves = new List<Curve>();
            foreach (EdgeArray edgeArray in edgeArrays)
            {
                for (int i = 0; i < edgeArray.Size; i++)
                {
                    Edge edge = edgeArray.get_Item(i);
                    var curve = edge.AsCurve();
                    curves.Add(curve);
                }
            }

            return curves;
        }
    }
}
