using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.MEP.Creator
{
    /// <summary>
    /// Class to create MEPCurves. 
    /// Transactions are not provided, so methods should be wrapped to transacion.
    /// </summary>
    public class BuilderByPoints
    {
        private readonly List<XYZ> _points = new();
        private readonly MEPCurve _baseMEPCurve;
        private readonly MEPElementsModel _mEPElementsModel;

        /// <summary>
        /// Create instance of object to create MEPCurves by 
        /// <paramref name="baseMEPCurve"/> and <paramref name="points"/>.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="points"></param>
        public BuilderByPoints(MEPCurve baseMEPCurve, List<XYZ> points)
        {
            _points = points;
            _baseMEPCurve = baseMEPCurve;
            _mEPElementsModel = new MEPElementsModel();
        }

        /// <summary>
        /// Build MEPSystem with elbows. 
        /// Ducts will be aligned if system contains they.
        /// </summary>
        /// <returns>Returns created system.</returns>
        /// <param name="transactionBuilder"></param>
        public MEPCurvesModel BuildSystem(TransactionBuilder<Element> transactionBuilder)
        {
            MEPCurvesModel mEPElementsModel = null;

            transactionBuilder.Build(() => mEPElementsModel = BuildMEPCurves(), "Create MEPSystem by path");
            transactionBuilder.Build(() => mEPElementsModel.RefineDucts(_baseMEPCurve), "Align ducts");
            transactionBuilder.Build(() => mEPElementsModel = mEPElementsModel.WithElbows(), "Insert elbows by path");

            return mEPElementsModel;
        }

        /// <summary>
        /// Build MEPCurves by given points.
        /// </summary>
        /// <returns>Returns created MEPCurvesModel.</returns>
        public MEPCurvesModel BuildMEPCurves()
        {
            var mEPCurveCreator = new MEPCurveCreator(_baseMEPCurve);
            MEPCurve baseMEPCurve = _baseMEPCurve;

            for (int i = 0; i < _points.Count - 1; i++)
            {
                XYZ p1 = _points[i];
                XYZ p2 = _points[i + 1];

                MEPCurve mEPCurve = mEPCurveCreator.Create(p1, p2, baseMEPCurve);
                baseMEPCurve = mEPCurve;

                _mEPElementsModel.AllElements.Add(mEPCurve);
                _mEPElementsModel.MEPCurves.Add(mEPCurve);
            }

            return new MEPCurvesModel(_mEPElementsModel);
        }
    }
}
