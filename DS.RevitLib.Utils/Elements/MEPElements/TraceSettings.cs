using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.MEPElements
{
    public class TraceSettings : ITraceSettings
    {
        private double _a = 90.DegToRad();
        private double _b = 100.MMToFeet();
        private double _c = 100.MMToFeet();
        private double _d = 50.MMToFeet();
        private double _h = 2200.MMToFeet();

        ///<inheritdoc/>
        public List<int> AList { get; } = new List<int>() { 90, 60, 45, 30, 15 };

        ///<inheritdoc/>
        public double A { get => _a; set => _a = value; }

        ///<inheritdoc/>
        public double B { get => _b; set => _b = value; }

        ///<inheritdoc/>
        public double C { get => _c; set => _c = value; }

        ///<inheritdoc/>
        public double D { get => _d; set => _d = value; }

        ///<inheritdoc/>
        public double H { get => _h; set => _h = value; }
    }
}
