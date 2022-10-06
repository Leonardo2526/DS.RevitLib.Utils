using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives.NewFolder1
{
    internal class TeeRelationBuilder : NewElementRelationBuilder<FamilyInstance>
    {
        private readonly XYZ _baseCenter;
        private readonly List<XYZ> _baseDirs;

        public TeeRelationBuilder(FamilyInstance baseElement) : base(baseElement)
        {
            _baseCenter = ElementUtils.GetLocationPoint(_baseElement);
            _baseDirs = ElementUtils.GetDirections(_baseElement);
        }

        public override Relation GetRelation(Element element)
        {
            var collinear = IsCollinear(element);
            return collinear ? Relation.Parent : Relation.Child;
        }

        private bool IsCollinear(Element element)
        {
            var commonConnector = ConnectorUtils.GetCommonConnectors(_baseElement, element).elem2Con;
            var systemDirection = (_baseCenter - commonConnector.Origin).RoundVector().Normalize();

            foreach (var dir in _baseDirs)
            {
                if (XYZUtils.Collinearity(dir, systemDirection))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
