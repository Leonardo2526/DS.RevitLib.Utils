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
            Id = mEPCurve.Id.IntegerValue;
        }

        #region Properties

        public ConnectorProfileType ProfileType => MEPCurveUtils.GetProfileType(MEPCurve);

        public XYZ Direction => MEPCurveUtils.GetDirection(MEPCurve);

        public List<Connector> Connectors => ConnectorUtils.GetConnectors(MEPCurve);

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

        /// <summary>
        /// Typecasting Element to MEPCurve.
        /// </summary>
        public MEPCurve MEPCurve { get; }

        public double InsulationThickness => MEPCurve.GetInsulationThickness();

        public int Id { get; }

        #endregion


        public override double GetSizeByVector(XYZ orth)
        {
            return SolidModel.GetSizeByVector(orth, SolidModel.Center);
        }

    }
}
