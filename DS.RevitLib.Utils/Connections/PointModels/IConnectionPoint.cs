using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Connections.PointModels
{
    /// <summary>
    /// Represents objects used for system connection.
    /// </summary>
    public interface IConnectionPoint
    {

        /// <summary>
        /// Point of connection.
        /// </summary>
        public XYZ Point { get; }

        /// <summary>
        /// Specify if point is valid. 
        /// </summary>
        public bool IsValid { get; set; }
    }
}
