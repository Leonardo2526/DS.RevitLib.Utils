using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Models
{
    public class MEPCurveGeometryModel
    {
        public MEPCurveGeometryModel(MEPCurve mEPCurve)
        {
            MEPCurve = mEPCurve;
            ProfileType = MEPCurveUtils.GetProfileType(mEPCurve);
            Direction = MEPCurveUtils.GetDirection(mEPCurve);
            (Width, Height) = MEPCurveUtils.GetWidthHeight(mEPCurve);
            Length = MEPCurveUtils.GetLength(mEPCurve);
            Area = MEPCurveUtils.GetCrossSectionArea(mEPCurve);
        }

        #region Properties

        public ConnectorProfileType ProfileType { get; private set; }
        public XYZ Direction { get; private set; }
        public List<Connector> Connectors
        {
            get
            {
                return ConnectorUtils.GetConnectors(MEPCurve);
            }
        }
        public List<Connector> MainConnectors
        {
            get
            {
                var (con1, con2) = ConnectorUtils.GetMainConnectors(MEPCurve);
                return new List<Connector> { con1, con2 } ;
            }
        }
        public List<XYZ> ConnectorsPoints
        {
            get
            {
                return Connectors.Select(c => c.Origin).ToList();
            }
        }
        public double Length { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Area { get; private set; }
        public Line Line
        {
            get
            {
                return MEPCurveUtils.GetLine(MEPCurve);
            }
        }
        public MEPCurve MEPCurve { get; }
        public double InsulationThickness
        {
            get
            {
                if (Type.Name == "Pipe" || Type.Name == "Duct")
                {
                    return Insulation.GetThickness(MEPCurve);
                }
                else
                {
                    return 0;
                }
            }
        }


        #endregion


        public double GetSizeByVector(XYZ vector)
        {
            return MEPCurveUtils.GetSizeByVector(MEPCurve, vector);
        }

    }
}
