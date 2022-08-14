using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
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
                return (List<FamilyInstance>)Elements.OfType<FamilyInstance>();
            }
        }

        public List<Element> Elements { get; set; }

        public List<NodeElement> ChildrenNodes { get; set; }

        public List<NodeElement> ParentNodes { get; set; }


        #endregion

    }
}
