using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
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
            if(baseElement.GetType() == typeof(CableTray) || targetElement.GetType() == typeof(CableTray)) 
            { return; }     

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
    }
}
