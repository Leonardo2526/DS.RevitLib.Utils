using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements
{
    internal interface IElementFinder
    {
        List<Element> Find(Connector connector, Element elementToFind = null);
    }
}
