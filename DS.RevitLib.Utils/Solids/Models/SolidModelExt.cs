using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
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

            List<XYZ> tConnectorsPoints = new List<XYZ>();
            foreach (var point in ConnectorsPoints)
            {
                tConnectorsPoints.Add(transform.OfPoint(point));
            }

            Solid = tSolid;
            CentralLine = tLine;
            ConnectorsPoints = tConnectorsPoints;          
        }

    }
}
