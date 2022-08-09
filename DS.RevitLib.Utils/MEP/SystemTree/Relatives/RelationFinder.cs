using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class RelationFinder
    {
        private readonly Element _element;
        private readonly Element _ownNode;
        private readonly XYZ _ownDirection;
        private readonly MEPSystemBuilder _mEPSystemBuilder;


        public RelationFinder(Element element, Element ownNode, XYZ ownDirection, MEPSystemBuilder mEPSystemBuilder)
        {
            _element = element;
            _ownNode = ownNode;
            _ownDirection = ownDirection;
            _mEPSystemBuilder = mEPSystemBuilder;
        }

        //public Relation Find(PartType partType)
        //{

        //    switch (partType)
        //    {
        //        case PartType.Tee:
        //            {
        //                return new TeeRelation(_element, _ownNode, _ownDirection).Get();
        //            }
        //        case PartType.SpudPerpendicular:
        //                break;
        //            default:
        //                break;
        //        }
                     

        //    return Relation.Default;
        //}
    }
}
