using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
    public static class ObjectExtension
    {
        public static Document GetDocument(this object obj)
        {
            var element = obj as Element;
            return element?.Document;
        }
    }
}
