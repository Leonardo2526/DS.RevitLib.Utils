using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class MEPSystemBuilder
    {
        private readonly Element _element;

        public MEPSystemBuilder(Element element)
        {
            this._element = element;
        }

        //public Stack<Element> OwnStack { get; set; } = new Stack<Element>();
        public Stack<Element> ParentStack { get; set; } = new Stack<Element>();
        public Stack<Element> ChildStack { get; set; } = new Stack<Element>();

        public MEPSystemModel Build()
        {
            var rootModel = new Composite(_element);
            var builder = new ComponentBuilder(_element, this);
            var comp = builder.Build();
            rootModel.Add(comp);

            var childModel = new Composite(_element);

            foreach (var childNode in ChildStack)
            {
                var childBuilder = new ComponentBuilder(childNode, this);
                var childComp = childBuilder.Build();
                childModel.Add(childComp);
            }

            rootModel.Add(childModel);

            return new MEPSystemModel(rootModel);
        }
    }
}
