using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Alignments;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
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
        private readonly SolidModelExt _operationModel;
        private readonly XYZ _solidCenter;
        private readonly MEPCurve _targerMEPCurve;
        private readonly XYZ _placementPoint;

        public SolidPlacer(SolidModelExt operationModel, MEPCurve targerMEPCurve, XYZ placementPoint)
        {
            _operationModel = operationModel;
            _solidCenter = operationModel.Solid.ComputeCentroid();
            _targerMEPCurve = targerMEPCurve;
            _placementPoint = placementPoint;
        }

        public void Place()
        {
            //Move solidmodel to placement point position
            XYZ moveVector = (_placementPoint - _operationModel.CentralPoint).RoundVector();

            Transform moveTransform = Transform.CreateTranslation(moveVector);
            _operationModel.Transform(moveTransform);

            //Align solid
            var solidAngleAlignment = new SolidAngleAlignment(_operationModel, _targerMEPCurve);
            solidAngleAlignment.Align();
        }

    }
}
