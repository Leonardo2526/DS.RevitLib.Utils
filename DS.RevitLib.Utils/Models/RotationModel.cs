using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Models
{
    public struct RotationModel
    {
        public RotationModel(Line rotationAxis, double rotationAngle)
        {
            RotationAxis = rotationAxis;
            RotationAngle = rotationAngle;
        }

        public Line RotationAxis { get; set; }
        public double RotationAngle { get; set; }
    }
}
