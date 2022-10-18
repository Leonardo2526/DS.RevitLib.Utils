using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class FamInstTransactions
    {
        #region Fields

        private readonly Document _doc;
        private readonly string _transactionPrefix;
        private readonly Committer _committer;
        private readonly bool _transactionCommit;
        #endregion

        public FamInstTransactions(Document doc, Committer committer = null, string transactionPrefix = "", bool transactionCommit = false)
        {
            _doc = doc;
            _committer = committer is null ? new BaseCommitter() : committer;
            _transactionPrefix = string.IsNullOrEmpty(transactionPrefix) ? null : transactionPrefix + "_";
            _transactionCommit = transactionCommit;
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

        /// <summary>
        /// Create family instance.
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="point"></param>
        /// <param name="baseMEPCurve"></param>
        /// <returns>Returns created family instance.</returns>
        public FamilyInstance CreateFamilyInstane(FamilySymbol familySymbol, XYZ point, Level level = null, Element baseElement = null,
            CopyParameterOption copyParameterOption = CopyParameterOption.All)
        {
            level ??= new FilteredElementCollector(_doc).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(_doc, _transactionPrefix + "CreateFamilyInstane"))
            {
                try
                {
                    transNew.Start();

                    familyInstance = _doc.Create.NewFamilyInstance(point, familySymbol, level,
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    //baseElement option
                    if (baseElement is not null)
                    {
                        Insulation.Create(baseElement, familyInstance);
                        switch (copyParameterOption)
                        {
                            case CopyParameterOption.All:
                                ElementParameter.CopyAllParameters(baseElement, familyInstance);
                                break;
                            case CopyParameterOption.Sizes:
                                ElementParameter.CopySizeParameters(baseElement, familyInstance);
                                break;
                            default:
                                break;
                        }
                    }
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }

                _committer.Commit(transNew);
                ErrorMessages += _committer?.ErrorMessages;
            }

            //elevation correction
            var lp = ElementUtils.GetLocationPoint(familyInstance);
            if (Math.Round(lp.Z, 3) != Math.Round(point.Z, 3))
            {
                ElementsMover.MoveElement(familyInstance, point - lp);
            }

            return familyInstance;
        }

        public void SetSizeParameters(FamilyInstance famInst, Dictionary<Parameter, double> parameters)
        {
            var famInstParameters = MEPElementUtils.GetSizeParameters(famInst);

            var parameterSetter = new ParameterSetter(famInst, _committer, _transactionPrefix);

            foreach (var param in parameters)
            {
                var keyValuePair = famInstParameters.Where(obj => obj.Key.Id == param.Key.Id).FirstOrDefault();
                parameterSetter.SetValue(keyValuePair.Key, param.Value);
            }
        }


        delegate void ConnectOperation(Connector c1, Connector c2);
        private void TransactionConnect(Connector c1, Connector c2) => ConnectorUtils.ConnectConnectors(_doc, c1, c2);
        private void Connect(Connector c1, Connector c2) => c1.ConnectTo(c2);  

        public void Insert(FamilyInstance family, MEPCurve mEPCurve, out List<MEPCurve> splittedMEPCurves)
        {
            var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(family);
            splittedMEPCurves = mEPCurve.Cut(famInstCon1.Origin, famInstCon2.Origin);

            //connect connectors
            List<Connector> cons = new List<Connector>();
            foreach (var curve in splittedMEPCurves)
            {
                cons.AddRange(ConnectorUtils.GetConnectors(curve));
            }

            var selectedCon = ConnectorUtils.GetClosest(famInstCon1, cons);
            ConnectOperation connectOperation = _transactionCommit ? TransactionConnect : Connect;
            connectOperation.Invoke(famInstCon1, selectedCon);          

            selectedCon = ConnectorUtils.GetClosest(famInstCon2, cons);
            connectOperation.Invoke(famInstCon2, selectedCon);
        }

    }
}

