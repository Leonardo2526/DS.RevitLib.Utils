﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolve
{
    internal class RotateCenterLineResolver : CollisionResolver
    {
        public override bool CheckCollisions()
        {
            throw new NotImplementedException();
        }

        public override void Resolve()
        {
            if (IsResolved)
            {
                return;
            }
        }
    }
}
