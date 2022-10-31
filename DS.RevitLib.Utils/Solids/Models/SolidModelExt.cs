using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Solids.Models
{
    public class SolidModelExt : AbstractSolidModel
    {
        public SolidModelExt(Element element, Solid solid = null) : base(solid)
        {
            Element = element;
            Solid ??= ElementUtils.GetSolid(element);
            SolidCentroid = Solid.ComputeCentroid();
            ConnectorsPoints = GetConnectorPoints();
            Length = ConnectorsPoints.First().DistanceTo(ConnectorsPoints.Last());
            CentralLine = Element.GetCenterLine();

            //create basis
            var basisX = CentralLine.Direction;
            List<XYZ> normOrths = DS.RevitLib.Utils.Solids.SolidUtils.GetOrthoNormVectors(Solid, CentralLine);
            var basisY = GetMaxSizeBasis(normOrths, basisX);
            var basisZ = basisX.CrossProduct(basisY);
            Basis = new Basis(basisX, basisY, basisZ, CentralPoint);
            Basis.Round();
        }

        public Element Element { get; private set; }
        public List<XYZ> ConnectorsPoints { get; private set; }

        /// <summary>
        /// Length between main connectors.
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Line between main elements's connectors
        /// </summary>
        public Line CentralLine { get; private set; }

        public XYZ CentralPoint
        {
            get
            {
                return (CentralLine.GetEndPoint(0) + CentralLine.GetEndPoint(1)) / 2;
            }
        }

        public XYZ SolidCentroid { get; private set; }

        public Basis Basis { get; set; }

        private List<XYZ> GetConnectorPoints()
        {
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(Element);
            var list = new List<Connector>() { con1, con2 };
            return list.Select(c => c.Origin).ToList();
        }

        public void Transform(Transform transform)
        {
            //get transformed objects
            Solid tSolid = Autodesk.Revit.DB.SolidUtils.CreateTransformed(Solid, transform);
            Line tLine = CentralLine.CreateTransformed(transform) as Line;

            List<XYZ> tConnectorsPoints = new List<XYZ>();
            foreach (var point in ConnectorsPoints)
            {
                tConnectorsPoints.Add(transform.OfPoint(point));
            }

            Solid = tSolid;
            CentralLine = tLine;
            ConnectorsPoints = tConnectorsPoints;
            Basis.Transform(transform);
        }

        public void Transform(List<Transform> transforms)
        {
            foreach (var transform in transforms)
            {
                Transform(transform);
            }
        }

        private XYZ GetMaxSizeOrth(List<XYZ> orths)
        {
            XYZ maxVector = null;
            double maxSize = 0;
            foreach (var orth in orths)
            {
                //double size = 0;
                double size = GetSizeByVector(orth, SolidCentroid);
                if (size > maxSize)
                {
                    maxSize = size;
                    maxVector = orth;
                }
            }

            return maxVector;
        }

        private XYZ GetMaxSizeBasis(List<XYZ> orths, XYZ basisX)
        {
            XYZ maxSizeOrth = GetMaxSizeOrth(orths);

            if (XYZUtils.Perpendicular(maxSizeOrth, basisX))
            {
                return maxSizeOrth;
            }
            else
            {
                XYZ cross = maxSizeOrth.CrossProduct(basisX);
                return basisX.CrossProduct(cross).RoundVector().Normalize();
            }
        }

        public SolidModelExt Clone()
        {
            SolidModelExt model = (SolidModelExt)this.MemberwiseClone();
            model.Basis = new Basis(Basis.X, Basis.Y, Basis.Z, Basis.Point);
            return model;

        }

        public void ShowBoundingBox()
        {
            BoundingBoxXYZ box = Solid.GetBoundingBox();
            IVisualisator vs = new BoundingBoxVisualisator(box, Element.Document);
            new Visualisator(vs);
        }
    }
}
