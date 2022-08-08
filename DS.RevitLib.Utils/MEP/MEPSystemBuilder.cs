using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    public class MEPSystemBuilder
    {
        private readonly Element _element;

        public MEPSystemBuilder(Element element)
        {
            this._element = element;
        }

        public MEPSystemModel Build()
        {
            MEPSystemModel mEPSystemModel = new MEPSystemModel(_element);
            var comp = new ComponentBuilder(_element).Build();
            mEPSystemModel.Add(comp);

            return mEPSystemModel;
        }
    }
}
