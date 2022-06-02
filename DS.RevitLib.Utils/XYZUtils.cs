using Autodesk.Revit.DB;
using DS.MainUtils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
    public static class XYZUtils
    {
        #region PublicMethods

        /// <summary>
        /// Check if three vectors system have orientation (left or right) like origin
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>Return true if three vectors system have orientation like origin.</returns>
        public static bool BasisEqualToOrigin(XYZ a, XYZ b, XYZ c)
        {
            a = a.RoundVector();
            b = b.RoundVector();
            c = c.RoundVector();

            double[,] matrix = CreateMatrix3D(a, b, c);
            double det = Matrix.GetMatrixDeterminant(matrix);

            if (det > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if two vectors are colliner.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Return true if two vectors are colliner.</returns>
        public static bool Collinearity(XYZ a, XYZ b)
        {
            a = a.RoundVector();
            b = b.RoundVector();

            XYZ crossProduct = a.CrossProduct(b);
            if (Math.Round(crossProduct.GetLength(), 2) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if three vectors are coplanar.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Return true if three vectors are coplanar.</returns>
        public static bool Coplanarity(XYZ a, XYZ b, XYZ c)
        {
            a = a.RoundVector();
            b = b.RoundVector();
            c = c.RoundVector();
            if (Math.Round(a.TripleProduct(b, c), 2) == 0)
            {
                return true;
            }
            return false;
        }

        #endregion


        #region PrivateMethods

        /// <summary>
        /// Get matrix's determinant created by 3 vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <returns>Return matrix determinant value</returns>
        private static double GetDeterminantByVectors(XYZ vector1, XYZ vector2, XYZ vector3)
        {
            double[,] matrix = CreateMatrix3D(vector1, vector2, vector3);
            return Matrix.GetMatrixDeterminant(matrix);
        }

        /// <summary>
        /// Create 3D matrix by 3 vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <returns>Return 3D matrix.</returns>
        private static double[,] CreateMatrix3D(XYZ vector1, XYZ vector2, XYZ vector3)
        {
            List<XYZ> result = new List<XYZ>()
            {
                vector1, vector2, vector3
            };

            double[,] matrix = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                matrix[i, 0] = result[i].X;
                matrix[i, 1] = result[i].Y;
                matrix[i, 2] = result[i].Z;
            }

            return matrix;
        }


        #endregion
    }
}
