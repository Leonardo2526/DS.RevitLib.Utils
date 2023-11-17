using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Lines;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System.Diagnostics;

namespace DS.RevitCollisions.Models
{
    /// <summary>
    /// Represents intersection between two <see cref="Autodesk.Revit.DB.MEPCurve"/>'s.
    /// </summary>
    public class MEPCollision : Collision
    {
        private LineOverlapResult? _overlapResult;
        private MEPSystemModel _resolvigMEPSystem;
        private XYZ _basisVectorOnPlane;


        /// <summary>
        /// Instantiate an object that represents intersection between two <see cref="Autodesk.Revit.DB.MEPCurve"/>'s.
        /// </summary>
        /// <param name="stateElementModel"></param>
        /// <param name="resolvingElementModel"></param>
        /// <param name="collisionSolid"></param>
        /// <param name="planeModel"></param>
        public MEPCollision(AbstractElementModel stateElementModel, MEPCurveModel resolvingElementModel,
            Solid collisionSolid) :
            base(stateElementModel, resolvingElementModel, collisionSolid)
        {
            var basis = ElementUtils.GetBasis(resolvingElementModel.MEPCurve, stateElementModel.Element);
            Basis = new Basis(basis.basisX, basis.basisY, basis.basisZ, IntersectionSolid.Center);
        }

        #region Properties

        public MEPSystemModel ResolvigMEPSystem
        {
            get
            {
                if (_resolvigMEPSystem == null)
                { _resolvigMEPSystem = new SimpleMEPSystemBuilder(ResolvingElem).Build(); }
                return _resolvigMEPSystem;
            }
        }

        public MEPCurveModel ResolvingMEPCurveModel
        {
            get { return (MEPCurveModel)ResolvingElementModel; }
        }

        public Basis Basis { get; }

        /// <summary>
        /// Collision overlap relationship type.
        /// </summary>
        public LineOverlapResult? OverlapResult
        {
            get
            {
                if (_overlapResult == null)
                {
                    if (StateElementModel.Element is MEPCurve stateMEPCurve)
                    { _overlapResult = LineUtils.GetOverlapResult(ResolvingMEPCurveModel.Line, stateMEPCurve.GetCenterLine()); }
                    else
                    { _overlapResult = LineOverlapResult.None; }
                }
                return _overlapResult;
            }
        }


        public XYZ BasisVectorOnPlane
        {
            get
            {
                _basisVectorOnPlane ??= GetBasisVectorOnPlane();
                return _basisVectorOnPlane;
            }
        }

        /// <inheritdoc/>
        public override bool IsValid
        {
            get
            {
                return
                    ResolvingElem.IsValidObject &&
                    StateElem.IsValidObject &&
                    ResolvigMEPSystem.Root.IsValid;
            }
        }

        /// <inheritdoc/>
        public override bool HaveIntersection
        {
            get
            {
                if (!IsValid) { return false; }

                var intersectionSolid = CollisionUtils.
                    GetIntersectionSolid(ResolvingElem, StateElem, out Solid elem1Solid, out Solid elem2Solid);
                return intersectionSolid is not null;
            }
        }

        #endregion


        #region Methods

        /// <inheritdoc/>
        public override Collision SwapElements()
        {
            var newResolvingElementModel = new MEPCurveModel((MEPCurve)StateElementModel.Element, StateElementModel.SolidModel);

            (this.ResolvingElementModel, this.StateElementModel) = (newResolvingElementModel, this.ResolvingElementModel);

            this.ResolvingElemId = ResolvingElementModel.Element.Id;
            this.StateElemId = StateElementModel.Element.Id;

            _resolvigMEPSystem = null;

            OnPropertyChanged(nameof(ResolvingElem));
            OnPropertyChanged(nameof(StateElem));

            return this;
        }

        public MEPSystemModel RestoreMEPSystem(Document document)
        {
            var elem = document.GetElement(ResolvingElemId);
            if (elem == null) { Debug.WriteLine("Error: Unable to restore resolvigMEPSystem."); return null; }
            ResolvingElementModel = new MEPCurveModel((MEPCurve)elem, new SolidModel(ElementUtils.GetSolid(elem)));
            return _resolvigMEPSystem = new SimpleMEPSystemBuilder(elem).Build();
        }


        private XYZ GetBasisVectorOnPlane()
        {
            XYZ basisVectorOnPlane = null;
            //var basisVectorOnPlane = MEPCurveUtils.GetDirection(ResolvingMEPCurveModel.MEPCurve).Normalize();
            var basis = this.Basis;
            var XYplane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
            basisVectorOnPlane ??= basis.GetBasisVectorOnPlane(XYplane);
            basisVectorOnPlane ??= XYZ.BasisX;

            return basisVectorOnPlane;
        }




        #endregion

    }
}
