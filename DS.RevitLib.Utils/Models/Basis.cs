using Autodesk.Revit.DB;

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

    }

}
