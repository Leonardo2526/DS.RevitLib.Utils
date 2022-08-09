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
        protected readonly Element _element;
        protected readonly Element _connectedElement;
        protected readonly XYZ _ownDirection;

        protected ElementRelation(Element element, Element connectedToElement, XYZ ownDirection)
        {
            _element = element;
            _connectedElement = connectedToElement;
            _ownDirection = ownDirection;
        }

        public abstract Relation Get();
    }
}
