using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class NodePusher
    {
        private readonly Element _currentElement;
        private readonly Element _node;
        private readonly XYZ _ownDirection;
        private readonly MEPSystemBuilder _mEPSystemBuilder;
        private readonly ComponentBuilder _componentBuilder;


        public NodePusher(Element connectedElement, Element currentElement, XYZ ownDirection, ComponentBuilder componentBuilder)
        {
            _currentElement = currentElement;
            _node = connectedElement;
            _ownDirection = ownDirection;
            _componentBuilder = componentBuilder;
            _mEPSystemBuilder = componentBuilder._mEPSystemBuilder;
        }

        public void Push()
        {
            _componentBuilder.OwnStack.Push(_node);

            Relation relation = GetRelation(_node);

            //switch (relation)
            //{               
            //    case Relation.Child:
            //        _componentBuilder._mEPSystemBuilder.ChildStack.Push(_node);
            //        break;
            //    case Relation.Parent:
            //        _componentBuilder._mEPSystemBuilder.ParentStack.Push(_node);
            //        break;
            //    default:
            //        break;
            //}

            _componentBuilder.Nodes.Add(new NodeElement(_node, relation));
        }

        private Relation GetRelation(Element element)
        {
            PartType partType = ElementUtils.GetPartType(element as FamilyInstance);
            switch (partType)
            {
                case PartType.Tee:
                    {
                        return new TeeRelation(element, _node, _ownDirection).Get();
                    }
                case PartType.SpudPerpendicular:
                    break;
                default:
                    break;
            }


            return Relation.Default;
        }
    }



}
