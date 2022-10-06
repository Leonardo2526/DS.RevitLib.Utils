using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.FamilyInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

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
        public MEPCurve GetConnectionMEPCurve(List<XYZ> path)
        {
            if (Element is MEPCurve)
            {
                return Element as MEPCurve;
            }

            var famInst = Element is FamilyInstance ? Element as FamilyInstance : null;
            var (parents, child) = famInst.GetConnectedElements();
            return parents.First() as MEPCurve;

            var mParents = parents.OfType<MEPCurve>().ToList();

            if (PartType==PartType.Elbow)
            {
                XYZ nextDir = path[2] - path[1];
                bool firstCollinerity = XYZUtils.Collinearity(nextDir, mParents.First().GetCenterLine().Direction);
                return firstCollinerity ? mParents.First() : mParents.Last();
            }

            if (child is null)
            {
                return mParents.First();
            }
            //get elements lines
            //var parentLines = new List<Line>();
            //parents.ForEach(p => parentLines.Add(p.GetCenterLine()));

            var parentdbLine = parents.First().GetCenterLine();
            var parentLine = Line.CreateUnbound(parentdbLine.Origin, parentdbLine.Direction);

            var childbLine = child.GetCenterLine();
            var childLine = Line.CreateUnbound(childbLine.Origin, childbLine.Direction);

            XYZ parentProjectPoint = parentLine.Project(Point).XYZPoint;
            XYZ childProjectPoint = childLine.Project(Point).XYZPoint;

            var elem = (parentProjectPoint - childProjectPoint).IsZeroLength() ? child : mParents.First();

            return elem as MEPCurve;
        }

    }
}
