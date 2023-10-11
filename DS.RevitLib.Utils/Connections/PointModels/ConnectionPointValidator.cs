using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Creation.Transactions;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;

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
        private List<Element> _docElements;
        private Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;

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
                foreach (var error in results)
                {
                    TransactionFactory?.CreateAsync(() =>
                    TaskDialog.Show("Ошибка", error.ErrorMessage), "show message");
                }
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
        public List<(Element, Element)> GetCollisions()
        {
            //get collisions in freeCon.
            var collisions = _collisionDetector.GetCollisions(ConnectionPoint.Element);
            if (!collisions.Any()) { return collisions; }

            if (ConnectionPoint.Element is FamilyInstance)
            { return collisions; }
            else
            {
                //Specify collision objects on point
                var collisionsOnPoint = collisions.
                    Where(obj => obj.Item2.GetSolidInLink(_doc).Contains(ConnectionPoint.Point)).ToList();
                return collisionsOnPoint;
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
