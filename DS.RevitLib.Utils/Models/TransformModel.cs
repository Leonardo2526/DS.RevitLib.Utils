﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Models
{
    public class TransformModel
    {
        public XYZ MoveVector { get; set; }
        public RotationModel CenterLineRotation { get; set; }
        public RotationModel AroundCenterLineRotation { get; set; }
        public XYZ ReferencePointMoveVector { get; set; }
    }
}
