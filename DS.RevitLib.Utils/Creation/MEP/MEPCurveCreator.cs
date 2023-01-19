using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Elements;

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
        /// Create a new instance of object to create and modify MEPCurves. 
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
                if (mEPCurve.MEPSystem is null)
                {
                    return MEPElementUtils.GetDefaultMepSystemType(mEPCurve).Id;
                }
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
            MEPCurve mEPCurve = ElementTypeName == "Pipe" ?
                Pipe.Create(_doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2) :
                Duct.Create(_doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);

            Insulation.Create(baseMEPCurve, mEPCurve);
            ElementParameter.CopyAllParameters(baseMEPCurve, mEPCurve);

            return mEPCurve;
        }

        public MEPCurve Create(Connector c1, XYZ p2, MEPCurve baseMEPCurve = null)
        {
            baseMEPCurve ??= _baseMEPCurve;
            MEPCurve mEPCurve = ElementTypeName == "Pipe" ?
                Pipe.Create(_doc, ElementTypeId, MEPLevelId, c1, p2) :
                Duct.Create(_doc, ElementTypeId, MEPLevelId, c1, p2);

            Insulation.Create(baseMEPCurve, mEPCurve);
            ElementParameter.CopyAllParameters(baseMEPCurve, mEPCurve);

            return mEPCurve;
        }

        public MEPCurve Create(Connector c1, Connector c2, MEPCurve baseMEPCurve = null)
        {
            baseMEPCurve ??= _baseMEPCurve;
            MEPCurve mEPCurve = ElementTypeName == "Pipe" ?
                Pipe.Create(_doc, ElementTypeId, MEPLevelId, c1, c2) :
                Duct.Create(_doc, ElementTypeId, MEPLevelId, c1, c2);

            Insulation.Create(baseMEPCurve, mEPCurve);
            ElementParameter.CopyAllParameters(baseMEPCurve, mEPCurve);

            return mEPCurve;
        }

        /// <summary>
        /// Swap MEPCurve's width and height.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurve with swaped parameters.</returns>
        public MEPCurve SwapSize(MEPCurve mEPCurve)
        {
            double width = mEPCurve.Width;
            double height = mEPCurve.Height;

            Parameter widthParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
            Parameter heightParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);

            widthParam.Set(height);
            heightParam.Set(width);

            return mEPCurve;
        }
    }
}
