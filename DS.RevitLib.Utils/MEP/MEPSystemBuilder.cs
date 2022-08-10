using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

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
            builder.ParentElements.Add(_element);
            //var comp = builder.Build();
            //rootModel.Root = comp;


            //Get parents
            var _model = new Composite();
            var modelComponent = GetParents(builder);
            if (modelComponent is not null && modelComponent.Any())
            {
                foreach (var item in modelComponent)
                {
                    _model.Add(item);
                }
                return new MEPSystemModel(_model);
            }


            //var childs = GetChilds(builder);
            //if (childs is not null)
            //{
            //    rootModel.Add(childs);
            //}

            return new MEPSystemModel(_model);
        }

        private List<Composite> GetParents(ComponentBuilder builder, Composite childComposite = null)
        {
            if (!builder.ParentElements.Any())
            {
                return null;
            }

            //add parent components
            var composites = new List<Composite>();

            foreach (var parentElem in builder.ParentElements)
            {
                var composite = new Composite();

                var rootBuilder = new ComponentBuilder(parentElem, this);
                MEPSystemComponent rootComp = rootBuilder.Build();
                composite.Root = rootComp;


                //add children to root
                var newChilds = GetChilds(rootBuilder, childComposite?.Root as MEPSystemComponent);
                if (newChilds is not null)
                {
                    foreach (var newChild in newChilds)
                    {
                        composite.Add(newChild);
                    }
                }

                //add root to it's parents
                List<Composite> parents = GetParents(rootBuilder, composite);
                if (parents is null || !parents.Any())
                {
                    composites.Add(composite);
                }
                else
                {
                    foreach (var parent in parents)
                    {
                        parent.Add(childComposite);
                    }
                    composites.AddRange(parents);
                }

            }

            return composites;
        }



        private List<Composite> GetChilds(ComponentBuilder builder, MEPSystemComponent currentChildComp = null)
        {
            if (!builder.ChildElements.Any())
            {
                return null;
            }

            //add childs components
            var composites = new List<Composite>();

            foreach (var childElem in builder.ChildElements)
            {
                if (currentChildComp is not null && IsElementInCurrentChildComp(childElem, currentChildComp))
                {
                    continue;
                }

                var composite = new Composite();

                var childBuilder = new ComponentBuilder(childElem, this);
                var childComp = childBuilder.Build();

                //add child component
                composite.Root = childComp;

                //add childs of child
                List<Composite> childs = GetChilds(childBuilder);
                if (childs is not null && childs.Any())
                {
                    foreach (var child in childs)
                    {
                        composite.Add(child);
                    }
                    //composite.AddRange(childs.Cast<Component>().ToList());

                }

                composites.Add(composite);
            }

            return composites;
        }

        private bool IsElementInCurrentChildComp(Element childElem, MEPSystemComponent currentChildComp)
        {
            var connected = ConnectorUtils.GetConnectedElements(childElem);

            foreach (var connectedToChildElem in connected)
            {
                if (connectedToChildElem.Id == currentChildComp.ParentNode1?.Id ||
                    connectedToChildElem.Id == currentChildComp.ParentNode2?.Id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
