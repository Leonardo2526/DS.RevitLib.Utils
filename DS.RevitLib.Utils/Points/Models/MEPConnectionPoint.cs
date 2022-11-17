using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.FamilyInstances;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Points.Models
{
    /// <summary>
    /// Class for object to describe a point for element's connection.
    /// </summary>
    public class MEPConnectionPoint : IMEPConnectionPoint
    {
        /// <summary>
        /// Initiate a new object for element connection.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="element"></param>
        public MEPConnectionPoint(XYZ point, Element element)
        {
            Point = point;
            Element = element;
        }

        /// <summary>
        /// Point of connection to element.
        /// </summary>
        public XYZ Point { get; private set; }

        /// <summary>
        /// Element to connect.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// Element's partType.
        /// </summary>
        public PartType PartType
        {
            get
            {
                FamilyInstance fam = Element is FamilyInstance ? Element as FamilyInstance : null;
                return fam is null ? PartType.Undefined : ElementUtils.GetPartType(fam);
            }
        }

        /// <summary>
        /// Get MEPCurve for connection to current point.
        /// </summary>
        /// <returns></returns>
        public MEPCurve GetConnectionMEPCurve(IEnumerable<ElementId> deletedIds)
        {
            if (Element is MEPCurve)
            {
                return Element as MEPCurve;
            }

            var famInst = Element is FamilyInstance ? Element as FamilyInstance : null;
            var (parents, child) = famInst.GetConnectedElements();

            var mParents = parents.OfType<MEPCurve>().ToList();

            if (PartType == PartType.Elbow)
            {
                foreach (var item in mParents)
                {
                    if (!deletedIds.Contains(item.Id)) { return item; }
                }
            }

            return parents.First() as MEPCurve;
        }
    }
}
