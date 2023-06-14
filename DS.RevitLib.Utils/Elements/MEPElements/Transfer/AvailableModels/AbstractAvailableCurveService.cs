using Nito.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.AvailableModels
{
    public abstract class AbstractAvailableCurveService<T>
    {
        protected readonly List<T> _targetCurves;
        protected readonly double _minCurveLength;
        protected readonly double _minPlacementLength;
        private readonly bool _saveElementsOrder;

        protected AbstractAvailableCurveService(List<T> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false)
        {
            _targetCurves = targetCurves;
            _minCurveLength = minCurveLength;
            _minPlacementLength = minPlacementLength;
            _saveElementsOrder = saveElementsOrder;
            List<T> validMEPCurves = targetCurves.Where(obj => CheckMinPlacementLength(obj)).ToList();
            validMEPCurves.ForEach(obj => AvailableCurves.AddToBack(obj));
        }

        public Deque<T> AvailableCurves { get; private set; } = new Deque<T>();

        public T Get(double placementLength)
        {
            int counter = AvailableCurves.Count;
            while (!CheckPlacementLength(AvailableCurves.First(), placementLength))
            { counter--; if (counter == 0) { return default(T); } }
            return AvailableCurves.RemoveFromFront();
        }

        protected bool CheckPlacementLength(T curve, double placementLength)
        {
            double curveLength = GetLength(curve);
            if (curveLength < placementLength)
            {
                if (_saveElementsOrder)
                {
                    AvailableCurves.RemoveFromFront();
                }
                else
                {
                    AvailableCurves.AddToBack(AvailableCurves.RemoveFromFront());
                }
                return false;
            }

            return true;
        }

        public bool CheckMinPlacementLength(T curve)
        {
            double curveLength = GetLength(curve);
            if (curveLength > _minPlacementLength)
            {
                return true;
            }

            return false;
        }

        protected abstract double GetLength(T curve);
    }
}
