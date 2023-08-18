using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.MEP.Neighbours
{
    interface INeighbourSearch
    {
        /// <summary>
        /// Get elements for next search step.
        /// </summary>
        public List<Element> GetElementsForSearch(List<Element> elements);
    }

}
