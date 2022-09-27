using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Points.Models
{
    public class MEPConnectionPoint : IMEPConnectionPoint
    {
        public MEPConnectionPoint(XYZ point, Element element)
        {
            Point = point;
            Element = element;
        }
        public XYZ Point { get; private set; }
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
