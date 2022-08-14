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
            switch (partType)
            {
                case PartType.Tee:
                    {
                        return new TeeRelation(familyInstance, element).Get();
                    }
                case PartType.SpudPerpendicular:
                    {
                        return new SpudRelation(familyInstance, element).Get();
                    }
                default:
                    break;
            }


            return Relation.Default;
        }
    }
}
