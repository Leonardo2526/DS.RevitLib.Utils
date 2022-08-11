using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class NodePusher : Pusher
    {
        public NodePusher(NodeElement node, Element element, ComponentBuilder componentBuilder) : base(node, element, componentBuilder)
        {
        }

        public override void Push()
        {
            _componentBuilder.Stack.Push(_node.Element);

            Relation relation = GetRelation(_node.Element, _element);
            _node.SystemRelation = relation;
            _componentBuilder.Nodes.Add(_node);
        }

        //private Relation GetRelation()
        //{
        //    PartType partType = ElementUtils.GetPartType(_node);
        //    switch (partType)
        //    {
        //        case PartType.Tee:
        //            {
        //                return new TeeRelation(_node, _element).Get();
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
