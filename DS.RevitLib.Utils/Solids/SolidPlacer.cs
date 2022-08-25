using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Alignments;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Solids
{
    public class SolidPlacer
    {
        private readonly SolidModelExt _solidModel;
        private readonly XYZ _solidCenter;
        private readonly MEPCurve _targerMEPCurve;
        private readonly XYZ _placementPoint;

        public SolidPlacer(SolidModelExt solidModel, MEPCurve targerMEPCurve, XYZ placementPoint)
        {
            _solidModel = solidModel;
            _solidCenter = solidModel.Solid.ComputeCentroid();
            _targerMEPCurve = targerMEPCurve;
            _placementPoint = placementPoint;
        }

        public SolidModelExt Place()
        {
            //Move solidmodel to placement point position
            XYZ moveVector = (_placementPoint - _solidModel.CentralLine.Origin).RoundVector();
            Transform moveTransform = Transform.CreateTranslation(moveVector);
            _solidModel.Transform(moveTransform);

            //Align solid
            var solidAngleAlignment = new SolidAngleAlignment(_solidModel, _targerMEPCurve);
            return solidAngleAlignment.Align(); 
        }

    }
}
