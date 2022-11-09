using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Solids.Models;
using System;

namespace DS.RevitLib.Utils.Elements.Models
{
    public abstract class AbstractElementModel
    {
        public AbstractElementModel(Element element, SolidModel solidModel)
        {
            Element = element;
            SolidModel = solidModel;
            Type = element.GetType();
            Location = ElementUtils.GetLocationPoint(element);
        }

        public SolidModel SolidModel { get; set; }
        public Type Type { get; set; }
        public XYZ Location { get; set; }

        public Element Element { get; }

        public abstract double GetSizeByVector(XYZ orth);
    }
}
