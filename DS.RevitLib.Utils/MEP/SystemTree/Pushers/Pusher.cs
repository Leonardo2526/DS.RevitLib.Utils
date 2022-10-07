using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal abstract class Pusher
    {
        protected readonly Element _element;
        protected readonly NodeElement _node;
        protected readonly ComponentBuilder _componentBuilder;

        public Pusher(NodeElement node, Element element, ComponentBuilder componentBuilder)
        {
            _element = element;
            _node = node;
            _componentBuilder = componentBuilder;
        }

        public abstract void Push();


        protected Relation GetRelation(FamilyInstance familyInstance, Element element)
        {
            PartType partType = ElementUtils.GetPartType(familyInstance);
            NewElementRelationBuilder<FamilyInstance> relationBuilder = null;
            switch (partType)
            {
                case PartType.Tee:
                    {
                        relationBuilder = new TeeRelationBuilder(familyInstance);
                    }
                    break;
                case PartType.SpudPerpendicular:
                case PartType.SpudAdjustable:
                case PartType.TapPerpendicular:
                case PartType.TapAdjustable:
                    {
                        relationBuilder = new SpudRelationBuilder(familyInstance);
                    }
                    break;
                default:
                    break;
            }

            return relationBuilder.GetRelation(element) == Relation.Child ? Relation.Parent : Relation.Child;
        }
    }
}
