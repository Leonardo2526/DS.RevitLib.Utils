using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class TeeRelation : ElementRelation
    {
        public TeeRelation(FamilyInstance familyInst, Element connectedElement) : base(familyInst, connectedElement)
        {
        }

        public override Relation Get()
        {
            var collinear = GetCollinear();

            if (collinear is null)
            {
                return Relation.Parent;
            }
            else
            {
                return Relation.Child;
            }

        }

        private XYZ GetCollinear()
        {
            var center = ElementUtils.GetLocationPoint(_familyInst);
            var commonConnector = ConnectorUtils.GetCommonConnectors(_familyInst, _connectedElement).elem2Con;
            var systemDirection = (center - commonConnector.Origin).RoundVector().Normalize();

            var conDirs = ElementUtils.GetDirections(_familyInst);

            foreach (var dir in conDirs)
            {              
                if (XYZUtils.Collinearity(dir, systemDirection))
                {
                    return dir;
                }
            }

            return null;
        }
    }
}
