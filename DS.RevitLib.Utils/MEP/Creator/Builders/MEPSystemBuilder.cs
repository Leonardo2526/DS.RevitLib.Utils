﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public abstract class MEPSystemBuilder
    {
        #region Constructor
        protected MEPSystemModel MEPSystemModel { get; set; }

        public MEPSystemBuilder(MEPCurve baseMEPCurve)
        {
            Doc = baseMEPCurve.Document;
            BaseMEPCurve = baseMEPCurve;
            MEPSystemModel = new MEPSystemModel();
        }

        #endregion

        public static implicit operator MEPSystemModel(MEPSystemBuilder mepBuider)
        {
            return mepBuider.MEPSystemModel;
        }

        #region Fields

        protected readonly Document Doc;
        protected readonly MEPCurve BaseMEPCurve;

        #endregion



        #region Methods

        abstract public MEPCurvesModel BuildMEPCurves();
     

        #endregion

    }
}