using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class CompositeBuilder
    {
        private readonly Element _element;
        private readonly MEPSystemComponent _rootComponent;

        public CompositeBuilder(MEPSystemComponent component)
        {
            _rootComponent = component;
        }

        public CompositeBuilder(Element element)
        {
            _element = element;

            var builder = new ComponentBuilder(_element);
            _rootComponent = builder.Build();
        }

        public Composite Build()
        {
            var composite = new Composite(_rootComponent);

            List<MEPSystemComponent> children = GetRelations(_rootComponent.ChildrenNodes);
            List<MEPSystemComponent> parents = GetRelations(_rootComponent.ParentNodes);

            composite.AddChildren(children.Cast<Component>().ToList());
            composite.AddParents(parents.Cast<Component>().ToList());

            return composite;
        }

        private List<MEPSystemComponent> GetRelations(List<NodeElement> nodes)
        {
            var components = new List<MEPSystemComponent>();

            foreach (var node in nodes)
            {
                var builder = new ComponentBuilder(node.RelationElement);
                var component = builder.Build();
                components.Add(component);
            }

            return components;
        }
    }
}
