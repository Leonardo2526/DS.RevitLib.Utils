using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
                { results.ForEach(r => Messenger.Show(r.ErrorMessage, "Ошибка")); }
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
        /// Specifies if <paramref name="point"/> is within <see cref="BoundOutline"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="point"/> is within <see cref="BoundOutline"/>.
        /// <para>
        /// 
        /// Othewise returns <see langword="false"/>.</para>
        /// </returns>
        public bool IsWithinLengthLimits(XYZ point)
        {
            if (BoundOutline is null) { return true; }
            else
            {
                return point.More(BoundOutline.MinimumPoint) && point.Less(BoundOutline.MaximumPoint);
            }
        }
    }
}
