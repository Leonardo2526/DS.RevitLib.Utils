using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public abstract class MEPSystemComponentBuilder
    {
        #region Constructor
        protected MEPSystemComponent MEPSystemComponent { get; set; }

        public MEPSystemComponentBuilder(MEPCurve baseMEPCurve, string transactionPrefix)
        {
            Doc = baseMEPCurve.Document;
            BaseMEPCurve = baseMEPCurve;
            MEPSystemComponent = new MEPSystemComponent();
            TransactionPrefix = transactionPrefix;
        }

        #endregion

        public static implicit operator MEPSystemComponent(MEPSystemComponentBuilder mepBuider)
        {
            return mepBuider.MEPSystemComponent;
        }

        #region Fields

        protected readonly Document Doc;
        protected readonly MEPCurve BaseMEPCurve;
        protected readonly string TransactionPrefix;

        public string ErrorMessages { get; protected set; }

        #endregion



        #region Methods

        abstract public MEPCurvesModel BuildMEPCurves();
     

        #endregion

    }
}
