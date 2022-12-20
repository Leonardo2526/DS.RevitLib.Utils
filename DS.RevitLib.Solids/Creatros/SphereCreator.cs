using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Solids
{
    public class SphereCreator : ISolidCreator
    {
        private readonly double _radius;
        private readonly XYZ _center;
        public SphereCreator(double radius, XYZ center)

        {
            _radius = radius;
            _center = center;
        }

        public Arc Profile { get; private set; }
        public Solid Solid { get; private set; }

        public Solid CreateSolid()
        {
            Profile = Arc.Create(_center - _radius * XYZ.BasisZ,
                _center + _radius * XYZ.BasisZ,
                _center + _radius * XYZ.BasisX);

            Line line = Line.CreateBound(Profile.GetEndPoint(1), Profile.GetEndPoint(0));

            var cloop = new CurveLoop();
            cloop.Append(Profile);
            cloop.Append(line);

            IList<CurveLoop> loop = new List<CurveLoop>() { cloop };
            var frame = new Frame(_center, XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ);

            return Solid = GeometryCreationUtilities.CreateRevolvedGeometry(frame, loop, 0, Math.PI * 2);
        }

        public void ShowSolid(Document doc, AbstractTransactionBuilder transactionBuilder = null)
        {
            transactionBuilder ??= new TransactionBuilder(doc);
            transactionBuilder.Build(() =>
                {
                    Solid.ShowShape(doc);
                }, "Show Solid");
        }
    }
}
