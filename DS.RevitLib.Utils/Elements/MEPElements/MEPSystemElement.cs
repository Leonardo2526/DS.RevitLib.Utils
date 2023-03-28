using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.MEPElements
{
    internal class MEPSystemElement
    {
        readonly Document Doc;

        public MEPSystemElement(Document doc)
        {
            Doc = doc;
        }

        /// <summary>
        /// Get all elements included in current system name.
        /// </summary>
        public List<Element> GetSystemElements(string strMEPSysName, bool canContain = false)
        {
            var mEPSystem = CheckIfMEPSystemNameExist(strMEPSysName, canContain);
            if (mEPSystem is null)
            { return null; }

            FilteredElementCollector fec = new FilteredElementCollector(Doc);
            ParameterValueProvider pvp = new ParameterValueProvider(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM));
            FilterStringRuleEvaluator fsre = new FilterStringEquals();
            FilterRule fr = new FilterStringRule(pvp, fsre, mEPSystem.Name, true);
            ElementParameterFilter epf = new ElementParameterFilter(fr);

            return fec.WherePasses(epf).Where(x => x.IsPhysicalElement()).ToList();
        }

        MEPSystem CheckIfMEPSystemNameExist(string strMEPSysName, bool canContain)
        {
            FilteredElementCollector pipingSystem = new FilteredElementCollector(Doc).OfClass(typeof(MEPSystem));
            foreach (MEPSystem mEPSystemType in pipingSystem)
            {
                if (strMEPSysName == mEPSystemType.Name)
                    return mEPSystemType;
                else if (canContain && mEPSystemType.Name.Contains(strMEPSysName))
                { return mEPSystemType; }
            }

            TaskDialog.Show("Error", "No such MEP system name exist in current document.");
            return null;
        }
    }
}
