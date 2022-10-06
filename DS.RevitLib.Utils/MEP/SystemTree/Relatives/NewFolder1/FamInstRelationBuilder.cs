using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives.NewFolder1
{
    internal class FamInstRelationBuilder : NewElementRelationBuilder<FamilyInstance>
    {
        private readonly NewElementRelationBuilder<FamilyInstance> _builer;

        public FamInstRelationBuilder(FamilyInstance baseElement) : base(baseElement)
        {
            _builer = GetBuilder();
        }

        public override Relation GetRelation(Element element)
        {
            return _builer.GetRelation(element);
        }

        private NewElementRelationBuilder<FamilyInstance> GetBuilder()
        {
            PartType partType = ElementUtils.GetPartType(_baseElement);
            switch (partType)
            {
                case PartType.Tee:
                    return new TeeRelationBuilder(_baseElement);
                case PartType.SpudPerpendicular: case PartType.SpudAdjustable:
                case PartType.TapAdjustable: case PartType.TapPerpendicular:
                    return new SpudRelationBuilder(_baseElement);
                default:
                    break;
            }

            return null;
        }
    }
}
