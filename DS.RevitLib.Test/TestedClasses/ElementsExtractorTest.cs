using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Collisions2;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements;
using iUtils.SelctionFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class ElementsExtractorTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public ElementsExtractorTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

     
        public void Run()
        {
            var exludedCathegories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctFittingInsulation,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctCurvesInsulation,
                BuiltInCategory.OST_PipeFittingInsulation,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PipeCurvesInsulation,
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials
            };
            var (docElements, linkElementsDict) = new ElementsExtractor(_doc, exludedCathegories).GetAll();

            Debug.WriteLine($"DocElements count is: {docElements.Count}");
            Debug.WriteLine($"linkElements count is: {linkElementsDict?.Count}");
        }

    }
}
