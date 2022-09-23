using Autodesk.Revit.DB;
using System;

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

        public double GetSizeByVector(XYZ orth)
        {
            return DS.RevitLib.Utils.Solids.SolidUtils.GetSizeByVector(Solid, orth);
        }
    }
}
