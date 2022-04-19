using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Neighbours
{
    class Search : INeighbourSearch
    {
        public List<Element> GetElementsForSearch(List<Element> elements)
        {
            List<Element> elementsForNewSearch = new List<Element>();

            foreach (Element element in elements)
            {
                if (MEPElementUtils.CheckMEPElement(element))
                {
                    elementsForNewSearch.Add(element);
                }
            }

            return elementsForNewSearch;
        }
    }
}
