using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    public class BBCollisionUtilsTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _transactionBuilder;

        public BBCollisionUtilsTest(Document doc, UIDocument uiDoc)
        {
            Debug.IndentLevel = 1;
            _uiDoc = uiDoc;
            _doc = doc;
            _transactionBuilder = new TransactionBuilder(doc);
        }

        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select object1");
            var checkedElement1 = _doc.GetElement(reference);
            reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select object2");
            var checkedElement2 = _doc.GetElement(reference);

            //var checkedElements = new List<Element>()
            //{ checkedElement1 };
            var checkedElements = new List<Element>()
            { checkedElement1, checkedElement2 };
            checkedElements = checkedElements.Distinct().ToList();
            checkedElements.ForEach(element => { element.ShowBoundingBox(); });
            var bb = ElementUtils.GetBoundingBox(checkedElements);

            _transactionBuilder.Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(bb, _doc);
                visualizator.Show();
            }, "show BoundingBox");
            //_uiDoc.RefreshActiveView();

            var allElements = new List<Element>();

            var currneDocElements = _doc.GetGeometryElements();
            allElements.AddRange(currneDocElements);

            List<RevitLinkInstance> allLinks = new List<RevitLinkInstance>();
            allLinks = new FilteredElementCollector(_doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            foreach (var link in allLinks)
            {
                var lDoc = link.GetLinkDocument();
                var tr = link.GetTotalTransform();
                currneDocElements = lDoc.GetGeometryElements(null);
                allElements.AddRange(currneDocElements);

                //show bb
                currneDocElements.ForEach(obj => obj.ShowBoundingBox(link));
            }

            Debug.WriteLine($"AllElements count - {allElements.Count}");

            var outline = new Outline(bb.Min, bb.Max);  
            var bBCollisionUtils = new BBCollisionUtils(_doc, allElements, allLinks);
            var elemetnsInOutline = bBCollisionUtils.GetElements(outline, 0, checkedElements);
            Debug.WriteLine($"ElemetnsInOutline count - {elemetnsInOutline.Count} \n ElemetnsInOutline Ids: ");
            elemetnsInOutline.ForEach(el => Debug.WriteLine($"{el.Id}"));
            var ids = elemetnsInOutline.Select(obj => obj.Id).ToList();
            _uiDoc.Selection.SetElementIds(ids);
        }
    }
}
