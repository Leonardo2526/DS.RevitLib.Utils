using Autodesk.Revit.DB;
using System.Collections.Generic;
using DS.RevitUtils.MEP;
using System.Linq;
using DS.RevitLib.Utils;

namespace DS.RVT.ModelSpaceFragmentation
{


    class ModelSolid
    {
        readonly Document Doc;

        public ModelSolid(Document doc)
        {
            Doc = doc;
        }

        public static Dictionary<Element, List<Solid>> SolidsInModel { get; set; }


        public List<Solid> GetSolidsByBBF(BoundingBoxIntersectsFilter boundingBoxFilter)
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            ICollection<BuiltInCategory> elementCategoryFilters = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_DuctCurves,
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_MechanicalEquipment,
                    BuiltInCategory.OST_Walls
                };

            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(elementCategoryFilters);

            //Exclusions
            List<Element> connectedElements = new List<Element>()
            {
                Main.CurrentElement
            };
            ConnectedElement connectedElement = new ConnectedElement();
            connectedElements.AddRange(connectedElement.GetAllConnected(Main.CurrentElement, Doc));
            ICollection<ElementId> elementIds = connectedElements.Select(el => el.Id).ToList();
            ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);

            collector.WhereElementIsNotElementType();
            collector.WherePasses(boundingBoxFilter);
            collector.WherePasses(exclusionFilter);
            IList<Element> intersectedElementsBox = collector.WherePasses(elementMulticategoryFilter).ToElements();

            List<Solid> solidsDictionary = new List<Solid>();

            List<Solid> solids = new List<Solid>();
            foreach (Element elem in intersectedElementsBox)
            {
                solids = ElementUtils.GetSolids(elem);
                if (solids.Count != 0)
                {
                    solidsDictionary.AddRange(solids);
                }
            }

            return solidsDictionary;
        }


    }
}
