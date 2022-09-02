using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Alignments;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    internal class MEPCurveCutter
    {
        private readonly MEPCurve _mEPCurve;
        private readonly XYZ _point1;        
        private readonly XYZ _point2;

        public MEPCurveCutter(MEPCurve mEPCurve, XYZ point1, XYZ point2)
        {
            this._mEPCurve = mEPCurve;

            this._point1 = point1.RoundVector();
            this._point2 = point2.RoundVector();
        }

        /// <summary>
        /// Cut MEPCurve between points.
        /// </summary>
        /// <returns>Returns splitted MEPCurves</returns>
        public List<MEPCurve> Cut()
        {
            var (mEPCurveCon1, mEPCurveCon2) = ConnectorUtils.GetMainConnectors(_mEPCurve);
            if (!_point1.IsBetweenPoints(mEPCurveCon1.Origin, mEPCurveCon2.Origin) |
               !_point2.IsBetweenPoints(mEPCurveCon1.Origin, mEPCurveCon2.Origin))
            {
                return null;
            }

            List<MEPCurve> _splittedMEPCurves = Split(_mEPCurve);
            return DeleteMiddleMEPCurve(_splittedMEPCurves);
        }

        private List<MEPCurve> Split(MEPCurve mEPCurve)
        {
            var creator = new MEPCurveCreator(mEPCurve);
            MEPCurve splittedMEPCurve1 = creator.SplitElement(_point1) as MEPCurve;
            var angleAlignment = new AngleAlignment(splittedMEPCurve1, _mEPCurve);
            angleAlignment.AlignNormOrths();

            MEPCurve mEPCurveToSplit = GetMEPCurveByPoint(mEPCurve, splittedMEPCurve1, _point2);
            var splitCreator = new MEPCurveCreator(mEPCurveToSplit);
            MEPCurve splittedMEPCurve2 = splitCreator.SplitElement(_point2) as MEPCurve;
            angleAlignment = new AngleAlignment(splittedMEPCurve2, _mEPCurve);
            angleAlignment.AlignNormOrths();

            return new List<MEPCurve>() { mEPCurve, splittedMEPCurve1, splittedMEPCurve2};
        }


        private List<MEPCurve> DeleteMiddleMEPCurve(List<MEPCurve> mEPCurves)
        {
            var orderedElements = mEPCurves.Cast<Element>().ToList().Order();

            //get elements to delete
            var toDelete = orderedElements.Where(obj => obj.Id != orderedElements.First().Id && obj.Id != orderedElements.Last().Id).ToList();
            foreach (var del in toDelete)
            {
                orderedElements.Remove(del);
                ElementUtils.DeleteElement(_mEPCurve.Document, del);
            }

            return orderedElements.Cast<MEPCurve>().ToList();
        }

        /// <summary>
        /// Select MEPCurve if point lies on it's centerline
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private MEPCurve GetMEPCurveByPoint(MEPCurve mEPCurve1, MEPCurve mEPCurve2, XYZ point)
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(mEPCurve1);

            if (point.IsBetweenPoints(con1.Origin, con2.Origin))
            {
                return mEPCurve1;

            }
            return mEPCurve2;
        }
    }
}
