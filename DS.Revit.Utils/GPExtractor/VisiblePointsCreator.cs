using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.Utils.GPExtractor
{
    class VisiblePointsCreator
    {
        public void Create(Document Doc, List<XYZ> points)
        {


            int j;
            foreach (XYZ point in points)
            {
                AddCell(Doc, point);

            }

        }

        public void AddCell(Document Doc, XYZ location)
        {

            // get the given view's level for beam creation
            Level level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            // get a family symbol
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);

            FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;
            FamilyInstance instance = null;

            using (Transaction transNew = new Transaction(Doc, "GPExtractor.CreatePoint"))
            {
                try
                {
                    transNew.Start();

                    instance = Doc.Create.NewFamilyInstance(location, gotSymbol,
                 level, StructuralType.NonStructural);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }

        }
    }
}
