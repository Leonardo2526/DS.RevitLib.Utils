using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public abstract class MEPElementsModelBuilder
    {
        #region Constructor
        protected MEPElementsModel MEPElementsModel { get; set; }

        public MEPElementsModelBuilder(MEPCurve baseMEPCurve, string transactionPrefix = "")
        {
            Doc = baseMEPCurve.Document;
            BaseMEPCurve = baseMEPCurve;
            MEPElementsModel = new MEPElementsModel();
            TransactionPrefix = transactionPrefix;
        }

        #endregion

        public static implicit operator MEPElementsModel(MEPElementsModelBuilder mepBuider)
        {
            return mepBuider.MEPElementsModel;
        }

        #region Fields

        protected readonly Document Doc;
        protected readonly MEPCurve BaseMEPCurve;
        protected readonly string TransactionPrefix;

        public string ErrorMessages { get; protected set; }

        #endregion



        #region Methods

        abstract public MEPCurvesModelTransaction BuildMEPCurves();


        #endregion

    }
}
