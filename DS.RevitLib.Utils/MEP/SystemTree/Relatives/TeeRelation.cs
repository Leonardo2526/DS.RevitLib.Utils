using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class TeeRelation : ElementRelation
    {
        public TeeRelation(Element element, Element connectedToElement, XYZ ownDirection) : base(element, connectedToElement, ownDirection)
        {
        }

        public override Relation Get()
        {
            var conDirs = ElementUtils.GetDirections(_element);
            var collinears = conDirs.Where(x => XYZUtils.Collinearity(x, _ownDirection)).ToList();

            if (collinears.Any())
            {
                return Relation.Child; 
            }
            else
            {
                return Relation.Parent;
            }

        }
    }
}
