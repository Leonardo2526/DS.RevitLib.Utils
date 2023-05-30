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

        /// <summary>
        /// Get basis vector from <paramref name="basis"/> on <paramref name="plane"/>.
        /// </summary>
        /// <param name="basis"></param>
        /// <param name="plane"></param>
        /// <param name="projAvailable">Specifies whether projection on <paramref name="plane"/> is available if no occurent was found.</param>
        /// <returns>Returns first occurence of basis vector from <paramref name="basis"/> on <paramref name="plane"/>.
        /// <para>Returns <see langword="null"/> if no occurence was found.</para>
        /// </returns>
        public XYZ GetBasisVectorOnPlane(Plane plane, bool projAvailable = false)
        {
            var basisVectorInPlane = GetBasisVectorOntoPlane(this, plane);
            if(basisVectorInPlane != null) { return basisVectorInPlane; }

            if (projAvailable)
            {
                //create new projection basis
                var xProj = plane.ProjectOnto(this.X);
                var yProj = plane.ProjectOnto(this.Y);
                var zProj = plane.ProjectOnto(this.Z);
                var oProj = plane.ProjectOnto(this.Point);
                var projBasis = new Basis(xProj, yProj, zProj, oProj);

                basisVectorInPlane = GetBasisVectorOntoPlane(projBasis, plane);
                if (basisVectorInPlane != null) { return basisVectorInPlane; }
            }

            return null;
        }

        /// <summary>
        /// Get basis vector from <paramref name="basis"/> on <paramref name="plane"/>.
        /// </summary>
        /// <param name="basis"></param>
        /// <param name="plane"></param>
        /// <returns>Returns first occurence of basis vector from <paramref name="basis"/> on <paramref name="plane"/>.</returns>
        private XYZ GetBasisVectorOntoPlane(Basis basis, Plane plane)
        {
            if(basis.X.IsZeroLength() || basis.Y.IsZeroLength() || basis.Z.IsZeroLength())
            { return null; }

            var v1 = plane.XVec; var v2 =plane.YVec;

            if (XYZUtils.Coplanarity(basis.X, v1, v2)) { return basis.X; }
            if (XYZUtils.Coplanarity(basis.Y, v1, v2)) { return basis.Y; }
            if (XYZUtils.Coplanarity(basis.Z, v1, v2)) { return basis.Z; }
            return null;
        }

        #endregion


    }

}
