using Autodesk.Revit.DB;
using System;
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

            if (ElementUtils.IsElementMEPCurve(element))
            {
                var notSpuds = MEPCurveUtils.GetNotSpudConnectors(element as MEPCurve);
                XYZ basePoint = notSpuds.First().Origin;
                var builder = new ComponentBuilder(_element, basePoint);
                _rootComponent = builder.Build();
            }
            else
            {
                throw new ArgumentException("CompositeBuilder - Element is not MEPCurve");
            }

        }

        public Composite Build()
        {
            var composite = new Composite(_rootComponent);

            if (_rootComponent.ChildrenNodes is not null)
            {
                List<MEPSystemComponent> children = GetRelations(_rootComponent.ChildrenNodes);
                composite.AddChildren(children.Cast<Component>().ToList());
            }
            if (_rootComponent.ParentNodes is not null)
            {
                List<MEPSystemComponent> parents = GetRelations(_rootComponent.ParentNodes);
                composite.AddParents(parents.Cast<Component>().ToList());
            }

            return composite;
        }

        private List<MEPSystemComponent> GetRelations(List<NodeElement> nodes)
        {
            var components = new List<MEPSystemComponent>();

            foreach (var node in nodes)
            {
                var point = ElementUtils.GetLocationPoint(node.Element);
                var builder = new ComponentBuilder(node.RelationElement, point);
                var component = builder.Build();
                components.Add(component);
            }

            return components;
        }
    }
}
