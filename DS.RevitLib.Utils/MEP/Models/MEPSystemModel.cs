using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Models
{
    public class MEPSystemModel : MEPSystemComponent
    {

        public MEPSystemModel(Element baseElement) : base(baseElement)
        {
        }

        public List<MEPSystemComponent> MEPSystemComponents { get; private set; } = new List<MEPSystemComponent>();
        public List<Element> AllElements
        {
            get
            {
                return MEPSystemComponents.SelectMany(x => x.Elements).ToList();
            }
        }


        public void Add(MEPSystemComponent c)
        {
            MEPSystemComponents.Add(c);
        }
    }
}
