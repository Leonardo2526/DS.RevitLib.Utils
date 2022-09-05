using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;

namespace DS.RevitLib.Utils.Models
{
    public class Basis
    {
        public Basis(XYZ basisX, XYZ basisY, XYZ basisZ, XYZ basePoint)
        {
            X = basisX;
            Y = basisY;
            Z = basisZ;
            Point = basePoint;
        }

        public XYZ X { get; private set; }
        public XYZ Y { get; private set; }
        public XYZ Z { get; private set; }

        /// <summary>
        /// Central point of basis
        /// </summary>
        public XYZ Point { get; private set; }



        #region PublicMethods
        public void Transform(Transform transform)
        {
            if (transform.IsTranslation)
            {
                Point = transform.OfPoint(Point);
            }
            else
            {
                X = transform.OfVector(X);
                Y = transform.OfVector(Y);
                Z = transform.OfVector(Z);
            }
        }

        public Basis Clone()
        {
            return (Basis)this.MemberwiseClone();
        }

        public void Round(int i = 3)
        {
            X = X.RoundVector(i);
            Y = Y.RoundVector(i);
            Z = Z.RoundVector(i);
            Point = Point.RoundVector(i);
        }

        /// <summary>
        /// Check if basis is orthogonal
        /// </summary>
        public bool IsOrthogonal(int roundValue = 2)
        {
            double scalarXY = X.DotProduct(Y);
            double scalarXZ = X.DotProduct(Z);
            double scalarYZ = Y.DotProduct(Z);
            if (Math.Round(scalarXY, roundValue) == 0 &&
                Math.Round(scalarXZ, roundValue) == 0 &&
                Math.Round(scalarYZ, roundValue) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get orientation of basis.
        /// </summary>
        /// <returns></returns>
        public BasisOrientation GetOrientaion()
        {
            double det = X.TripleProduct(Y, Z);

            if (det > 0)
            {
                return BasisOrientation.Right;
            }

            return BasisOrientation.Left;
        }

        #endregion


    }

}
