using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.MEPElements
{
    /// <summary>
    /// The interface used for trace settings.
    /// </summary>
    public interface ITraceSettings
    {
        /// <summary>
        /// Elbow angle.
        /// </summary>
        double A { get; set; }

        /// <summary>
        /// Distance between elements.
        /// </summary>
        double B { get; set; }

        /// <summary>
        /// Distance between elements.
        /// </summary>
        double C { get; set; }

        /// <summary>
        /// Distance between elements joints.
        /// </summary>
        double D { get; set; }

        /// <summary>
        /// Distance between elements and floor.
        /// </summary>
        double H { get; set; }

        /// <summary>
        /// Available elbows angles.
        /// </summary>
        public List<int> AList { get; }
    }
}
