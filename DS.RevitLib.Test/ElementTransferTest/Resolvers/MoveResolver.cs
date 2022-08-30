﻿using Autodesk.Revit.DB;
using DS.RevitLib.Test;
using DS.RevitLib.Test.ElementTransferTest;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    internal class MoveResolver : CollisionResolver<TransformModel>
    {
        private readonly SolidModelExt _operationElement;
        private readonly XYZ _basePoint;
        private readonly TargetMEPCuve _targetMEPCurve;
        private readonly double _collisionLength;

        public MoveResolver(SolidElemCollision collision, ICollisionChecker collisionChecker, 
            XYZ basePoint, TargetMEPCuve targetMEPCurve) : 
            base(collision, collisionChecker)
        {
            _operationElement = collision.Object1;
            _basePoint = basePoint;
            _targetMEPCurve = targetMEPCurve;
            CurrnetPoint = _basePoint;
            Solid intersectionSolid = collision.GetIntersection();
            _collisionLength = 0;
        }

        public XYZ CurrnetPoint { get; private set; }

        public override TransformModel Resolve()
        {
            XYZ point = GetPoint();
            if (point is null)
            {
                return null;
            }

            XYZ moveVector = point - _operationElement.CentralPoint;
            Transform rotateTransform = Transform.CreateTranslation(moveVector);
            _operationElement.Transform(rotateTransform);

            if (!_collisionChecker.GetCollisions().Any())
            {
                IsResolved = true;
            }

            if (IsResolved)
            {
                _operationElement.TransformModel.MoveVector = moveVector;
            }
            else
            {
                _successor.Resolve();
            }

            return _operationElement.TransformModel;
        }

        private XYZ GetPoint()
        {
            XYZ point = _basePoint + _targetMEPCurve.Direction.Multiply(_collisionLength);
            if (point.IsBetweenPoints(_targetMEPCurve.StartPlacementPoint, _targetMEPCurve.EntPlacementPoint))
            {
                return point;
            }

                return null;
        }
    }
}