using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.CLZ
{
    class CLZInfo
    {
        static double ElementWidthHalf = (ElementSize.ElemDiameterF / 2);
        static double ElementHeghtHalf = (ElementSize.ElemDiameterF / 2);

        static double Clearance = 100;


        /// <summary>
        /// Clerance between elements in reference coordinate system by width
        /// </summary>
        public static int WidthClearanceRCS { get; set; } = 0;

        ///// <summary>
        ///// Clerance between elements in reference coordinate system by height
        ///// </summary>
        public static int HeightClearanceRCS { get; set; } = 0;

        public static double WidthClearanceF { get; set; }
        public static double HeightClearanceF { get; set; }

        public CLZInfo()
        {
            double ClearanceF = UnitUtils.Convert(Clearance,
                               DisplayUnitType.DUT_MILLIMETERS,
                               DisplayUnitType.DUT_DECIMAL_FEET);

            WidthClearanceF = ClearanceF + ElementWidthHalf;
            WidthClearanceRCS = (int)Math.Round(WidthClearanceF / Main.PointsStepF);

            HeightClearanceF = ClearanceF + ElementHeghtHalf;
            HeightClearanceRCS = (int)Math.Round(HeightClearanceF / Main.PointsStepF);

        }



    }
}
