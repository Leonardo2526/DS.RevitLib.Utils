using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Connections.PointModels.PointModels
{
    /// <summary>
    /// An object that represents <see cref="ConnectionPoint"/> validator.
    /// </summary>
    public class ConnectionPointValidator
    {
        private readonly Document _doc;
        private readonly MEPSystemModel _mEPSystemModel;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly int _distnaceToFindFloor = 30;


        /// <summary>
        /// Instantiate an object to validate <see cref="ConnectionPoint"/>.
        /// </summary>
        /// <param name="mEPSystemModel"></param>
        /// <param name="docElements"></param>
        /// <param name="linkElementsDict"></param>
        public ConnectionPointValidator(MEPSystemModel mEPSystemModel, IElementCollisionDetector collisionDetector)
        {
            _doc = mEPSystemModel.Root.BaseElement.Document;
            _mEPSystemModel = mEPSystemModel;
            _collisionDetector = collisionDetector;
        }

        /// <summary>
        /// Validated point.
        /// </summary>
        public ConnectionPoint ConnectionPoint { get; set; }

        /// <summary>
        /// Specifies used bound of <see cref="Document"/>.
        /// </summary>
        public Outline BoundOutline { get; set; }

        /// <summary>
        /// Factory to commit transactions.
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }

        public ITraceSettings TraceSettings { get; set; }

        public bool IsInsulationAccount { get; set; }

        public bool CheckFloorLimits { get; set; }

        /// <summary>
        /// Specifies whether point is valid for connection.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if <see cref="_mEPSystemModel"/> contatins point and point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        public bool Validate()
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(ConnectionPoint);
            if (!Validator.TryValidateObject(ConnectionPoint, context, results, true))
            {
                if (Messenger != null)
                {
                    var messageBuilder = new StringBuilder();
                    if (results.Count == 1) 
                    { messageBuilder.AppendLine(results.First().ErrorMessage); }
                    else if (results.Count > 1)
                        for (int i = 0; i < results.Count; i++)
                        {
                            var r = results[i];
                            messageBuilder.AppendLine($"Ошибка {i + 1}. {r.ErrorMessage}");
                            messageBuilder.AppendLine("---------");
                        }

                    if (messageBuilder.Length > 0) { Messenger.Show(messageBuilder.ToString(), "Ошибка"); }
                }
                //{ results.ForEach(r => Messenger.Show(r.ErrorMessage, "Ошибка")); }
                return false;
            }
            else { return true; }
        }

        /// <summary>
        /// Specifies whether <see cref="_mEPSystemModel"/> contatins point.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if <see cref="_mEPSystemModel"/> contatins point.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        public bool GetSystemValidity()
        {
            if (ConnectionPoint.Element is null) { return false; }
            return _mEPSystemModel.AllElements.Select(obj => obj.Id).Contains(ConnectionPoint.Element.Id);
        }

        /// <summary>
        /// Specifies whether point has no collisions.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        public List<(XYZ, Element)> GetCollisions()
        {
            var collisions = new List<(XYZ, Element)>();

            //get collisions in freeCon.
            var elemCollisions = _collisionDetector.GetCollisions(ConnectionPoint.Element);
            if (!elemCollisions.Any()) { return collisions; }

            if (ConnectionPoint.Element is FamilyInstance)
            { return collisions; }
            else
            {
                //Specify collision objects on point
                foreach (var c in elemCollisions)
                {
                    if (c.Item2.GetSolidInLink(_doc).Contains(ConnectionPoint.Point))
                    {
                        var pc = (ConnectionPoint.Point, c.Item2);
                        collisions.Add(pc);
                    }
                }
                return collisions;
            }
        }

        /// <summary>
        /// Specifies if <paramref name="point"/> is within outline limits.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="point"/> is within <see cref="BoundOutline"/>.
        /// <para> 
        /// Othewise returns <see langword="false"/>.</para>
        /// </returns>
        public bool IsWithinOutlineLimits(XYZ point)
        {
            if (BoundOutline is null) { return true; }
            else
            {
                return point.More(BoundOutline.MinimumPoint) && point.Less(BoundOutline.MaximumPoint);
            }
        }

        /// <summary>
        /// Specifies if <paramref name="connectionPoint"/> is within floor/ceiling limits.
        /// </summary>
        /// <param name="connectionPoint"></param>
        /// <returns>
        /// (<see langword="true"/>, <see langword="true"/>) 
        /// if <paramref name="connectionPoint"/> is within floor and ceiling limits.
        /// <para> 
        /// (<see langword="false"/>, <see langword="false"/>) 
        /// if <paramref name="connectionPoint"/> is outside floor and ceiling limits.</para>
        /// </returns>
        public (bool withinFloor, bool withinCeiling) IsWithinFloorLimits(ConnectionPoint connectionPoint)
        {
            var h2 = connectionPoint.Element.GetSizeByVector(XYZ.BasisZ, connectionPoint.Point);
            var ins = IsInsulationAccount && connectionPoint.Element is MEPCurve mEPCurve ?
                mEPCurve.GetInsulationThickness() : 0;
            var hmin = h2 + ins;
          
            double offsetFromFloor = hmin + TraceSettings.H;
            double offsetFromCeiling = hmin + TraceSettings.B;

            var minHFloor = offsetFromFloor;
            var minHCeiling = offsetFromCeiling;

            XYZ pointFloorBound = connectionPoint.Point.GetXYZBound(_doc, minHFloor, -_distnaceToFindFloor);
            XYZ pointCeilingBound = connectionPoint.Point.GetXYZBound(_doc, minHCeiling, _distnaceToFindFloor);

            return (pointFloorBound is not null, pointCeilingBound is not null);
        }
    }
}
