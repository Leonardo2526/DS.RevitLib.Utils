using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Solids.Models;
using iUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    public static class SolidCollisionCheckerTest
    {
        public static void Run(Document doc)
        {
            var geomModelElems = GetGeometryElements(doc);
            var modelElements = geomModelElems.Select(obj => obj.Element).ToList();
            var solidsExt = new List<SolidModelExt>();
            foreach (var elem in modelElements)
            {
                solidsExt.Add(new SolidModelExt(elem));
            }

            List<RevitLinkInstance> allLinks = new List<RevitLinkInstance>();
            allLinks = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();
            var geomlinkElems = GetGeometryElements(allLinks.First().GetLinkDocument());
            var linkElements = geomlinkElems.Select(obj => obj.Element).ToList();

            var checker = new SolidCollisionChecker(solidsExt, linkElements, null);
            var collisions = checker.GetCollisions();

            string outString = null;
            foreach (SolidElemCollision col in collisions)
            {
                outString += col.Object1.Element.Id + " - " + col.Object2.Id + "\n";
            }

            TaskDialog.Show("Collisions: ", collisions.Count.ToString() + "\n" + outString);
        }



        private static List<GeometryData> GetGeometryElements(Document doc, Transform tr = null)
        {
            if (doc == null || !doc.IsValidObject)
                return new List<GeometryData>();
            var categories = doc.Settings.Categories.Cast<Category>().Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id)
                .Where(x => !x.IntegerValue.Equals((int)BuiltInCategory.OST_Materials)).ToList();
            var filter = new ElementMulticategoryFilter(categories);
            var elems = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                    .WherePasses(filter).Where(x => ContainsSolidChecker(x))
                    .Select(x => new GeometryData(x, tr, false)).ToList();

            elems = elems.Where(x => x.Element.Category.GetBuiltInCategory().ToString() !=
            BuiltInCategory.OST_TelephoneDevices.ToString()).ToList();

            return elems;
        }
        private static bool ContainsSolidChecker(Element x)
        {
            var g = x.get_Geometry(new Options() { ComputeReferences = false, DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = false })
                ?.Cast<GeometryObject>().ToList();

            return CheckGeometry(g);

            bool CheckGeometry(List<GeometryObject> g)
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
}
