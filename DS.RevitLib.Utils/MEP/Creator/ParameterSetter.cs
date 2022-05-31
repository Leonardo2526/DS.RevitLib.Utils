using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public static class ParameterSetter
    {
        /// <summary>
        /// Set new value to element's parameter
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurve with swaped parameters.</returns>
        public static Element SetValue(Element element, Parameter parameter, double value)
        {            
            using (Transaction transNew = new Transaction(element.Document, "SetParameter"))
            {
                try
                {
                    transNew.Start();

                    parameter.Set(value);
                }

                catch (Exception e)
                { }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }
            return element;
        }
    }
}
