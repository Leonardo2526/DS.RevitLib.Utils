using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                return Elements.Where(x => ElementUtils.IsElementMEPCurve(x)).
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

        public List<Element> GetElements(Element elem1, Element elem2)
        {
            var elemsIds = Elements.Select(obj => obj.Id).ToList();

            int ind1 = elemsIds.IndexOf(elem1.Id);
            int ind2 = elemsIds.IndexOf(elem2.Id);

            int minInd = Math.Min(ind1, ind2);
            int maxInd = Math.Max(ind1, ind2);

            var range = Elements.FindAll(x => Elements.IndexOf(x) >= minInd && Elements.IndexOf(x) <= maxInd);

            return range;
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
            if(baseElemCon.Owner.Id != BaseElement.Id)
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
    }
}
