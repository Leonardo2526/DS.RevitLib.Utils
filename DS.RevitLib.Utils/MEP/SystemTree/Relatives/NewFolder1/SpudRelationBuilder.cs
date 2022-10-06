﻿using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives.NewFolder1
{
    internal class SpudRelationBuilder : NewElementRelationBuilder<FamilyInstance>
    {
        private readonly XYZ _baseCenter;

        public SpudRelationBuilder(FamilyInstance baseElement) : base(baseElement)
        {
            _baseCenter = ElementUtils.GetLocationPoint(_baseElement);
        }

        public override Relation GetRelation(Element element)
        {
            if (element is not MEPCurve)
            {
                throw new ArgumentException("Element should be MEPCurve type.");
            }
            var collinear = IsCollinear(element as MEPCurve);
            return collinear ? Relation.Child : Relation.Parent;
        }

        private bool IsCollinear(MEPCurve mEPCurve)
        {
            var elementDir = MEPCurveUtils.GetDirection(mEPCurve);
            var elemCons = ConnectorUtils.GetConnectors(mEPCurve);

            foreach (var con in elemCons)
            {
                XYZ baseToConDir = (_baseCenter - con.Origin).RoundVector().Normalize();
                if (!baseToConDir.IsZeroLength() && !XYZUtils.Collinearity(baseToConDir, elementDir))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
