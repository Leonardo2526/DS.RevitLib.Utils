using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Solids.Models
{
    public abstract class AbstractSolidModel
    {

        public AbstractSolidModel(Solid solid = null)
        {
            Solid = solid ?? null;
        }

        public Solid Solid { get; protected set; }

        /// <summary>
        /// Centroid of the Solid
        /// </summary>
        public XYZ Center
        {
            get
            {
                return Solid.ComputeCentroid();
            }
        }

        public double GetSizeByVector(XYZ orth, XYZ solidCentroid)
        {
            return DS.RevitLib.Utils.Solids.SolidUtils.GetSizeByVector(Solid, orth, solidCentroid);
        }
    }
}
