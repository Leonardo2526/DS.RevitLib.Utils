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
    public class ConnectionPointValidator : IConnectionPointValidator
    {
        private readonly Document _doc;
        private readonly ITraceSettings _traceSettings;
        private readonly IElementCollisionDetector _collisionDetector;

        /// <summary>
        /// Instantiate an object to validate <see cref="ConnectionPoint"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="traceSettings"></param>
        /// <param name="collisionDetector"></param>
        /// <param name="mEPSystemModel"></param>
        /// <param name="docElements"></param>
        /// <param name="linkElementsDict"></param>
        public ConnectionPointValidator(Document doc, ITraceSettings traceSettings, IElementCollisionDetector collisionDetector)
        {
            _doc = doc;
            _traceSettings = traceSettings;
            _collisionDetector = collisionDetector;
        }

        #region Properties

        /// <summary>
        /// System to check validity.
        /// </summary>
        public MEPSystemModel MEPSystemModel { get; set; }

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


        public bool IsInsulationAccount { get; set; }

        public bool CheckFloorLimits { get; set; } = true;

        #endregion

        /// <summary>
        /// Specifies whether point is valid for connection.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if <see cref="MEPSystemModel"/> contatins point and point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        public bool Validate(ConnectionPoint connectionPoint)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(connectionPoint);
            if (!Validator.TryValidateObject(connectionPoint, context, results, true))
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
                return false;
            }
            else { return true; }
        }

        /// <summary>
        /// Get results of validation.
        /// </summary>
        /// <param name="connectionPoint"></param>
        /// <returns>
        /// Errors of validation.
        /// <para>
        /// Empty list if no errors occured.
        /// </para>
        /// </returns>
        public IEnumerable<ValidationResult> GetValidationResults(ConnectionPoint connectionPoint)
        {
            var errors = new List<ValidationResult>();

            if (!GetSystemValidity(connectionPoint))
            { errors.Add(new ValidationResult("Неверная система объекта.")); }

            var collisions = GetCollisions(connectionPoint);
            if (collisions is not null && collisions.Any())
            {
                var report = GetCollisionsReport(collisions);
                errors.Add(new ValidationResult(report));
            }

            if (!IsWithinOutlineLimits(connectionPoint.Point))
            { errors.Add(new ValidationResult("Точка вне зоны решения коллизии.")); }

            if (CheckFloorLimits)
            {
                var (withinFloor, withinCeiling) = IsWithinFloorLimits(connectionPoint);
                if (!withinFloor)
                { errors.Add(new ValidationResult("Расстояние от точки до пола меньше минимального.")); }
                if (!withinCeiling)
                { errors.Add(new ValidationResult("Расстояние от точки до потолка меньше минимального.")); }
            }

            return errors;
        }

        /// <summary>
        /// Specifies whether <see cref="MEPSystemModel"/> contatins point.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if <see cref="MEPSystemModel"/> contatins point.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        private bool GetSystemValidity(ConnectionPoint connectionPoint)
        {
            if (MEPSystemModel is null) { return true; }
            if (connectionPoint.Element is null) { return false; }
            return MEPSystemModel.AllElements.Select(obj => obj.Id).Contains(connectionPoint.Element.Id);
        }

        /// <summary>
        /// Specifies whether point has no collisions.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        private List<(XYZ, Element)> GetCollisions(ConnectionPoint connectionPoint)
        {
            var collisions = new List<(XYZ, Element)>();

            //get collisions in freeCon.
            var elemCollisions = _collisionDetector.GetCollisions(connectionPoint.Element);
            if (!elemCollisions.Any()) { return collisions; }

            if (connectionPoint.Element is FamilyInstance)
            { return collisions; }
            else
            {
                //Specify collision objects on point
                foreach (var c in elemCollisions)
                {
                    if (c.Item2.GetSolidInLink(_doc).Contains(connectionPoint.Point))
                    {
                        var pc = (connectionPoint.Point, c.Item2);
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
        private bool IsWithinOutlineLimits(XYZ point)
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
        private (bool withinFloor, bool withinCeiling) IsWithinFloorLimits(ConnectionPoint connectionPoint)
        {
            (XYZ pointFloorBound, XYZ pointCeilingBound) =
                connectionPoint.GetFloorBounds(_doc, _traceSettings.H, _traceSettings.B, IsInsulationAccount);

            return (pointFloorBound is not null, pointCeilingBound is not null);
        }

        private string GetCollisionsReport(List<(XYZ, Element)> collisions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("В точке не должно быть коллизий.");

            var elements = collisions.Select(c => c.Item2);
            var groups = elements.GroupBy(e => e.Document);

            foreach (var group in groups)
            {
                sb.AppendLine();
                sb.AppendLine($"Модель: '{group.Key.Title}': ");
                foreach (var g in group)
                {
                    sb.AppendLine("  Id: " + g.Id.IntegerValue.ToString());
                }
            }
            sb.Append("\nИтого коллизий: " + collisions.Count);

            return sb.ToString();
        }
    }
}
