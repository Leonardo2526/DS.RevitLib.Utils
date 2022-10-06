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
            var (con1, con2) = ConnectorUtils.GetMainConnectors(MEPCurve);
            MainConnectors = new List<Connector> { con1, con2 };
            MEPCurveType = mEPCurve.Document.GetElement(mEPCurve.GetTypeId()) as MEPCurveType;
            Connectors = ConnectorUtils.GetConnectors(mEPCurve);
            ConnectorsPoints= Connectors.Select(c => c.Origin).ToList();
            Line = MEPCurveUtils.GetLine(mEPCurve);
        }

        #region Properties

        public MEPCurve MEPCurve { get; }
        public MEPCurveType MEPCurveType { get; }
        public ConnectorProfileType ProfileType { get; }
        public XYZ Direction { get; }
        public List<Connector> Connectors { get; }
        public List<Connector> MainConnectors { get; }
        public List<XYZ> ConnectorsPoints { get; }
        public double Length { get; }
        public double Width { get; }
        public double Height { get; }
        public double Area { get; }
        public Line Line { get; }    
        public double InsulationThickness
        {
            get
            {
                return Insulation.GetThickness(MEPCurve);
            }
        }

        #endregion


        public double GetSizeByVector(XYZ vector)
        {
            return MEPCurveUtils.GetSizeByVector(MEPCurve, vector);
        }

    }
}
