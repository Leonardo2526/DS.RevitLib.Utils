using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class ComponentBuilder
    {
        private readonly Document _doc;
        private Element _baseElement;
        public XYZ _direction;

        public ComponentBuilder(Element baseElement)
        {
            _baseElement = baseElement;
            _doc = baseElement.Document;
            _direction = ElementUtils.GetDirections(baseElement).First();
        }

        public List<NodeElement> Nodes { get; set; } = new List<NodeElement>();
        public Stack<Element> Stack { get; set; } = new Stack<Element>();
        public ObservableCollection<Element> Elements { get; set; } = new ObservableCollection<Element>();




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

        private ObservableCollection<Element> GetElements(Element element)
        {

            Stack.Push(element);

            while (Stack.Any())
            {
                var currentElement = Stack.Pop();

                List<Element> connectedElements = ConnectorUtils.GetConnectedElements(currentElement);
                connectedElements = Elements.Any() ?
                    connectedElements.Where(x => x.Id != Elements.Last().Id).ToList() : connectedElements;

                Elements.Add(currentElement);

                if (connectedElements is null | !connectedElements.Any() && Stack.Count == 1)
                {
                    Elements.Move(0, Elements.Count - 1);
                }
                else
                {
                    SortByRelation(connectedElements, Stack, currentElement);
                }
            }

            return Elements;
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
