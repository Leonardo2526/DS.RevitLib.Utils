using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using Document = Autodesk.Revit.DB.Document;

namespace DS.RevitLib.Test
{
    internal class SolildPlacerTest
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public SolildPlacerTest(UIDocument uidoc, Document doc, UIApplication uiapp)
        {
            Uidoc = uidoc;
            Doc = doc;
            Uiapp = uiapp;
        }

        public void Run()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select operation element");
            Element operationElement = Doc.GetElement(reference);

            reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select target MEPCurve");
            MEPCurve targetElement = (MEPCurve)Doc.GetElement(reference);

            XYZ point = ElementUtils.GetLocationPoint(targetElement);

            var model = new SolidModelExt(operationElement);
            var solidPlacer = new SolidPlacer(model, targetElement, point);
            model = solidPlacer.Place();

            Show(model);

            var checkedObjects1 = new List<Solid>() { model.Solid };
            var checkedObjects2 = GetGeometryElements(Doc);
            var excludedObjects = new List<Element> { targetElement };
            var colSearch = new CollisionSearchClient<Solid, Element>
                (new SolidCollisionSearch(checkedObjects1, checkedObjects2, excludedObjects));
            var collisions = colSearch.GetCollisions();

            string collisionsAccount = "Collisions count: " + collisions.Count.ToString();
            foreach (var item in collisions)
            {
                collisionsAccount += "\n" + item.Id;
            }

            TaskDialog.Show("Collisions", collisionsAccount);
        }


        private void Show(SolidModelExt model)
        {
            BoundingBoxXYZ box = model.Solid.GetBoundingBox();
            IVisualisator vs = new BoundingBoxVisualisator(box, Doc);
            new Visualisator(vs);

            var lineCreator = new ModelCurveCreator(Doc);
            lineCreator.Create(model.CentralLine);

            var normLine = Line.CreateBound(model.CentralPoint, model.CentralPoint + model.MaxOrth);
            lineCreator.Create(normLine);
        }

        private List<Element> GetGeometryElements(Document doc, Transform tr = null)
        {
            if (doc == null || !doc.IsValidObject)
                return null;

            var categories = doc.Settings.Categories.Cast<Category>().Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id)
                .Where(x => !x.IntegerValue.Equals((int)BuiltInCategory.OST_Materials)).ToList();
            var filter = new ElementMulticategoryFilter(categories);
            var elems = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                    .WherePasses(filter).Where(x => ContainsSolidChecker(x)).ToList();

            elems = elems.Where(x => x.Category.GetBuiltInCategory().ToString() !=
            BuiltInCategory.OST_TelephoneDevices.ToString()).ToList();

            return elems;
        }

        private bool ContainsSolidChecker(Element x)
        {
            var g = x.get_Geometry(new Options() { ComputeReferences = false, DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = false })
                ?.Cast<GeometryObject>().ToList();

            return CheckGeometry(g);           
        }

        private bool CheckGeometry(List<GeometryObject> g)
        {
            if (g is null) return false;

            foreach (var elem in g)
            {
                if (elem is Solid s && s.Volume > 1e-6)
                    return true;
                else if (elem is GeometryInstance gi)
                {
                    var go = gi.GetInstanceGeometry();
                    return CheckGeometry(go?.Cast<GeometryObject>().ToList());
                }
            }
            return false;
        }
    }
}
