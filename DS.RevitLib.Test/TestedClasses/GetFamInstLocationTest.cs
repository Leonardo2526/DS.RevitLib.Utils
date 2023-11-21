using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class GetFamInstLocationTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public GetFamInstLocationTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            Run();
        }

        private void Run()
        {
            var famInst = new ElementSelector(_uiDoc).Pick() as FamilyInstance;

            if(famInst == null) { return; }

            var location = famInst.GetLocation();
            location.Show(_doc);
        }
    }
}
