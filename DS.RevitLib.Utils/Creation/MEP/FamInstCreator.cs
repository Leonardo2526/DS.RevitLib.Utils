using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    /// <summary>
    /// Class for create and modify FamilyInstances. 
    /// Transactions are not provided, so a used method should be wrapped into transacion.
    /// </summary>
    public static class FamInstCreator
    {
        #region PublicMethods

        /// <summary>
        /// Create new elbow between two MEPCurves.
        /// </summary>
        /// <param name="mepCurve1"></param>
        /// <param name="mepCurve2"></param>
        public static FamilyInstance CreateElbow(MEPCurve mepCurve1, MEPCurve mepCurve2)
        {
            Document doc = mepCurve1.Document;
            FamilyInstance familyInstance = null;

            List<Connector> connectors1 = ConnectorUtils.GetConnectors(mepCurve1);
            List<Connector> connectors2 = ConnectorUtils.GetConnectors(mepCurve2);

            ConnectorUtils.GetNeighbourConnectors(out Connector con1, out Connector con2,
            connectors1, connectors2);
            if(con1 is null ||con2 is null) 
            { return null; }
            try
            {
                familyInstance = doc.Create.NewElbowFitting(con1, con2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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
        public static FamilyInstance Create(Connector con1, Connector con2, Connector con3 = null)
        {
            Document doc = con1.MEPSystem.Document;
            return con3 is null ? doc.Create.NewElbowFitting(con1, con2) : doc.Create.NewTeeFitting(con1, con2, con3);
        }

        /// <summary>
        /// Create takeoff fitting
        /// </summary>
        /// <param name="con"></param>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public static FamilyInstance Create(Connector con, MEPCurve mEPCurve)
        {
            Document doc = mEPCurve.Document;
            return doc.Create.NewTakeoffFitting(con, mEPCurve);
        }

        /// <summary>
        /// Create family instance by <paramref name="familySymbol"/> in <paramref name="point"/>.
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="point"></param>
        /// <param name="level"></param>
        /// <returns>Returns created family instance.</returns>
        public static FamilyInstance Create(FamilySymbol familySymbol, XYZ point, Level level = null)
        {
            Document doc = familySymbol.Document;
            level ??= new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            FamilyInstance familyInstance = doc.Create.NewFamilyInstance(point, familySymbol, level,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            //elevation correction
            var lp = ElementUtils.GetLocationPoint(familyInstance);
            if (Math.Round(lp.Z, 3) != Math.Round(point.Z, 3))
            {
                ElementTransformUtils.MoveElement(doc, familyInstance.Id, point - lp);
            }

            return familyInstance;
        }

        /// <summary>
        /// Set <paramref name="parameters"/> to <paramref name="famInst"/>
        /// </summary>
        /// <param name="famInst"></param>
        /// <param name="parameters"></param>
        public static void SetSizeParameters(FamilyInstance famInst, Dictionary<Parameter, double> parameters)
        {
            var famInstParameters = MEPElementUtils.GetSizeParameters(famInst);

            foreach (var param in parameters)
            {
                var keyValuePair = famInstParameters.Where(obj => obj.Key.Id == param.Key.Id).FirstOrDefault();
                keyValuePair.Key.Set(param.Value);
            }
        }

        #endregion
    }
}

