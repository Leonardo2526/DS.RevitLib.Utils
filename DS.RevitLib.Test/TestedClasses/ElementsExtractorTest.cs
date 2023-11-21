using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Visualisators;
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
        private readonly TransactionBuilder _transactionBuilder;
        List<BuiltInCategory> _exludedCathegories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctFittingInsulation,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctCurvesInsulation,
                BuiltInCategory.OST_PipeFittingInsulation,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PipeCurvesInsulation,
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials,
                BuiltInCategory.OST_Rooms
            };

        public ElementsExtractorTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
            _transactionBuilder = new TransactionBuilder(doc);
        }


        public void Run()
        {
            var (docElements, linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories).GetAll();

            Debug.WriteLine($"DocElements count is: {docElements.Count}");
            Debug.WriteLine($"linkElements count is: {linkElementsDict?.Count}");
        }

        public void RunWithBox()
        {
            var outline = GetOutline();

            var bb = new BoundingBoxXYZ();
            bb.Min = outline.MinimumPoint;
            bb.Max = outline.MaximumPoint;


            _transactionBuilder.Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(bb, _doc);
                visualizator.Show();
            }, "show BoundingBox");
            //outline.MinimumPoint.Show(_doc);
            //outline.MaximumPoint.Show(_doc);
            //_uiDoc.RefreshActiveView();
            //return;

            var (docElements, linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories, outline).GetAll();

            var docElementIds = docElements.Select(e => e.Id).ToList();
            var linkElementIds = linkElementsDict.SelectMany(l => l.Value.Select(e => e.Id)).ToList();
            var elementIds = new List<ElementId>();
            elementIds.AddRange(docElementIds);
            elementIds.AddRange(linkElementIds);

            _uiDoc.Selection.SetElementIds(elementIds);

            Debug.WriteLine($"DocElements count is: {docElementIds.Count}");
            Debug.WriteLine($"linkElements count is: {linkElementIds?.Count}");
        }

        private Outline GetOutline()
        {
            var p1 = _uiDoc.Selection.PickPoint("Укажите точку 1");
            p1.Show(_doc);
            _uiDoc.RefreshActiveView();

            var p2 = _uiDoc.Selection.PickPoint("Укажите точку 2");
            p2.Show(_doc);
            _uiDoc.RefreshActiveView();

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { p1, p2 });

            double offsetX = 0;
            double offsetY = 0;
            double offsetZ = 5000.MMToFeet();

            var moveVector = new XYZ(XYZ.BasisX.X * offsetX, XYZ.BasisY.Y * offsetY, XYZ.BasisZ.Z * offsetZ);

            var p11 = minPoint + moveVector;
            var p12 = minPoint - moveVector;
            (XYZ minP1, XYZ maxP1) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p11, p12 });

            var p21 = maxPoint + moveVector;
            var p22 = maxPoint - moveVector;
            (XYZ minP2, XYZ maxP2) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p21, p22 });

            return new Outline(minP1, maxP2);
        }

    }
}
