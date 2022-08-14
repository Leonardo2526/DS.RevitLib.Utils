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
        protected readonly FamilyInstance _familyInst;
        protected readonly Element _connectedElement;

        protected ElementRelation(FamilyInstance familyInst, Element connectedElement)
        {
            _familyInst = familyInst;
            _connectedElement = connectedElement;
        }

        public abstract Relation Get();
    }
}
