using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.Solids.Models;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Models
{
    public class MEPCurveModel : AbstractElementModel
    {
        public MEPCurveModel(MEPCurve mEPCurve, SolidModel solidModel) : base(mEPCurve, solidModel)
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

        /// <summary>
        /// Typecasting Element to MEPCurve.
        /// </summary>
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


        public override double GetSizeByVector(XYZ orth)
        {
            return SolidModel.GetSizeByVector(orth, SolidModel.Center);
        }

    }
}
