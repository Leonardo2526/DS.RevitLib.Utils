using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree.ConnectedBuilders;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class ComponentBuilder
    {
        private readonly Element _baseElement;
        private List<Element> _connectedToBase = new List<Element>();

        public ComponentBuilder(Element baseElement)
        {
            _baseElement = baseElement;
        }

        public List<NodeElement> Nodes { get; set; } = new List<NodeElement>();
        public Stack<Element> Stack { get; set; } = new Stack<Element>();
        public List<Element> Elements { get; set; } = new List<Element>();


        public MEPSystemComponent Build()
        {
            var mEPSystemComponent = new MEPSystemComponent();

            mEPSystemComponent.BaseElement = _baseElement;

            //fill elements
            mEPSystemComponent.Elements = GetElements(_baseElement).ToList();

            if (Nodes.Any())
            {
                //fill parent Nodes
                var parentNodes = Nodes.Where(x => x.SystemRelation == Relation.Parent).ToList();
                mEPSystemComponent.ParentNodes = parentNodes;

                //fill child nodes
                var childNodes = Nodes.Where(x => x.SystemRelation == Relation.Child).ToList();
                mEPSystemComponent.ChildrenNodes = childNodes;
            }

            return mEPSystemComponent;
        }

        private List<Element> GetElements(Element element)
        {
            bool reversed = false;
            int i = 0;
            Stack.Push(element);

            while (Stack.Any())
            {
                var currentElement = Stack.Pop();

                if (_connectedToBase.Any() && _connectedToBase.Select(x => x.Id).Contains(currentElement.Id))
                {
                    i++;
                }
                if (!reversed && i == 2)
                {
                    Elements.Reverse();
                    reversed = true;
                }

                List<Element> connectedElements = ConnectorUtils.GetConnectedElements(currentElement);
                var intersected = connectedElements.Select(x => x.Id).Intersect(Elements.Select(x => x.Id)).ToList();

                if (intersected.Count > 1 ||
                   currentElement.Category.Name.Contains("Equipment") || currentElement.Category.Name.Contains("Оборудование"))
                {
                    Elements.Add(currentElement);
                    if (Stack.Any() && Stack.Peek().Id == currentElement.Id) Stack.Pop();
                    continue;
                }

                Element exludedElement = intersected.Count == 0 ? null :
                    element.Document.GetElement(intersected.First());
                var builder = new ConnectedElementsBuilder(currentElement, exludedElement);
                List<Element> newConnectedElements = builder.Build();

                if (currentElement.Id == element.Id)
                {
                    _connectedToBase = newConnectedElements;
                }

                Elements.Add(currentElement);

                if (newConnectedElements is not null && newConnectedElements.Any())
                {
                    SortByRelation(newConnectedElements, Stack, currentElement);
                }
            }

            return Elements;
        }

        private Element GetExcluded(Element element)
        {
            if (!Elements.Any())
            {
                return null;
            }


            List<Element> connectedElements = ConnectorUtils.GetConnectedElements(element);
            var intersected = connectedElements.Select(x => x.Id).Intersect(Elements.Select(x => x.Id)).ToList();

            switch (intersected.Count)
            {
                case 0:
                    return null;
                case 1:
                    return element.Document.GetElement(intersected.First());
                default:
                    break;
            }

            throw new ArgumentException("intersected.Count != 1");
        }

        private void SortByRelation(List<Element> connectedElements, Stack<Element> stack, Element currentElement)
        {
            NodeElement currentNodeElement = null;
            if (MEPElementUtils.IsNodeElement(currentElement))
            {
                currentNodeElement = Nodes.Where(x => x.Element.Id == currentElement.Id).First();
            }

            foreach (var connectedElement in connectedElements)
            {
                if (currentNodeElement is not null)
                {
                    var postNodePusher = new PostNodePusher(currentNodeElement, connectedElement, this);
                    postNodePusher.Push();

                    if (postNodePusher.PushedToParent)
                    {
                        break;
                    }

                    continue;
                }

                if (MEPElementUtils.IsNodeElement(connectedElement))
                {
                    var nodePusher = new NodePusher
                        (new NodeElement(connectedElement as FamilyInstance), currentElement, this);
                    nodePusher.Push();


                    continue;
                }

                stack.Push(connectedElement);

            }

        }

    }
}
