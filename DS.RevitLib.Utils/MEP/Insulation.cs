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
        /// Create insulation of target element by baseElement
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="targetElement"></param>
        public static void Create(Element baseElement, Element targetElement)
        {
            var oldInsulations = InsulationLiningBase.GetInsulationIds(baseElement.Document, baseElement.Id)
                .Select(x => baseElement.Document.GetElement(x) as InsulationLiningBase).ToList();

            if (oldInsulations != null && oldInsulations.Any())
            {
                Type type = baseElement.GetType();
                if (type.Name == "Pipe")
                {
                    PipeInsulation.Create(baseElement.Document, targetElement.Id, oldInsulations.First().GetTypeId(), oldInsulations.First().Thickness);
                }
                else
                {
                    DuctInsulation.Create(baseElement.Document, targetElement.Id, oldInsulations.First().GetTypeId(), oldInsulations.First().Thickness);
                }
            }
        }

        /// <summary>
        /// Get mEPCurve's insulation thickness.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return thickness.</returns>
        public static double GetThickness(MEPCurve mEPCurve)
        {
            var insulations = InsulationLiningBase.GetInsulationIds(mEPCurve.Document, mEPCurve.Id)
                .Select(x => mEPCurve.Document.GetElement(x) as InsulationLiningBase).ToList();

            if (insulations != null && insulations.Any())
            {
                return insulations.First().Thickness;
            }

            return 0;
        }


    }
}
