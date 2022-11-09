using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.MEP.Neighbours
{
    class Search : INeighbourSearch
    {
        public List<Element> GetElementsForSearch(List<Element> elements)
        {
            List<Element> elementsForNewSearch = new List<Element>();

            foreach (Element element in elements)
            {
                if (MEPElementUtils.IsValidType(element))
                {
                    elementsForNewSearch.Add(element);
                }
            }

            return elementsForNewSearch;
        }
    }
}
