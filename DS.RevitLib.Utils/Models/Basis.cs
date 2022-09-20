using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Models
{
    public class Basis
    {
        /// <summary>
        /// Create basis of three vectors from basePoint.
        /// </summary>
        /// <param name="basisX"></param>
        /// <param name="basisY"></param>
        /// <param name="basisZ"></param>
        /// <param name="basePoint"></param>
        public Basis(XYZ basisX, XYZ basisY, XYZ basisZ, XYZ basePoint)
        {
            X = basisX;
            Y = basisY;
            Z = basisZ;
            Point = basePoint;
        }

        /// <summary>
        /// First vector
        /// </summary>
        public XYZ X { get; private set; }
        /// <summary>
        /// Second vector
        /// </summary>
        public XYZ Y { get; private set; }
        /// <summary>
        /// Third vector
        /// </summary>
        public XYZ Z { get; private set; }

        /// <summary>
        /// Central point of basis
        /// </summary>
        public XYZ Point { get; private set; }



        #region PublicMethods

        /// <summary>
        /// Transform current Basis.
        /// </summary>
        /// <param name="transform"></param>
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

        /// <summary>
        /// Transform current Basis by list of transforms.
        /// </summary>
        /// <param name="transforms"></param>
        public void Transform(List<Transform> transforms)
        {
            foreach (var tr in transforms)
            {
                Transform(tr);
            }
        }


        /// <summary>
        /// Create shallow copy of current instance.
        /// </summary>
        /// <returns></returns>
        public Basis Clone()
        {
            return (Basis)this.MemberwiseClone();
        }

        /// <summary>
        /// Round all properties of current instance.
        /// </summary>
        /// <param name="i"></param>
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

        /// <summary>
        /// Show 
        /// </summary>
        /// <param name="doc"></param>
        public void Show(Document doc)
        {
            var creator = new ModelCurveCreator(doc);
            creator.Create(Point, Point + X.Multiply(3));
            creator.Create(Point, Point + Y.Multiply(2));
            creator.Create(Point, Point + Z);
        }

        #endregion


    }

}
