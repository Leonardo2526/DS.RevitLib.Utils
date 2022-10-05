using Autodesk.Revit.DB;
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
    }
}
