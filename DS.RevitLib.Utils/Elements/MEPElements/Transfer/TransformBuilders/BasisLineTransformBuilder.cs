using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transforms;
using DS.RevitLib.Utils.Elements.Transfer.TransformModels;
//using OLMP.MEPAC.Entities.LogMessage;
//using Revit.Async;

namespace DS.RevitLib.Utils.Elements.Transfer.TransformBuilders
{
    internal class BasisLineTransformBuilder : TransformBuilder
    {
        public BasisLineTransformBuilder(Basis sourceObject, Line targetObject) : base(sourceObject, targetObject)
        {
        }

        /// <summary>
        /// Build transform to align centerPoints and sourceBasis.X direction with line direction.
        /// </summary>
        /// <returns></returns>
        public override TransformModel Build()
        {
            var source = _sourceObject as Basis;
            var target = _targetObject as Line;

            var transformModel = new BasisLineTransformModel(source, target);

            //add move transform
            var moveVector = target.GetCenter() - source.Point;
            Transform transform = Transform.CreateTranslation(moveVector);
            transformModel.Transforms.Add(transform);

            //add rotate transform
            double angle;
            XYZ axis = source.X.CrossProduct(target.Direction).RoundVector();
            if (axis.IsZeroLength())
            {
                return transformModel;
            }
            else
            {
                angle = source.X.AngleTo(target.Direction);
            }
            transform = Transform.CreateRotationAtPoint(axis, angle, source.Point);
            transformModel.Transforms.Add(transform);

            return transformModel;
        }
    }
}
