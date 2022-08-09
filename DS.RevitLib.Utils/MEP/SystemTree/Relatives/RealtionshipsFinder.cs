using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal abstract class ElementRelation
    {
        private readonly Element _element;
        private readonly Element _ownNode;
        private readonly XYZ _ownDirection;

        protected ElementRelation(Element element, Element ownNode, XYZ ownDirection)
        {
            _element = element;
            _ownNode = ownNode;
            _ownDirection = ownDirection;
        }

        public abstract Relation Get();
    }
}
