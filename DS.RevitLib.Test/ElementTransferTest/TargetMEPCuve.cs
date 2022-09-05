using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.ElementTransferTest
{
    public class TargetMEPCuve
    {
        public TargetMEPCuve(MEPCurve mEPCurve, XYZ startPlacementPoint, XYZ entPlacementPoint, 
            Connector startConnector, Connector endConnector)
        {
            MEPCurve = mEPCurve;
            StartPlacementPoint = startPlacementPoint;
            EntPlacementPoint = entPlacementPoint;
            StartConnector = startConnector;
            EndConnector = endConnector;
        }

        public MEPCurve MEPCurve { get; private set; }
        public XYZ StartPlacementPoint { get; private set; }
        public XYZ EntPlacementPoint { get; private set; }
        public Line Line
        {
            get
            {
               return MEPCurve.GetCenterLine();
            }
        }

        public Connector StartConnector { get; private set; }
        public Connector EndConnector { get; private set; }

        public XYZ Direction
        {
            get
            {
                return (EndConnector.Origin - StartConnector.Origin).RoundVector().Normalize();
            }
        }
    }
}
