using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Solids.Models
{

    public class SolidModelExt : AbstractSolidModel
    {
        public SolidModelExt(Element element, Solid solid = null) : base(solid)
        {
            Element = element;
            Solid = ElementUtils.GetSolid(element);
            ConnectorsPoints = GetConnectorPoints();
            Length = ConnectorsPoints.First().DistanceTo(ConnectorsPoints.Last());
            CentralLine = Element.GetCenterLine();

            List<XYZ> normOrths = DS.RevitLib.Utils.Solids.SolidUtils.GetOrthoNormVectors(Solid, CentralLine);
            var MaxOrth = GetMaxSizeOrth(normOrths);
            MaxOrthLine = Line.CreateBound(CentralPoint, CentralPoint + MaxOrth);
        }

        public Element Element { get; private set; }
        public List<XYZ> ConnectorsPoints { get; private set; }

        /// <summary>
        /// Length between main connectors.
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Line between main elements's connectors
        /// </summary>
        public Line CentralLine { get; private set; }

        public XYZ CentralPoint
        {
            get
            {
                return (CentralLine.GetEndPoint(0) + CentralLine.GetEndPoint(1)) / 2;
            }
        }

        /// <summary>
        ///Orth vector with max solid's size.
        /// </summary>
        //public XYZ MaxOrth { get; private set; }

        public Line MaxOrthLine { get; private set; }

        private List<XYZ> GetConnectorPoints()
        {
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(Element);
            var list = new List<Connector>() { con1, con2 };
            return list.Select(c => c.Origin).ToList();
        }

        public void Transform(Transform transform)
        {
            //get transformed objects
            Solid tSolid = Autodesk.Revit.DB.SolidUtils.CreateTransformed(Solid, transform);
            Line tLine = CentralLine.CreateTransformed(transform) as Line;
            Line tMaxOrthLine = MaxOrthLine.CreateTransformed(transform) as Line;

            List<XYZ> tConnectorsPoints = new List<XYZ>();
            foreach (var point in ConnectorsPoints)
            {
                tConnectorsPoints.Add(transform.OfPoint(point));
            }

            Solid = tSolid;
            CentralLine = tLine;
            ConnectorsPoints = tConnectorsPoints;
            MaxOrthLine = tMaxOrthLine;
        }

        private XYZ GetMaxSizeOrth(List<XYZ> orths)
        {
            XYZ maxVector = null;
            double maxSize = 0;
            foreach (var orth in orths)
            {
                double size = GetSizeByVector(orth);
                if (size > maxSize)
                {
                    maxSize = size;
                    maxVector = orth;
                }
            }

            return maxVector;
        }

        public override AbstractSolidModel Clone()
        {
            return (AbstractSolidModel)this.MemberwiseClone();
        }
    }
}
