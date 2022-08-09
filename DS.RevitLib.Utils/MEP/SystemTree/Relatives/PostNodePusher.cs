using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class PostNodePusher : Pusher
    {
        public PostNodePusher(NodeElement node, Element element, ComponentBuilder componentBuilder) : base(node, element, componentBuilder)
        {
        }

        public bool PushedToParent { get; private set; }

        public override void Push()
        {
            Relation teeRelation = GetRelation(_node.Element, _element);

            switch (teeRelation)
            {              
                case Relation.Child:
                    if (_node.SystemRelation == Relation.Child)
                    {
                        _componentBuilder.Stack.Push(_element);
                    }
                    else if(_node.SystemRelation == Relation.Parent)
                    {
                        _componentBuilder._mEPSystemBuilder.ParentStack.Push(_element);
                        _componentBuilder.Elements.Move(0, _componentBuilder.Elements.Count - 1);
                        PushedToParent = true;
                    }
                    break;
                case Relation.Parent:
                    _componentBuilder.ChildElements.Add(_element);
                    break;
                default:
                    break;
            }
        }

        //private Relation GetRealation(Element element)
        //{
        //    var dir = ElementUtils.GetDirections(element);
        //    var collinears = dir.Where(x => XYZUtils.Collinearity(x, _ownDirection)).ToList();

        //    if (collinears.Any())
        //    {
        //        return Relation.Own;
        //    }

        //    return _nodeElement.Relation;
        //}

        //private Relation GetRelation()
        //{
        //    PartType partType = ElementUtils.GetPartType(_nodeElement.Element);
        //    switch (partType)
        //    {
        //        case PartType.Tee:
        //            {
        //                return new TeeRelation(_nodeElement.Element, _element).Get();
        //            }
        //        case PartType.SpudPerpendicular:
        //            break;
        //        default:
        //            break;
        //    }


        //    return Relation.Default;
        //}
    }



}
