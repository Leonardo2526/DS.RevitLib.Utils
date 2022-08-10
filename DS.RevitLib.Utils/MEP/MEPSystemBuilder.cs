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
            var _model = new Composite();

            Composite rootComposite = GetRoot(_element, out List <Element> parentElements);

            //Get parents
            var modelComponent = GetParents(parentElements, rootComposite);

            if (modelComponent is not null && modelComponent.Any())
            {
                foreach (var item in modelComponent)
                {
                    _model.Add(item);
                }
                return new MEPSystemModel(_model);
            }

            _model.Add(rootComposite);
            return new MEPSystemModel(_model);
        }


        private Composite GetRoot(Element element, out List<Element> parentElements)
        {
            parentElements = new List<Element>();

            var rootComposite = new Composite();

            var builder = new ComponentBuilder(element, this);
            var component = builder.Build();
            rootComposite.Root = component;

            //get children
            var newChilds = GetChildren(builder);
            if (newChilds is not null)
            {
                foreach (var newChild in newChilds)
                {
                    rootComposite.Add(newChild);
                }
            }

            parentElements.AddRange(builder.ParentElements);

            return rootComposite;
        }


        private List<Composite> GetParents(List<Element> parentElements, Composite childComposite)
        {
            if (!parentElements.Any())
            {
                return null;
            }

            //add parent components
            var composites = new List<Composite>();

            foreach (var parentElem in parentElements)
            {
                var composite = new Composite();
                composite.Add(childComposite);

                var rootBuilder = new ComponentBuilder(parentElem, this);
                MEPSystemComponent rootComp = rootBuilder.Build();
                composite.Root = rootComp;

                //add children to root
                var restChildren = GetChildren(rootBuilder, childComposite?.Root as MEPSystemComponent);
                if (restChildren is not null && restChildren.Any())
                {
                    foreach (var newChild in restChildren)
                    {
                        composite.Add(newChild);
                    }
                }

                //add root to it's parents
                List<Composite> parents = GetParents(rootBuilder.ParentElements, composite);
                if (parents is null || !parents.Any())
                {
                    composites.Add(composite);
                }
                else
                {
                 
                    composites.AddRange(parents);
                }
            }
            return composites;
        }



        private List<Composite> GetChildren(ComponentBuilder builder, MEPSystemComponent currentChildComp = null)
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
                List<Composite> childs = GetChildren(childBuilder);
                if (childs is not null && childs.Any())
                {
                    foreach (var child in childs)
                    {
                        composite.Add(child);
                    }
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
