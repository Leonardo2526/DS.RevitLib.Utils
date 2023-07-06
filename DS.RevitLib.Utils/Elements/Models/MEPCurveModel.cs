using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transactions;
using DS.RevitLib.Utils.Various;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public double ElbowRadius { get; private set; }

        #endregion


        public override double GetSizeByVector(XYZ orth)
        {
            return SolidModel.GetSizeByVector(orth, SolidModel.Center);
        }

        public async Task<double> GetElbowRadius(double elbowAngle, ITransactionFactory factory = null)
        {
            factory ??= new ContextTransactionFactory(MEPCurve.Document);
            return ElbowRadius = await new ElbowRadiusCalc(this, factory).GetRadius(elbowAngle);
        }

    }
}
