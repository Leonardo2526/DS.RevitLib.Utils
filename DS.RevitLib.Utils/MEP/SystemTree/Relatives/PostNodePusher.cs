using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class PostNodePusher
    {
        private readonly Element _element;
        private readonly NodeElement _nodeElement;
        private readonly XYZ _ownDirection;
        private readonly ComponentBuilder  _componentBuilder;

        public PostNodePusher(Element element, NodeElement nodeElement, ComponentBuilder componentBuilder)
        {
            _element = element;
            _nodeElement = nodeElement;
            _ownDirection = componentBuilder._direction;
            _componentBuilder = componentBuilder;
        }

        public bool PushedToParent { get; private set; }

        public void Push()
        {
            Relation relation = GetRealation(_element);

            switch (relation)
            {
                case Relation.Own:
                    _componentBuilder.OwnStack.Push(_element);
                    break;
                case Relation.Child:
                    _componentBuilder._mEPSystemBuilder.ChildStack.Push(_element);
                    break;
                case Relation.Parent:
                    _componentBuilder._mEPSystemBuilder.ParentStack.Push(_element);
                    _componentBuilder.Elements.Move(0, _componentBuilder.Elements.Count - 1);
                    PushedToParent = true;
                    break;
                default:
                    break;
            }
        }

        private Relation GetRealation(Element element)
        {
            var dir = ElementUtils.GetDirections(element);
            var collinears = dir.Where(x => XYZUtils.Collinearity(x, _ownDirection)).ToList();

            if (collinears.Any())
            {
                return Relation.Own;
            }

            return _nodeElement.Relation;
        }
    }



}
