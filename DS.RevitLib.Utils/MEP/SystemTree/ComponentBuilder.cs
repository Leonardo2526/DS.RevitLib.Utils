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
        private readonly XYZ _direction;
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
            var elements = new ObservableCollection<Element>();

            OwnStack.Push(element);

            while (OwnStack.Any())
            {
                var currentElement = OwnStack.Pop();

                List<Element> connectedElements = ConnectorUtils.GetConnectedElements(currentElement);
                connectedElements = elements.Any() ?
                    connectedElements.Where(x => x.Id != elements.Last().Id).ToList() : connectedElements;

                elements.Add(currentElement);

                if (connectedElements is null | !connectedElements.Any() && OwnStack.Count == 1)
                {
                    elements.Move(0, elements.Count - 1);
                }
                else
                {
                    SortByRelation(connectedElements, OwnStack, currentElement);
                }
            }

            return elements;
        }

        private void SortByRelation(List<Element> connectedElements, Stack<Element> stack, Element currentElement)
        {
            foreach (var connectedElement in connectedElements)
            {

                //if (IsNodeElement(currentElement, out PartType partType))
                //{
                    //var postNodePusher = new PostNodePusher(connectedElement);
                    //postNodePusher.Push();
                //}

                //if (IsNodeElement(_element, out PartType partType))
                //{
                //    var relationFinder = new RelationFinder(connectedElement, currentElement, _direction, _mEPSystemBuilder);
                //    var relation = relationFinder.Find();
                //    Nodes.Add(new NodeElement(connectedElement, relation));
                //}

                stack.Push(connectedElement);



            }

        }

    }
}
