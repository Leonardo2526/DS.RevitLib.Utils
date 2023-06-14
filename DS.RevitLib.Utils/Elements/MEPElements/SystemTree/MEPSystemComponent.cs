using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.RevitLib.Utils.Connection;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    /// <summary>
    /// Leaf
    /// </summary>
    public class MEPSystemComponent : Component
    {
        #region Properties

        public Element BaseElement { get; set; }

        public List<MEPCurve> MEPCurves
        {
            get
            {
                return Elements.Where(x => x is MEPCurve).
                    Select(x => x as MEPCurve).ToList();
            }
        }

        public List<FamilyInstance> FamilyInstances
        {
            get
            {
                return Elements.OfType<FamilyInstance>().ToList();
            }
        }

        public List<FamilyInstance> Accessories
        {
            get
            {
                return Elements.Where(x =>
                x.Category.Name.Contains("Accessories") || x.Category.Name.Contains("Арматура")).
                    OfType<FamilyInstance>().ToList();
            }
        }

        public List<FamilyInstance> Fittings
        {
            get
            {
                return Elements.Where(x =>
                x.Category.Name.Contains("Fittings") || x.Category.Name.Contains("Соединительные")).
                    OfType<FamilyInstance>().ToList();
            }
        }

        public List<Element> Elements { get; set; }

        public List<NodeElement> ChildrenNodes { get; set; }

        public List<NodeElement> ParentNodes { get; set; }

        /// <summary>
        /// Specify whether all <see cref="Elements"/> are valid objects. 
        /// </summary>
        public bool IsValid
        {
            get { return Elements.TrueForAll(obj => obj.IsValidObject); }
        }

        /// <summary>
        /// Specify whether all not spud <see cref="Elements"/> are valid objects. 
        /// </summary>
        public bool HasNoBreaks
        {
            get
            {
                var validElem = Elements.FirstOrDefault(obj => obj.IsValidObject && !MEPElementUtils.IsNodeElement(obj));
                MEPSystemComponent newComp = new ComponentBuilder(validElem).Build();
                var newElemnetsIds = newComp.Elements.Select(obj => obj.Id);

                var oldElementsIds = Elements.Where(x => x.IsValidObject).Select(obj => obj.Id).ToList();
                oldElementsIds.TrueForAll(obj => newElemnetsIds.Contains(obj));

                return oldElementsIds.TrueForAll(obj => newElemnetsIds.Contains(obj));
            }
        }

        #endregion

        /// <summary>
        /// Get accessories from list between gived points.
        /// </summary>
        /// <param name="accessories"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>Returns accessories from list between gived points by location points coincidence.</returns>
        public List<FamilyInstance> GetAccessories(List<FamilyInstance> accessories, XYZ point1, XYZ point2)
        {
            var elemsIds = Accessories.Select(obj => obj.Id).ToList();

            FamilyInstance famInst1 = Get(accessories, point1);
            FamilyInstance famInst2 = Get(accessories, point2);

            int ind1 = elemsIds.IndexOf(famInst1.Id);
            int ind2 = elemsIds.IndexOf(famInst2.Id);

            int minInd = Math.Min(ind1, ind2);
            int maxInd = Math.Max(ind1, ind2);

            var range = Accessories.FindAll(x => Accessories.IndexOf(x) > minInd && Accessories.IndexOf(x) < maxInd);

            return range.ToList();
        }

        /// <summary>
        /// Get elements span of current component.
        /// </summary>
        /// <param name="elem1"></param>
        /// <param name="elem2"></param>
        /// <param name="includeEdge">Check if include or not <paramref name="elem1"/> and <paramref name="elem2"/> to result.</param>
        /// <returns>Returns list of elements from <paramref name="elem1"/> to <paramref name="elem2"/>.</returns>
        public List<Element> GetElements(Element elem1, Element elem2, bool includeEdge = true)
        {
            if (!IsValid) { return null; }
            var elemsIds = Elements.Select(obj => obj.Id).ToList();

            int ind1 = elemsIds.IndexOf(elem1.Id);
            int ind2 = elemsIds.IndexOf(elem2.Id);

            int minInd = Math.Min(ind1, ind2);
            int maxInd = Math.Max(ind1, ind2);

            var range = includeEdge ?
                Elements.FindAll(x => Elements.IndexOf(x) >= minInd && Elements.IndexOf(x) <= maxInd) :
                Elements.FindAll(x => Elements.IndexOf(x) > minInd && Elements.IndexOf(x) < maxInd);
            List<ElementId> rangeIds = range.Select(obj => obj.Id).ToList();
            var mEPCurveIds = Elements.OfType<MEPCurve>().Select(obj => obj.Id).ToList();

            //spud correction
            (MEPCurve mePCurve1, int mepInd1) = GetSpudMEPCurveToInsert(elem1, rangeIds, mEPCurveIds);
            if (mePCurve1 is not null)
            {
                range.Insert(mepInd1, mePCurve1);
            }
            (MEPCurve mePCurve2, int mepInd2) = GetSpudMEPCurveToInsert(elem2, rangeIds, mEPCurveIds);
            if (mePCurve2 is not null)
            {
                if (mePCurve1 is not null && mePCurve2.Id == mePCurve1.Id) { }
                else if(mepInd2 > 0)
                {
                    range.Insert(mepInd2, mePCurve2);
                }
            }

            return range;
        }

        /// <summary>
        /// Get MEPCurve connected to child spud if range doesn't contain it yet.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="rangeIds"></param>
        /// <param name="mEPCurveIds"></param>
        /// <returns></returns>
        private (MEPCurve mePCurve, int ind) GetSpudMEPCurveToInsert(Element element,
            List<ElementId> rangeIds, List<ElementId> mEPCurveIds)
        {
            var partType = element is FamilyInstance ? ElementUtils.GetPartType(element as FamilyInstance) : PartType.Undefined;

            if (partType == PartType.SpudPerpendicular || partType == PartType.SpudAdjustable ||
                partType == PartType.TapPerpendicular || partType == PartType.TapAdjustable)
            {
                var childIds = ChildrenNodes.Select(obj => obj.Element.Id).ToList();
                if (!childIds.Contains(element.Id))
                {
                    return (null, 0);
                }

                List<MEPCurve> connectedMEPCurves = ConnectorUtils.GetConnectedElements(element).OfType<MEPCurve>().ToList();
                var connectedMEPCurvesIds = connectedMEPCurves.Select(obj => obj.Id);
                var intersectionId = connectedMEPCurvesIds.Intersect(mEPCurveIds).First();

                if (!rangeIds.Contains(intersectionId))
                {
                    var ind = rangeIds.IndexOf(element.Id);
                    return (element.Document.GetElement(intersectionId) as MEPCurve, ind);
                }
            }

            return (null, 0);
        }

        /// <summary>
        /// Get familyInstance from list which location point is closest to given point.
        /// </summary>
        /// <param name="familyInstances"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private FamilyInstance Get(List<FamilyInstance> familyInstances, XYZ point)
        {
            foreach (var item in familyInstances)
            {
                var lp = ElementUtils.GetLocationPoint(item);
                if (Math.Round(lp.DistanceTo(point), 3) == 0)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all spuds connected to gived MEPCurve
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public List<FamilyInstance> GetConnectedSpuds(MEPCurve mEPCurve)
        {
            if (ChildrenNodes is null || !ChildrenNodes.Any())
            {
                return null;
            }

            var spudsToBase = new List<FamilyInstance>();
            foreach (var child in ChildrenNodes)
            {
                if (child.Element.IsSpud() && ConnectorUtils.ElementsConnected(child.Element, mEPCurve))
                {
                    spudsToBase.Add(child.Element);
                }
            }

            return spudsToBase;
        }


        /// <summary>
        /// Get elements from Elements list by connector of BaseElement.
        /// </summary>
        /// <param name="baseElemCon"></param>
        /// <returns>Returns list of elements from BaseElement with baseElemCon direction to edge element. 
        /// BaseElement is not included.</returns>
        public List<Element> GetElementsSpan(Connector baseElemCon)
        {
            if (baseElemCon.Owner.Id != BaseElement.Id)
            {
                throw new ArgumentException("Connector owner is not BaseElement.");
            }

            var connectedElem = ConnectorUtils.GetConnectedByConnector(baseElemCon, BaseElement);
            if (connectedElem is null)
                return null;

            int dInd = FindIndex(BaseElement) - FindIndex(connectedElem);
            if (dInd > 0)
            {
                var elems = GetElements(Elements.First(), connectedElem);
                elems.Reverse();
                return elems;
            }
            else
            {
                return GetElements(connectedElem, Elements.Last());
            }
        }

        /// <summary>
        /// Find index of element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Returns index of element in current list of Elements.</returns>
        public int FindIndex(Element element)
        {
            return Elements.FindIndex(obj => obj.Id == element.Id);
        }

        /// <summary>
        /// Try to find <paramref name="element"/> in <see cref="ChildrenNodes"/> and <see cref="ParentNodes"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Returns <see cref="NodeElement"/> if it was found. Otherwise returns null.</returns>
        public NodeElement FindNode(Element element)
        {
            NodeElement nodeElement = null;
            if (ChildrenNodes is not null && ChildrenNodes.Any())
            {
                nodeElement = ChildrenNodes.FirstOrDefault(node => node.Element.Id == element.Id);
                if (nodeElement is not null) { return nodeElement; }
            }
            if (ParentNodes is not null && ParentNodes.Any())
            {
                nodeElement = ParentNodes.FirstOrDefault(node => node.Element.Id == element.Id);
                if (nodeElement is not null) { return nodeElement; }
            }
            return nodeElement;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is MEPSystemComponent component &&
                   EqualityComparer<ElementId>.Default.Equals(BaseElement.Id, component.BaseElement.Id);
        }

        private void ConnectElements((Element, Element) elementsToConnect) =>
            new MEPCurveConnectionFactory(elementsToConnect.Item1.Document,
                elementsToConnect.Item1 as MEPCurve,
                elementsToConnect.Item2 as MEPCurve).Connect();

        /// <summary>
        /// Fix component by replacing not valid elements with MEPCurve.
        /// </summary>
        /// <param name="trb"></param>
        public void Fix(AbstractTransactionBuilder trb = null)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                if (!Elements[i].IsValidObject && i > 0 && i < Elements.Count)
                {
                    var elementsToConnect = (Elements[i - 1], Elements[i + 1]);
                    if (trb is null) { ConnectElements(elementsToConnect); }
                    else { trb.Build(() => ConnectElements(elementsToConnect), "Fix component"); }
                    break;
                }
            }
        }
    }
}
