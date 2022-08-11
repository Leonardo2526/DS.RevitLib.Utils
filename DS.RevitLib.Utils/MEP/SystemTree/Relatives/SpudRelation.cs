using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class SpudRelation : ElementRelation
    {
        public SpudRelation(FamilyInstance familyInst, Element connectedElement) : base(familyInst, connectedElement)
        {
        }

        public override Relation Get()
        {
            var collinear = IsCollinear();

            if (collinear)
            {
                return Relation.Parent;
            }
            else
            {
                return Relation.Child;
            }

        }

        private bool IsCollinear()
        {
            var center = ElementUtils.GetLocationPoint(_familyInst);
            var baseDir = MEPCurveUtils.GetDirection(_connectedElement as MEPCurve);
            var elemcons = ConnectorUtils.GetConnectors(_connectedElement);

            foreach (var con in elemcons)
            {
                XYZ dir = (center - con.Origin).RoundVector().Normalize();

                if (dir.IsZeroLength())
                {
                    continue;
                }

                if (!XYZUtils.Collinearity(dir, baseDir))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
