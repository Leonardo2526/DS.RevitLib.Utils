﻿using Autodesk.Revit.DB;
using System.Collections.Generic;
using DSSolid = DS.RevitLib.Utils.Solids;

namespace DS.RevitLib.Utils.MEP.Neighbours
{
    class SearchWithCollision : INeighbourSearch
    {
        /// <summary>
        /// Source elements for collision check.
        /// </summary>
        private List<Element> _SourceColElements;
        public SearchWithCollision(List<Element> sourceColElements)
        {
            _SourceColElements = sourceColElements;
        }

        /// <summary>
        /// Get elements for next search step.
        /// </summary>
        public List<Element> GetElementsForSearch(List<Element> elements)
        {
            List<Element> elementsForNewSearch = new List<Element>();

            foreach (Element element in elements)
            {
                if (MEPElementUtils.IsValidType(element))
                {
                    List<Solid> solidIntersections = DSSolid.SolidUtils.GetIntersection(
                        _SourceColElements, new List<Element>() { element });
                    if (solidIntersections.Count > 0)
                    {
                        elementsForNewSearch.Add(element);
                    }
                }
            }

            return elementsForNewSearch;
        }
    }
}
