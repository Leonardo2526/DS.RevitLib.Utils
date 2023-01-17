using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    /// <summary>
    /// An object to get move transform from intersection solid.
    /// </summary>
    public class MoveTransformStrategy
    {
        private readonly Line _baseLine;
        private readonly XYZ _endPoint;
        private readonly XYZ _moveDir;
        private readonly double _offset;

        /// <summary>
        /// Instantiate an object to get move transform from intersection solid.
        /// </summary>
        public MoveTransformStrategy(Line baseLine, XYZ startPoint, XYZ endPoint, XYZ moveDir, double offset = 0)
        {
            _baseLine = baseLine;
            StartPoint = startPoint;
            _endPoint = endPoint;
            _moveDir = moveDir;
            _offset = offset;
        }

        /// <summary>
        /// Get transform from <paramref name="basePoint"/> by <paramref name="intersectionSolid"/>.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="intersectionSolid"></param>
        /// <returns>Returns movement from <paramref name="basePoint"/>.</returns>
        public Transform GetTransform(XYZ basePoint, Solid intersectionSolid)
        {
            (var p1, var p2) = intersectionSolid.GetEdgeProjectPoints(_baseLine);
            double moveDist = p1.DistanceTo(p2) + _offset;

            XYZ vector = _moveDir.Multiply(moveDist);

            //check new point position
            MovePoint = basePoint + vector;
            if(!MovePoint.IsBetweenPoints(basePoint, _endPoint)) { return null; }

            return Transform.CreateTranslation(vector);
        }

        /// <summary>
        /// Start point on <see cref="MEPCurve"/> to find movement.
        /// </summary>
        public XYZ StartPoint { get;private set; }

        /// <summary>
        /// Transformed point from basePoint.
        /// </summary>
        public XYZ MovePoint { get; private set; }
    }
}
