using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels.PointModels;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Connections.PointModels
{
    /// <summary>
    /// An object that represents connection point of MEPSystem without specifing objects to connect.
    /// </summary>
    public class ConnectionPoint : IConnectionPoint, IValidatableObject
    {
        /// <summary>
        /// Instantiate an object to create connection point of MEPSystem without specifing objects to connect.
        /// </summary>
        ///  <param name="element">Element on connection point.</param>
        /// <param name="point">Joint point</param>
        public ConnectionPoint(Element element, XYZ point)
        {
            Element = element;
            Point = point;
        }

        /// <inheritdoc/>
        public XYZ Point { get; protected set; }

        /// <summary>
        /// Element on connection point.
        /// </summary>
        public Element Element { get; }

        /// <summary>
        /// <see cref="Element"/> partType.
        /// </summary>
        public PartType PartType
        {
            get
            {
                FamilyInstance fam = Element is FamilyInstance ? Element as FamilyInstance : null;
                return fam is null ? PartType.Undefined : ElementUtils.GetPartType(fam);
            }
        }


        private bool _isValid = true;
        /// <inheritdoc/>
        public bool IsValid { get => _isValid= Validate(); set => _isValid = value; } 

        /// <summary>
        /// Validator for point properties.
        /// </summary>
        public IConnectionPointValidator Validator { get; set; }

        /// <summary>
        /// Direction to connect point.
        /// </summary>
        public XYZ Direction { get; private set; }

        /// <summary>
        /// (<see cref="Autodesk.Revit.DB.XYZ"/> , <see cref="Autodesk.Revit.DB.XYZ"/>) 
        /// that specify <see cref="Autodesk.Revit.DB.XYZ"/>'s on floor and ceiling that are closest to <see cref="ConnectionPoint"/>.
        /// </summary>
        public (XYZ pointFloorBound, XYZ pointCeilingBound) FloorBounds { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => Validator?.GetValidationResults(this);

        /// <summary>
        /// Specifies whether point is valid for connection.
        /// </summary>
        /// <param name="mEPSystemModel"></param>
        /// <param name="docElements"></param>
        /// <param name="linkElementsDict"></param>
        /// <returns>Returns <see langword="true"></see> if <paramref name="mEPSystemModel"/> contatins point and point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        private bool Validate()
        {
            if(Validator is null) { return true; }         
            if (Element is null) { return false; }
            return Validator.Validate(this);
        }

        /// <summary>
        /// Get direction by <paramref name="refPoint"/> of <paramref name="refElement"/>.
        /// </summary>
        /// <param name="refPoint"></param>
        /// <param name="refElement"></param>
        /// <param name="isManualDir"></param>
        /// <param name="objectsToExclude"></param>
        /// <param name="uIDocument"></param>
        /// <remarks>
        /// Specify <paramref name="uIDocument"/> if get direction manually should be enabled.
        /// </remarks>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> direction to connect <see cref="Element"/> at <see cref="Point"/>.
        /// <para>
        /// <see langword="null"/> if no direction was found.
        /// </para>
        /// </returns>
        public XYZ GetDirection(XYZ refPoint, Element refElement, out bool isManualDir, IEnumerable<Element> objectsToExclude = null, 
            UIDocument uIDocument = null)
        {
            isManualDir = false;
            var mc = Element is MEPCurve curve ? curve : Element.GetBestConnected().OfType<MEPCurve>().FirstOrDefault();
            if (mc is null) { return null; }
            var factory = new ConnectionDirectionFactory(Point, mc, uIDocument);
            Direction = factory.GetDirection(refPoint, refElement);
            isManualDir = factory.IsManualDir;
            return Direction;
        }

        /// <summary>
        /// Get <see cref="MEPCurve"/> to connect.
        /// </summary>
        /// <param name="excluededIds"></param>
        /// <returns></returns>
        public MEPCurve GetMEPCurve(IEnumerable<ElementId> excluededIds = null)
        {
            if (Element is MEPCurve curve) { return curve; }

            var connectedMEPCurves = ConnectorUtils.GetConnectedElements(Element)?.Where(e => e is MEPCurve);
            if (connectedMEPCurves is null || !connectedMEPCurves.Any()) { return null; }

            if (excluededIds is null || !excluededIds.Any()) { return connectedMEPCurves.FirstOrDefault() as MEPCurve; }
            var noExeptionIds = connectedMEPCurves.Select(e => e.Id).Except(excluededIds);

            return Element.Document.GetElement(noExeptionIds.FirstOrDefault()) as MEPCurve;
        }


        /// <summary>
        /// Get (<see cref="Autodesk.Revit.DB.XYZ"/> , <see cref="Autodesk.Revit.DB.XYZ"/>) 
        /// that specify <see cref="Autodesk.Revit.DB.XYZ"/>'s on floor and ceiling that are closest to <see cref="ConnectionPoint"/>.
        /// </summary>
        /// <returns>
        /// (<see cref="Autodesk.Revit.DB.XYZ"/> , <see cref="Autodesk.Revit.DB.XYZ"/>)
        /// if <see cref="ConnectionPoint"/> is within floor and ceiling limits.
        /// <para> 
        /// (<see langword="null"/>, <see langword="null"/>) 
        /// if <see cref="ConnectionPoint"/> is outside floor and ceiling limits.</para>
        /// </returns>
        public (XYZ pointFloorBound, XYZ pointCeilingBound) GetFloorBounds(
            Document doc,
            double minDistToFloor, double minDistToCeiling,
            bool isInsulationAccount = true, 
            int distnaceToFindFloor = 30)
        {
            var pointElement = (Element, Point);
            return FloorBounds = 
                pointElement.GetFloorBounds(doc, minDistToFloor, minDistToCeiling, 
                isInsulationAccount, distnaceToFindFloor);
        }
    }
}
