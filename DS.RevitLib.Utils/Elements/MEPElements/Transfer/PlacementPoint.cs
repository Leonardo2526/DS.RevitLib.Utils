using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Lines;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    public class PlacementPoint
    {
        private readonly Line _line;
        private readonly double _placementLength;
        private readonly XYZ _p1;
        private readonly XYZ _p2;

        public PlacementPoint(Line line, double placementLength)
        {
            _line = line;
            _placementLength = placementLength;
            _p1 = line.GetEndPoint(0);
            _p2 = line.GetEndPoint(1);
        }

        public XYZ GetPoint(XYZ basePoint)
        {
            XYZ placementDirection = GetPlacementDirection(basePoint);
            return basePoint + placementDirection.Multiply(_placementLength / 2);
        }

        public XYZ GetPoint(PlacementOption placementOption)
        {
            switch (placementOption)
            {
                case PlacementOption.Center:
                    return _line.GetCenter();
                case PlacementOption.Edge:
                    return GetPoint(_p1);
                default:
                    break;
            }

            return null;
        }

        private XYZ GetPlacementDirection(XYZ basePoint)
        {
            XYZ direction;

            direction = (_p1 - basePoint).Normalize();
            if (direction.IsZeroLength())
            {
                return (_p2 - basePoint).Normalize();
            }

            return direction;
        }


    }
}
