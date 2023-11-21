using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
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
    public class MEPCollision : Collision, IMEPCollision
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
        public MEPCollision(MEPCurveModel resolvingElementModel, AbstractElementModel stateElementModel,
            Solid collisionSolid) :
            base(resolvingElementModel, stateElementModel, collisionSolid)
        {
            var basis = ElementUtils.GetBasis(resolvingElementModel.MEPCurve, stateElementModel.Element);
            Basis = new Basis(basis.basisX, basis.basisY, basis.basisZ, IntersectionSolid.Center);
        }

        #region Properties

        public MEPCurveModel Item1Model
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
                    { _overlapResult = LineUtils.GetOverlapResult(Item1Model.Line, stateMEPCurve.GetCenterLine()); }
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
                    Item1.IsValidObject &&
                    Item2.IsValidObject;
            }
        }

        /// <inheritdoc/>
        public override bool HaveIntersection
        {
            get
            {
                if (!IsValid) { return false; }

                var intersectionSolid = CollisionUtils.
                    GetIntersectionSolid(Item1, Item2, out Solid elem1Solid, out Solid elem2Solid);
                return intersectionSolid is not null;
            }
        }

        MEPCurve ICollision<MEPCurve, Element>.Item1 => Item1 as MEPCurve;

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

            OnPropertyChanged(nameof(Item1));
            OnPropertyChanged(nameof(Item2));

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
