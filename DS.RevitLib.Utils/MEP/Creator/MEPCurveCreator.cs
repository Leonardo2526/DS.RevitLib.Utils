using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.TransactionCommitter;

namespace DS.RevitLib.Utils.MEP.Creator
{
    /// <summary>
    /// Class for create and modify MEPCurves. 
    /// Transactions are not provided, so methods should be wrapped to transacion.
    /// </summary>
    internal class MEPCurveCreator
    {
        private readonly Document _doc;
        private readonly MEPCurve _baseMEPCurve;

        /// <summary>
        /// Create instance of object to create and modify MEPCurves. 
        /// </summary>
        /// <param name="baseMEPCurve">MEPCurve to get parameters for creation methods.</param>
        public MEPCurveCreator(MEPCurve baseMEPCurve)
        {
            _doc = baseMEPCurve.Document;
            _baseMEPCurve = baseMEPCurve;
        }

        /// <summary>
        /// Create object's instance to create and modify MEPCurves. 
        /// </summary>
        public MEPCurveCreator()
        { }


        #region Properties

        private ElementId MEPSystemTypeId
        {
            get
            {
                MEPCurve mEPCurve = _baseMEPCurve as MEPCurve;
                return mEPCurve.MEPSystem.GetTypeId();
            }
        }
        private ElementId ElementTypeId
        {
            get
            {
                return _baseMEPCurve.GetTypeId();
            }
        }
        private string ElementTypeName
        {
            get { return _baseMEPCurve.GetType().Name; }
        }
        private ElementId MEPLevelId
        {
            get
            {
                return _baseMEPCurve.ReferenceLevel.Id;
            }
        }

        #endregion

        /// <summary>
        /// Create MEPCurve between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="baseMEPCurve">MEPCurve to get parameters for creation methods.</param>
        /// <returns>Returns created MEPCurve</returns>
        public MEPCurve Create(XYZ p1, XYZ p2, MEPCurve baseMEPCurve = null)
        {
            baseMEPCurve ??= _baseMEPCurve;
            MEPCurve mEPCurve;
            if (ElementTypeName == "Pipe")
            {
                mEPCurve = Pipe.Create(_doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);
            }
            else
            {
                mEPCurve = Duct.Create(_doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);
            }

            Insulation.Create(baseMEPCurve, mEPCurve);
            ElementParameter.CopyAllParameters(baseMEPCurve, mEPCurve);

            return mEPCurve;
        }
    }
}
