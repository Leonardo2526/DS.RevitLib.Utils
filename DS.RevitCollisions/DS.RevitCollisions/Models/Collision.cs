using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Models
{
    /// <summary>
    /// Represents intersection between two <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public abstract class Collision : INotifyPropertyChanged, IElementCollision
    {
        private CollisionStatus _status = CollisionStatus.ToResolve;
        private AbstractElementModel _stateElementModel;
        private AbstractElementModel _resolvingElementModel;
        private SolidModel _intersectionSolidWithInsulation;

        /// <summary>
        /// Instantiate an object that represents intersection between two <see cref="Autodesk.Revit.DB.Element"/>'s.
        /// </summary>
        /// <param name="stateElementModel"></param>
        /// <param name="resolvingElementModel"></param>
        /// <param name="collisionSolid"></param>
        public Collision(AbstractElementModel resolvingElementModel, AbstractElementModel stateElementModel,
            Solid collisionSolid)
        {
            StateElementModel = stateElementModel;
            ResolvingElementModel = resolvingElementModel;
            ResolvingElemId = ResolvingElementModel.Element.Id;
            StateElemId = StateElementModel.Element.Id;
            IntersectionSolid = new SolidModel(collisionSolid);

            //(Line LineToBot, Element BotElem, Line LineToTop, Element TopElem) = FloorFinder.FindFloorAndCeiling(collisionSolid);
            //MaxZCoordinate = LineToTop?.GetEndPoint(1).Z ?? 10000 - MainParameters.Clearance;
            //MinZCoordinate = LineToBot?.GetEndPoint(1).Z ?? -10000 + MainParameters.ClearanceFloor.mmToFyt2();
        }

        delegate List<Solid> IntersectionSolidDelegate(IEnumerable<(Element, Element)> collisions);

        /// <summary>
        /// Rise event on properties changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        #region Properties

        /// <inheritdoc/>
        public CollisionStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                //if (_status == CollisionStatus.Resolved || _status == CollisionStatus.Unresolved)
                //{ HttpLog.Post(GetAccountDoc(DocModel.GetInstance())); }
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <inheritdoc/>
        public Element Item2
        {
            get { return StateElementModel.Element; }
        }

        /// <inheritdoc/>
        public Element Item1
        {
            get { return ResolvingElementModel.Element; }
        }

        /// <summary>
        /// Id of <see cref="Item1"/>.
        /// </summary>
        public ElementId ResolvingElemId { get; protected set; }

        /// <summary>
        /// Id of <see cref="Item2"/>.
        /// </summary>
        public ElementId StateElemId { get; protected set; }

        /// <summary>
        /// Collision id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Intersection <see cref="Solid"/> between <see cref="Item1"/> and <see cref="Item2"/>.
        /// </summary>
        public SolidModel IntersectionSolid { get; }

        /// <summary>
        /// Intersection <see cref="Solid"/> between <see cref="Item1"/> and <see cref="Item2"/>
        /// with insulation account.
        /// </summary>
        public SolidModel IntersectionSolidWithInsulation
        {
            get
            {
                return _intersectionSolidWithInsulation is null ?
                   _intersectionSolidWithInsulation =
                   new SolidModel((Item1, Item2).GetIntersectionSolidWithInsulation()) :
                    _intersectionSolidWithInsulation;
            }
        }

        /// <summary>
        /// Model of abstract base element with state position
        /// </summary>
        public AbstractElementModel StateElementModel
        {
            get { return _stateElementModel; }
            protected set
            {
                _stateElementModel = value;
                OnPropertyChanged("StateElem");
            }
        }

        /// <summary>
        /// Model of abstract base element with changeable position
        /// </summary>
        public AbstractElementModel ResolvingElementModel
        {
            get { return _resolvingElementModel; }
            protected set
            {
                _resolvingElementModel = value;
                OnPropertyChanged("ResolvingElem");
            }
        }

        /// <summary>
        /// Naximum Z from floor to element's top (ceilng bottom)
        /// </summary>
        public double MaxZCoordinate { get; private set; }

        /// <summary>
        /// Minimum Z from floor to element's bottom
        /// </summary>
        public double MinZCoordinate { get; private set; }

        /// <summary>
        /// Visualizator used to show collisions in document.
        /// </summary>
        public ICollisionVisualizator<Collision> Visualizator { get; set; }

        /// <summary>
        /// Specifies if all elements of <see cref="Collision"/> are valid objects.
        /// </summary>
        public abstract bool IsValid { get; }

        /// <summary>
        /// Specified if <see cref="Item1"/> and <see cref="Item2"/> still have intersection.
        /// </summary>
        public abstract bool HaveIntersection { get; }

        #endregion


        #region Methods

        /// <summary>
        /// Rise an event when propery changed.
        /// </summary>
        /// <param name="prop"></param>
        protected void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Specifies if current collision is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// <see langword="true"/> if <see cref="StateElemId"/> and <see cref="ResolvingElemId"/> are equals.
        /// <para>
        /// <see langword="true"/> if <see cref="IntersectionGroupSolid"/>'s are equals.
        /// </para>
        /// <para>
        /// Otherwise returns <see langword="false"/>.       
        /// </para>
        /// </returns>
        public override bool Equals(object obj)
        {
            var collision = obj as Collision;
            if (collision == null) return false;

            bool comparator1()
            {
                return StateElemId.IntegerValue == collision.StateElemId.IntegerValue &&
                    ResolvingElemId.IntegerValue == collision.ResolvingElemId.IntegerValue;
            }

            bool comparator2()
            {
                return StateElemId.IntegerValue == collision.ResolvingElemId.IntegerValue &&
                   ResolvingElemId.IntegerValue == collision.StateElemId.IntegerValue;
            }

            bool comparator3()
            {
                XYZ deltaCenter = IntersectionSolid.Center - collision.IntersectionSolid.Center;
                var deltaVolume = IntersectionSolid.Solid.Volume - collision.IntersectionSolid.Solid.Volume;
                return deltaCenter.IsZeroLength() && Math.Round(deltaVolume, 5) == 0;
            }

            return comparator1() || comparator2() || comparator3();
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 803821747;
            hashCode = hashCode * -1521134295 + EqualityComparer<Element>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<Element>.Default.GetHashCode(Item1);
            return hashCode;
        }

        /// <summary>
        /// Set <see cref="Item1"/> as <see cref="Item2"/>.
        /// </summary>
        /// <returns>
        /// <see cref="Collision"/> with swapped elements.
        /// <para>
        /// <see langword="null"/> if swapping was failed.
        /// </para>
        /// </returns>
        public abstract Collision SwapElements();

        /// <inheritdoc/>
        public void Show()
        {
            Visualizator?.Show(this);
        }

        /// <summary>
        /// Update current collison status.
        /// </summary>
        /// <param name="status"></param>
        public void UpdateStatus(CollisionStatus status)
        {
            this.Status = status;
        }

        /// <summary>
        /// Get solids of intersections all elements that have collisons with resolving element.
        /// </summary>
        /// <param name="insulationAccount"></param>
        /// <returns>
        /// Group intersection solid model with insulation account.
        /// </returns>
        public SolidModel GetIntersectionNeighborSolid(bool insulationAccount, IElementCollisionDetector collisionDetector)
        {
            var elementsToExclude = new List<Element>() { Item2 };
            var connectedElements = ConnectorUtils.GetConnectedElements(Item1, true);
            elementsToExclude.AddRange(connectedElements);

            collisionDetector.ExcludedElements = elementsToExclude;
            var collisions = collisionDetector.GetCollisions(Item1).
                Where(c => c.Item2 is not InsulationLiningBase);
            //var collisions = _factory.GetCollisions(ResolvingElem, elementsToExclude).
            //    Where(c => c.Item2 is not InsulationLiningBase);

            var solids = GetSolids(insulationAccount);

            var resolvingMEPCurveModel = ResolvingElementModel as MEPCurveModel;
            if (resolvingMEPCurveModel != null)
            {
                var maxDist = 10;
                //var maxDist = 50 * Math.Max(resolvingMEPCurveModel.Width, resolvingMEPCurveModel.Height);
                solids = solids.
                    Where(s => s.ComputeCentroid().DistanceTo(IntersectionSolid.Center) < maxDist).ToList();
            }

            var solid = DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(solids, 1);

            return new SolidModel(solid);

            List<Solid> GetSolids(bool insulationAccount)
            {
                List<Solid> solids = new List<Solid>();
                IntersectionSolidDelegate intersectionDelegate = null;
                if (insulationAccount)
                {
                    intersectionDelegate = (col) =>
                    {
                        solids = col.Select(c => c.GetIntersectionSolidWithInsulation()).ToList();
                        solids.Add(IntersectionSolidWithInsulation.Solid);

                        return solids;
                    };
                }
                else
                {
                    intersectionDelegate = (col) =>
                    {
                        solids = col.Select(c => c.GetIntersectionSolid()).ToList();
                        solids.Add(IntersectionSolid.Solid);

                        return solids;
                    };
                }

                intersectionDelegate.Invoke(collisions);

                return solids;
            }
        }

        #endregion

    }
}
