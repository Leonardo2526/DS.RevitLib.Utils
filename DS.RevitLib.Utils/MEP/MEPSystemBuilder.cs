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

        public MEPSystemModel Build()
        {
            var rootModel = new Composite();
            var builder = new ComponentBuilder(_element, this);
            var comp = builder.Build();
            rootModel.Add(comp);

            var childs = GetChilds(builder);
            rootModel.Add(childs);






            return new MEPSystemModel(rootModel);
        }


        private Composite GetChilds(ComponentBuilder builder)
        {
            if (!builder.ChildElements.Any())
            {
                return null;
            }

            //add childs components
            var childModel = new Composite();

            foreach (var childNode in builder.ChildElements)
            {
                var childBuilder = new ComponentBuilder(childNode, this);
                var childComp = childBuilder.Build();

                //add childs of child
                var childs = GetChilds(childBuilder);
                if (childs is not null && childs.children.Any())
                {
                    childModel.Add(childs);
                }

                //add child component
                childModel.Add(childComp);
            }

            return childModel;
        }
    }
}
