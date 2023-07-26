using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{ 

    class GraphicOverwriter
    {
        public void OverwriteElementsGraphic(List<FamilyInstance> instances, Color color)
        {
            foreach (FamilyInstance instance in instances)
            {
                OverwriteGraphic(instance, color);
            }
        }


        void OverwriteGraphic(Element element, Color color)
        {
            Document doc = element.Document;
            UIDocument uIDocument = new UIDocument(doc);

            OverrideGraphicSettings pGraphics = new OverrideGraphicSettings();
            pGraphics.SetProjectionLineColor(color);


            var patternCollector = new FilteredElementCollector(doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement solidFillPattern = patternCollector.ToElements().Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);

            View3D view3D = Get3dView(doc);
            
            pGraphics.SetSurfaceForegroundPatternId(solidFillPattern.Id);
            pGraphics.SetSurfaceBackgroundPatternColor(color);

            using (Transaction transNew = new Transaction(doc, "OverwriteGraphic"))
            {
                try
                {
                    transNew.Start();
                    doc.ActiveView.SetElementOverrides(element.Id, pGraphics);
                    view3D.SetElementOverrides(element.Id, pGraphics);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

        }

        /// <summary>
        /// Retrieve a suitable 3D view from document.
        /// </summary>
        View3D Get3dView(Document doc)
        {
            FilteredElementCollector collector
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D));

            foreach (View3D v in collector)
            {
                if (!v.IsTemplate)
                {
                    return v;
                }
            }
            return null;
        }

        public void OverwriteCell(Color color, int x = 0, int y = 0, int z = 0, XYZ point = null)
        {
            if (point == null)
                point = new XYZ(InputData.ZonePoint1.X + x * InputData.PointsStepF, 
                    InputData.ZonePoint1.Y + y * InputData.PointsStepF, InputData.ZonePoint1.Z + z * InputData.PointsStepF);

            foreach (ElementId elementId in VisiblePointsCreator.InstancesIds)
            {
                Element familyInstance = Main.Doc.GetElement(elementId);
                LocationPoint locPoint = familyInstance.Location as LocationPoint;
                XYZ centerPoint = locPoint.Point;

                //if (point.DistanceTo(centerPoint)<0.01)
                    OverwriteGraphic(familyInstance, color);
            }



        }
    }
}
