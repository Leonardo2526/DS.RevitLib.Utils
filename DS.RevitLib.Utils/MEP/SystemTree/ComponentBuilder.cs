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
        public MEPSystemBuilder _mEPSystemBuilder;

        public ComponentBuilder(Element baseElement, MEPSystemBuilder mEPSystemBuilder)
        {
            _baseElement = baseElement;
            _doc = baseElement.Document;
            _direction = ElementUtils.GetDirections(baseElement).First();
            this._mEPSystemBuilder = mEPSystemBuilder;
        }


        public List<NodeElement> Nodes { get; set; } = new List<NodeElement>();
        public Stack<Element> OwnStack { get; set; } = new Stack<Element>();
        public ObservableCollection<Element> Elements { get; set; } = new ObservableCollection<Element>();

        private List<BuiltInCategory> BoundaryCategories { get; } = new List<BuiltInCategory>()
        {BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_DuctFitting };



        public MEPSystemComponent Build()
        {
            var mEPSystemComponent = new MEPSystemComponent(_baseElement);
            mEPSystemComponent.Elements = GetElements(_baseElement).ToList();

            return mEPSystemComponent;
        }

        private ObservableCollection<Element> GetElements(Element element)
        {

            OwnStack.Push(element);

            while (OwnStack.Any())
            {
                var currentElement = OwnStack.Pop();

                List<Element> connectedElements = ConnectorUtils.GetConnectedElements(currentElement);
                connectedElements = Elements.Any() ?
                    connectedElements.Where(x => x.Id != Elements.Last().Id).ToList() : connectedElements;

                Elements.Add(currentElement);

                if (connectedElements is null | !connectedElements.Any() && OwnStack.Count == 1)
                {
                    Elements.Move(0, Elements.Count - 1);
                }
                else
                {
                    SortByRelation(connectedElements, OwnStack, currentElement);
                }
            }

            return Elements;
        }

        private void SortByRelation(List<Element> connectedElements, Stack<Element> stack, Element currentElement)
        {
            foreach (var connectedElement in connectedElements)
            {

                if (MEPElementUtils.IsNodeElement(currentElement))
                {
                    NodeElement nodeElement = Nodes.Where(x => x.Element.Id == currentElement.Id).First();
                    var postNodePusher = new PostNodePusher(connectedElement, nodeElement, this);
                    postNodePusher.Push();

                    if (postNodePusher.PushedToParent)
                    {
                        break;
                    }

                    continue;
                }

                if (MEPElementUtils.IsNodeElement(connectedElement))
                {
                    var nodePusher = new NodePusher(connectedElement, currentElement, _direction, this);
                    nodePusher.Push();


                    continue;
                }

                stack.Push(connectedElement);

            }

        }

    }
}
