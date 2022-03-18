using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public static class Insulation
    {
        /// <summary>
        /// Create insulation of target element by baseMEPCurve
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="targetElement"></param>
        public static void Create(MEPCurve baseMEPCurve, Element targetElement)
        {
            var oldInsulations = InsulationLiningBase.GetInsulationIds(baseMEPCurve.Document, baseMEPCurve.Id)
                .Select(x => baseMEPCurve.Document.GetElement(x) as InsulationLiningBase).ToList();

            if (oldInsulations != null && oldInsulations.Any())
            {
                Type type = baseMEPCurve.GetType();
                if (type.Name == "Pipe")
                {
                    PipeInsulation.Create(baseMEPCurve.Document, targetElement.Id, oldInsulations.First().GetTypeId(), oldInsulations.First().Thickness);
                }
                else
                {
                    DuctInsulation.Create(baseMEPCurve.Document, targetElement.Id, oldInsulations.First().GetTypeId(), oldInsulations.First().Thickness);
                }
            }
        }
    }
}
