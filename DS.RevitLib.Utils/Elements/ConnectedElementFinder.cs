using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements
{
    internal class ConnectedElementFinder : IElementFinder
    {
        private List<Element> _elements = new List<Element>();
        private readonly Queue<Element> _open = new Queue<Element>();

        public List<Element> Elements { get => _elements; private set => _elements = value; }

        public List<Element> Find(Connector connector, Element elementToFind = null)
        {
            var owner = connector.Owner;

            List<Element> elems = connector.GetConnected();
            elems.ForEach(_open.Enqueue);
            var close = new List<ElementId>() { owner.Id };

            while (_open.Count > 0)
            {
                var current = _open.Dequeue();
                _elements.Add(current);

                if (elementToFind != null && elementToFind.Id == current.Id) { break; }

                elems = ConnectorUtils.GetConnectedElements(current).
                    Where(e => !close.Contains(e.Id)).ToList();
                elems.ForEach(_open.Enqueue);
                close.Add(current.Id);
            }

            return _elements;
        }
    }
}
