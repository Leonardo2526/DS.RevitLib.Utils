using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class TeeMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        public TeeMEPCurveStrategy(MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength) : 
            base(mEPCurve1, mEPCurve2, minCurveLength)
        {
        }

        public override bool Connect()
        {
            throw new NotImplementedException();
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
