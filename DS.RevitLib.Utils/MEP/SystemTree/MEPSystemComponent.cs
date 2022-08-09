using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    /// <summary>
    /// Leaf
    /// </summary>
    public class MEPSystemComponent : Component
    {
        #region Properties

        public List<MEPCurve> MEPCurves
        {
            get
            {
                return Elements.Where(x => ElementUtils.IsElementMEPCurve(x)).
                    Select(x => x as MEPCurve).ToList();
            }
        }

        public List<Element> Elements { get; set; }

        public List<FamilyInstance> ChildNodes { get; set; }

        public Element ParentNode1 { get; set; }

        public Element ParentNode2{ get; set; }


        #endregion

    }
}
