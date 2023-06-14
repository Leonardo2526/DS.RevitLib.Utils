using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace DS.RevitLib.Utils.MEP.Models
{
    public class MEPCurveGeometryModel
    {
        public MEPCurveGeometryModel(MEPCurve mEPCurve)
        {
            MEPCurve = mEPCurve;
        }

        #region Properties

        public MEPCurve MEPCurve { get; }

        public MEPCurveType MEPCurveType => MEPCurve.Document.GetElement(MEPCurve.GetTypeId()) as MEPCurveType;

        public ConnectorProfileType ProfileType => MEPCurveUtils.GetProfileType(MEPCurve);        

        public XYZ Direction => MEPCurveUtils.GetDirection(MEPCurve);

        public List<Connector> Connectors => ConnectorUtils.GetConnectors(MEPCurve);

        public List<Connector> MainConnectors 
        { 
            get
            {
                var (con1, con2) = ConnectorUtils.GetMainConnectors(MEPCurve);
                return new List<Connector> { con1, con2 };
            }
        }

        public List<XYZ> ConnectorsPoints => Connectors.Select(c => c.Origin).ToList();
        public double Length => MEPCurveUtils.GetLength(MEPCurve);

        /// <summary>
        /// Outer width.
        /// </summary>
        public double Width => MEPCurve.GetOuterWidth();

        /// <summary>
        /// Outer height.
        /// </summary>
        public double Height => MEPCurve.GetOuterHeight();

        /// <summary>
        /// Outer area.
        /// </summary>
        public double Area => MEPCurve.GetOuterArea();

        /// <summary>
        /// Center line.
        /// </summary>
        public Line Line => MEPCurveUtils.GetLine(MEPCurve);


        public double InsulationThickness => MEPCurve.GetInsulationThickness();    

        #endregion


        public double GetSizeByVector(XYZ vector)
        {
            return MEPCurveUtils.GetSizeByVector(MEPCurve, vector);
        }

    }
}
