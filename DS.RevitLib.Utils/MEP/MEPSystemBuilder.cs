using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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

        public MEPSystemModel Build()
        {
            var rootModel = new Composite();
            var builder = new ComponentBuilder(_element, this);
            var comp = builder.Build();
            rootModel.Add(comp);

            if (builder.ChildElements.Any())
            {
                var childs = GetChilds(builder);
                rootModel.Add(childs);
            }


            //Get parents
            var _model = new Composite();
            var modelComponent = GetParents(builder, rootModel);
            if (modelComponent is not null && modelComponent.Any())
            {
                foreach (var item in modelComponent)
                {
                    _model.Add(item);
                }
                return new MEPSystemModel(_model);
            }

            return new MEPSystemModel(rootModel);
        }

        private List<Composite> GetParents(ComponentBuilder builder, Composite childComp)
        {
            if (!builder.ParentElements.Any())
            {
                return null;
            }

            //add parent components
            List<Composite> _composites = new List<Composite>();

            foreach (var parentElem in builder.ParentElements)
            {
                var composite = new Composite();

                var rootBuilder = new ComponentBuilder(parentElem, this);
                MEPSystemComponent rootComp = rootBuilder.Build();
                composite.Add(childComp);
                composite.Add(rootComp);

                //add parents of root
                List<Composite> parents = GetParents(rootBuilder, composite);
                if (parents is null || !parents.Any())
                {
                    _composites.Add(composite);
                }
                else
                {
                    foreach (var parent in parents)
                    {
                        parent.Add(composite);
                    }
                    foreach (var item in parents)
                    {
                        composite.Add(item);
                    }
                }

            }

            return _composites;
        }



        private Composite GetChilds(ComponentBuilder builder)
        {
            //add childs components
            var childModel = new Composite();

            foreach (var childElem in builder.ChildElements)
            {
                var childBuilder = new ComponentBuilder(childElem, this);
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
