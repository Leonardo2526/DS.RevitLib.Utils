using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using Nito.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Test
{
    internal class AvailableMEPCurvesService
    {
        private readonly double _minMEPCurveLength;
        private readonly bool _orederedFamInstSequence;
        private readonly double _minPlacementLength;
        private double _placementLength;

        public AvailableMEPCurvesService(List<MEPCurve> targetMEPCurves, double minMEPCurveLength, double minPlacementLength,
            bool orederedFamInstSequence = true)
        {
            _minMEPCurveLength = minMEPCurveLength;
            _minPlacementLength = minPlacementLength;
            _orederedFamInstSequence = orederedFamInstSequence;

            List<MEPCurve> validMEPCurves = targetMEPCurves.Where(obj => CheckMinLength(obj)).ToList();
            validMEPCurves.ForEach(obj => AvailableMEPCurves.AddToBack(obj));

        }
        //public Stack<MEPCurve> ReserveStack { get; private set; }
        public Deque<MEPCurve> AvailableMEPCurves { get; private set; } = new Deque<MEPCurve>();

        public MEPCurve Get(double placementLength)
        {
            _placementLength = placementLength;

            while(!AvailableForPlacement(AvailableMEPCurves.First()))
                {

            }           
            //var mEPCurves = AvailableMEPCurves.Where(obj => AvailableForPlacement(obj));
            return AvailableMEPCurves.RemoveFromFront();
        }

        public bool CheckMinLength(MEPCurve mEPCurve)
        {
            double length = mEPCurve.GetCenterLine().ApproximateLength;
            if (length > _minPlacementLength)
            {
                return true;
            }

            return false;
        }

        private bool AvailableForPlacement(MEPCurve mEPCurve)
        {
            if (CheckCollisions(mEPCurve) && CheckLength(mEPCurve))
            {
                return true;
            }

            return false;
        }

        private bool CheckLength(MEPCurve mEPCurve)
        {
            double targetLength = mEPCurve.GetCenterLine().ApproximateLength;
            if (targetLength < _placementLength)
            {
                if (_orederedFamInstSequence)
                {
                    AvailableMEPCurves.RemoveFromFront();
                }
                else
                {
                    AvailableMEPCurves.AddToBack(AvailableMEPCurves.RemoveFromFront());
                }
                return false;
            }

            return true;
        }

        private bool CheckCollisions(MEPCurve mEPCurve)
        {
            //code here

            return true;
        }
    }
}
