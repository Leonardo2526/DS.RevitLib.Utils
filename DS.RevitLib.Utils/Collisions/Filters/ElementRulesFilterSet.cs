using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisons;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions
{
   public  static class ElementRulesFilterSet
    {
        //public static Func<(Element, Element), bool> ToElementsRool(this Func<(MEPCurve, FamilyInstance), bool> mEPRool)
        //{
        //    bool func((Element, Element) f)
        //    {
        //        var e1 = f.Item1;
        //        var e2 = f.Item2;

        //        if (e1 is MEPCurve mEPCurve && e2 is FamilyInstance famIns)
        //        {
        //            var arg = (mEPCurve, famIns);
        //            return mEPRool.Invoke(arg);
        //        }

        //        return true;
        //    }

        //    return func;
        //}

        public static Func<(Element, Element), bool> ElementsRool()
        {
            static bool func((Element, Element) f)
            {
                var e1 = f.Item1;
                var e2 = f.Item2;

                return true;
            }

            return func;
        }

        //public static Func<(MEPCurve, FamilyInstance), bool> MEPCurveFamInstRool()
        //{
        //    Func<(MEPCurve, FamilyInstance), bool> func = (f) =>
        //    {
        //        var e1 = f.Item1;
        //        var e2 = f.Item2;
        //        return true;
        //    };

        //    return func;
        //}

        public static Func<(Element, Element), bool> MEPCurveFamInstRool()
        {
            Func<(Element, Element), bool> func = (f) =>
            {
                var e1 = f.Item1;
                var e2 = f.Item2;

                if (e1 is MEPCurve mEPCurve && e2 is FamilyInstance famIns)
                {
                    var arg = (mEPCurve, famIns);
                    return true;
                }

                return true;
            };

            return func;
        }
    }
}
