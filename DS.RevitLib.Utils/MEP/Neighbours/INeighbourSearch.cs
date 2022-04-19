using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
