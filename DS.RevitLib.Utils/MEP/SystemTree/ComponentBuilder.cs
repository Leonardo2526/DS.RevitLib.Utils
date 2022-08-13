using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class ComponentBuilder
    {
        private readonly Document _doc;
        private Element _baseElement;
        public XYZ _direction;
        private List<Element> _spudsOnBase = new List<Element>();
        private List<Element> _connectedToBase = new List<Element>();
        private NodeElement _node;

        public ComponentBuilder(Element baseElement)
        {
            _baseElement = baseElement;
            _doc = baseElement.Document;
            _direction = ElementUtils.GetDirections(baseElement).First();
        }

        public ComponentBuilder(NodeElement node)
        {
            _node = node;
            _baseElement = node.RelationElement;
            _doc = node.RelationElement.Document;
            _direction = ElementUtils.GetDirections(node.RelationElement).First();
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
                if (!reversed && _connectedToBase.Any() && _connectedToBase.Where(x => x.Id == currentElement.Id).Any())
                {
                    i++;
                    if (i == 2)
                    {
                        Elements.Reverse();
                        reversed = true;
                    }
                }

                List<Element> connectedElements = currentElement.Id == _baseElement.Id ?
                    GetConnectedToBase(currentElement) : GetConnected(currentElement);

                Elements.Add(currentElement);

                if (connectedElements is not null && connectedElements.Any())
                {
                    SortByRelation(connectedElements, Stack, currentElement);
                }

            }

            if (_connectedToBase.Count == 1)
            {
                Elements.Reverse();
            }

            return Elements;
        }



        private List<Element> GetConnected(Element element)
        {
            List<Element> connectedElements =  ConnectorUtils.GetConnectedElements(element);           

            var intersected = connectedElements.Select(x => x.Id).Intersect(Elements.Select(x => x.Id)).ToList();

            if (intersected.Count != 1)
            {
                throw new ArgumentException("intersected.Count != 1");
            }

            var intersectedElem = element.Document.GetElement(intersected.First());

            foreach (var inter in intersected)
            {
                connectedElements = connectedElements.Where(x => x.Id != inter).ToList();
            }

            var (elem1Con, elem2Con) = ConnectorUtils.GetCommonConnectors(element, intersectedElem);
            if (connectedElements.Count > 1)
            {
                connectedElements = connectedElements.OrderByPoint(elem1Con.Origin);
            }

            return connectedElements;
        }


        private List<Element> GetConnectedToBase(Element element)
        {
            if (ElementUtils.IsElementMEPCurve(element))
            {
                XYZ basePoint = GetBasePoint(element);
                _connectedToBase = MEPCurveUtils.GetOrderedConnected(element as MEPCurve, basePoint);
                return _connectedToBase;
            }

            return ConnectorUtils.GetConnectedElements(element);
        }


        private XYZ GetBasePoint(Element element)
        {
            if (_node is null)
            {
                var notSpuds = MEPCurveUtils.GetNotSpudConnectors(element as MEPCurve);
                return notSpuds.First().Origin;
            }
            else if (_node.SystemRelation == Relation.Child)
            {
                return ElementUtils.GetLocationPoint(_node.Element);
            }
            else if (_node.SystemRelation == Relation.Parent)
            {
                var notSpuds = MEPCurveUtils.GetNotSpudConnectors(element as MEPCurve);
                return notSpuds.First().Origin;
            }

            return null;
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
