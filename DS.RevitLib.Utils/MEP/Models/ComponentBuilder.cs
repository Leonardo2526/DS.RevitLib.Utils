using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Models
{
    public class ComponentBuilder
    {
        private readonly Document _doc;
        private Element _baseElement;

        public Element _nodeElem1;
        public Element _nodeElem2;

        private List<BuiltInCategory> BoundaryCategories { get; } = new List<BuiltInCategory>()
        {BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_DuctFitting };


        public ComponentBuilder(Element baseElement)
        {
            _baseElement = baseElement;
            _doc = baseElement.Document;
        }

        public MEPSystemComponent Build()
        {
            var mEPSystemComponent = new MEPSystemComponent(_baseElement);
            mEPSystemComponent.Elements = GetConnectedElemnts(_baseElement).ToList();

            return mEPSystemComponent;
        }

        private ObservableCollection<Element> GetConnectedElemnts(Element element)
        {
            ObservableCollection<Element> _elements = new ObservableCollection<Element>();

            Stack<Element> stack = new Stack<Element>();
            stack.Push(_baseElement);

            while (stack.Any())
            {
                var currentElement = stack.Pop();

                List<Element> connectedElements = ConnectorUtils.GetConnectedElements(currentElement);
                connectedElements = _elements.Any() ?
                    connectedElements.Where(x => x.Id != _elements.Last().Id).ToList() : connectedElements;


                _elements.Add(currentElement);

                if (connectedElements is null | !connectedElements.Any())
                {
                    _elements.Move(0, _elements.Count -1);
                }
                else
                {
                    foreach (var connetedElement in connectedElements)
                    {
                        if (!IsNodeElement(connetedElement))
                        {
                            stack.Push(connetedElement);
                        }
                    }
                }

            }

            return _elements;
        }

        private bool IsNodeElement(Element element)
        {
            return false;
        }

     
        /// <summary>
        /// Get element from connected whose direction is equal to group direction
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>Return null if no elements exist by direction condition.</returns>
        private Element SelectElement(List<Element> elements)
        {
            foreach (var element in elements)
            {
                //if (ConnectorUtils.CheckConnectorsDirection(_Direction, element))
                //{
                //    return element;
                //}
            }

            return null;
        }
    }
}
