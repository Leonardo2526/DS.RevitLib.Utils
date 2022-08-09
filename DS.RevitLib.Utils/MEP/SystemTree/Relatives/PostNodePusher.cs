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
        private readonly Element _ownNode;
        private readonly XYZ _ownDirection;
        private readonly ComponentBuilder  _componentBuilder;


        public void Push()
        {
            Relation relation = GetRealation();

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
                    break;
                case Relation.Default:
                    break;
                default:
                    break;
            }
        }

        private Relation GetRealation()
        {
            var dir = ElementUtils.GetDirections(_element).First();
            if (XYZUtils.Collinearity(dir, _ownDirection))
            {
                return Relation.Own;
            }
            else
            {
                return Relation.Child;
            }

        }


    }
}
