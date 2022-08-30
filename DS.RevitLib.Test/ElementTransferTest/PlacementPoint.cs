using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;

namespace DS.RevitLib.Test
{
    public class PlacementPoint
    {
        private readonly MEPCurve _mEPCurve;
        private readonly double _placementLength;
        private readonly Connector _con1;
        private readonly Connector _con2;

        public PlacementPoint(MEPCurve mEPCurve, double placementLength)
        {
            _mEPCurve = mEPCurve;
            _placementLength = placementLength;
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(mEPCurve);
            _con1 = con1;
            _con2 = con2;
        }

        public XYZ GetPoint(Connector baseConnector)
        {
            XYZ placementDirection = GetPlacementDirection(baseConnector);
            return baseConnector.Origin + placementDirection.Multiply(_placementLength/ 2);
        }

        public XYZ GetPoint(PlacementOption placementOption)
        {
            switch (placementOption)
            {
                case PlacementOption.Center:                  
                    return ElementUtils.GetLocationPoint(_mEPCurve);
                case PlacementOption.Edge:
                    return GetPoint(_con1);
                default:
                    break;
            }

            return null;
        }

        private XYZ GetPlacementDirection(Connector baseCon)
        {
            XYZ direction;

            direction = (_con1.Origin - baseCon.Origin).Normalize();
            if (direction.IsZeroLength())
            {
                return (_con2.Origin - baseCon.Origin).Normalize();
            }

            return direction;
        }

       
    }
}
