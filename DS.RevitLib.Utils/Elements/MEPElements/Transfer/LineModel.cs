using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    public class LineModel
    {
        public LineModel(Line line, Basis basis)
        {
            Line = line;
            Basis = basis;
        }

        public LineModel(MEPCurve mEPCurve)
        {
            Line = MEPCurveUtils.GetLine(mEPCurve);
            Basis = mEPCurve.GetBasis();
        }

        public Line Line { get; }
        public Basis Basis { get; private set; }

    }
}
