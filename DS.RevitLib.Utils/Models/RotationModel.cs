using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Models
{
    public class RotationModel
    {
        public RotationModel(Line axis, double angle)
        {
            Axis = axis;
            Angle = angle;
        }

        public Line Axis { get; set; }
        public double Angle { get; set; }
    }
}
