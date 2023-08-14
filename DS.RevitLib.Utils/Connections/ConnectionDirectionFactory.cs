using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various.Selections;

namespace DS.RevitLib.Utils.Connections
{
    /// <summary>
    /// The interface is used to create factory that produce direction at connection point.
    /// </summary>
    internal class ConnectionDirectionFactory
    {
        private readonly XYZ _connectionPoint;
        private readonly Element _connectionElement;
        private readonly UIDocument _uiDoc;
        private readonly Line _line;
        private readonly int _tolerance = 3;
        private readonly double _dTolerance = Math.Pow(0.1 , 3);
        private readonly XYZ _center;

        /// <summary>
        /// Instantiate an object to get direction at <paramref name="connectionPoint"/> of <paramref name="connectionElement"/>.
        /// </summary>
        /// <param name="connectionPoint"></param>
        /// <param name="connectionElement"></param>
        /// <param name="uiDoc"></param>
        /// <remarks>
        /// Specify <paramref name="uiDoc"/> if get direction manually should be enabled.
        /// </remarks>
        public ConnectionDirectionFactory(XYZ connectionPoint, Element connectionElement, UIDocument uiDoc = null)
        {
            _connectionPoint = connectionPoint;
            _connectionElement = connectionElement;
            _uiDoc = uiDoc;
            _line = connectionElement.GetCenterLine();
            _center = _line.GetCenter();
        }

        /// <summary>
        /// Get direction by <paramref name="refPoint"/> of <paramref name="refElement"/>.
        /// </summary>
        /// <param name="refPoint"></param>
        /// <param name="refElement"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> direction to connect connectionElement at connectionPoint.
        /// <para>
        /// <see langword="null"/> if no direction was found.
        /// </para>
        /// </returns>
        public XYZ GetDirection(XYZ refPoint, Element refElement)
        {
            XYZ dir = GetDirection(refElement);
            if (dir is not null) { return dir; }

            dir = GetDirection(refPoint);
            if (dir is not null) { return dir; }

            dir = GetDirectionAtFreeConnetor();
            if (dir is not null) { return dir; }

            dir = GetDirectionManual(_uiDoc);
            if (dir is not null) { return dir; }

            return null;
        }
        private XYZ GetDirection(Element refElement)
        {
            if(_connectionElement.Id == refElement.Id) { return null; }

            var connectors = ConnectorUtils.GetConnectors(_connectionElement);
            foreach (var con in connectors)
            {
                if (con.Find(refElement) is not null)
                {
                    var p = _line.Project(con.Origin).XYZPoint;
                    return (p - _center).Normalize();
                }
            }

            return null;
        }

        private XYZ GetDirection(XYZ pointAtConnectionElement)
        {
            if(!_line.Contains(pointAtConnectionElement, _tolerance)) { return null; }
            return (pointAtConnectionElement - _connectionPoint).Normalize();   
        }

        private XYZ GetDirectionAtFreeConnetor()
        {
            var fCons = ConnectorUtils.GetFreeConnector(_connectionElement);
            if(fCons == null || fCons.Count == 0) { return null; }

            var con1 = fCons.FirstOrDefault(c => c.Origin.DistanceTo(_connectionPoint) < _dTolerance);
           if(con1 == null) { return null; }

            return (con1.Origin - _line.GetCenter()).Normalize();
        }

        private XYZ GetDirectionManual(UIDocument uiDoc)
        {
            if(uiDoc == null) { return null; }

            var selector = new PointSelector(uiDoc) { AllowLink = false };

            var element = selector.Pick($"Укажите 1 точку направления.");
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, selector.Point);
            if (connectionPoint1.IsValid)
            {
                element = selector.Pick($"Укажите 2 точку направления..");
                ConnectionPoint connectionPoint2 = new ConnectionPoint(element, selector.Point);
                return (connectionPoint2.Point - connectionPoint1.Point).Normalize();
            }

            return null;
        }
    }
}
