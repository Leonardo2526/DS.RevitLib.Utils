using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Various;
using iUtils.SelctionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using System.Xml.Linq;
using System.Diagnostics;
using DS.RevitLib.Utils.SelectionFilters;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class SelectionTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public SelectionTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = _uiDoc.Document;
        }

        public void Run()
        {
            var selector = new MEPCurveSelector(_uiDoc) { AllowLink = true};
            var element = selector.Pick();
            Debug.WriteLine($"Selected element is: {element.Id}.");
        }

    }
}
