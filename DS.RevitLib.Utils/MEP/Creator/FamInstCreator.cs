using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class FamInstCreator
    {
        #region Fields

        private readonly Document _doc;
        private readonly string _transactionPrefix;
        private readonly Committer _committer;

        #endregion

        public FamInstCreator(Document doc, Committer committer = null, string transactionPrefix = "")
        {
            _doc = doc;
            if (committer is null)
            {
                _committer = new BaseCommitter();
            }
            else
            {
                _committer = committer;
            }

            if (!String.IsNullOrEmpty(transactionPrefix))
            {
                _transactionPrefix = transactionPrefix + "_";
            }
        }


        private Level MEPLevel
        {
            get
            {
                return new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault();
            }
        }

        public string ErrorMessages { get; private set; }

        /// <summary>
        /// Create fitting between two pipes
        /// </summary>
        /// <param name="mepCurve1"></param>
        /// <param name="mepCurve2"></param>
        public FamilyInstance CreateFittingByMEPCurves(MEPCurve mepCurve1, MEPCurve mepCurve2)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "CreateFittingByMEPCurves"))
            {
                try
                {
                    transNew.Start();

                    List<Connector> connectors1 = ConnectorUtils.GetConnectors(mepCurve1);
                    List<Connector> connectors2 = ConnectorUtils.GetConnectors(mepCurve2);

                    ConnectorUtils.GetNeighbourConnectors(out Connector con1, out Connector con2,
                    connectors1, connectors2);

                    familyInstance = _doc.Create.NewElbowFitting(con1, con2);
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
                //if (transNew.HasStarted())
                //{
                //    transNew.Commit();
                //}
            }
            return familyInstance;
        }


        /// <summary>
        /// Create elbow or tee by given connectors
        /// </summary>
        /// <param name="con1"></param>
        /// <param name="con2"></param>
        /// <param name="con3"></param>
        /// <returns></returns>
        public FamilyInstance CreateFittingByConnectors(Connector con1, Connector con2, Connector con3 = null)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "CreateFittingByConnectors"))
            {
                try
                {
                    transNew.Start();

                    if (con3 is null)
                    {

                        familyInstance = _doc.Create.NewElbowFitting(con1, con2);
                    }
                    else
                    {
                        familyInstance = _doc.Create.NewTeeFitting(con1, con2, con3);
                    }
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            return familyInstance;
        }

        /// <summary>
        /// Create takeoff fitting
        /// </summary>
        /// <param name="con"></param>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public FamilyInstance CreateTakeOffFitting(Connector con, MEPCurve mEPCurve)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "CreateTakeOffFitting"))
            {
                try
                {
                    transNew.Start();
                    familyInstance = _doc.Create.NewTakeoffFitting(con, mEPCurve);
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            return familyInstance;
        }


        public FamilyInstance CreateFamilyInstane(FamilySymbol familySymbol, MEPCurve baseMEPCurve)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "CreateFamilyInstane"))
            {
                try
                {
                    transNew.Start();

                    familyInstance = _doc.Create.NewFamilyInstance(new XYZ(0,0,0), familySymbol, baseMEPCurve.ReferenceLevel, 
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            return familyInstance;
        }

    }
}

