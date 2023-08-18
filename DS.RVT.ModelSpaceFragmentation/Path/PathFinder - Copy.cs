using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation;
using DS.PathSearch.GridMap;
using FrancoGustavo;
using System.Collections.Generic;
using Location = DS.PathSearch.Location;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Various;
using System.Windows.Media.Media3D;
using DS.ClassLib.VarUtils.Points;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public List<PathFinderNode> AStarPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints, 
            IPathRequiment pathRequiment, CollisionDetectorByTrace collisionDetector, IPointConverter pointConverter)
        {
            //InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            //data.ConvertToPlane();
            var uCS2startPoint =  pointConverter.ConvertToUCS2(new Point3D(startPoint.X, startPoint.Y, startPoint.Z));
            var uCS2endPoint =  pointConverter.ConvertToUCS2(new Point3D(endPoint.X, endPoint.Y, endPoint.Z));

            var uCS2startPointInd = uCS2startPoint.Round(1);
            var uCS2endPointInd = uCS2endPoint.Round(1);

            var map = new MapCreator();

            var uCS1BasePoint =new Point3D(ElementInfo.MaxBoundPoint.X, ElementInfo.MaxBoundPoint.Y, ElementInfo.MaxBoundPoint.Z);
            var uCS2maxPoint = pointConverter.ConvertToUCS2(new Point3D(uCS1BasePoint.X, uCS1BasePoint.Y, uCS1BasePoint.Z));
            var uCS2maxPointInt = uCS2maxPoint.Round(1);
            map.MinPoint = new Point3D(0,0,0);
            map.MaxPoint = uCS2maxPointInt;

            map.Matrix = new int[(int)uCS2maxPointInt.X, (int)uCS2maxPointInt.Y, (int)uCS2maxPointInt.Z];

            //foreach (StepPoint unpass in InputData.UnpassStepPoints)
            //    map.Matrix[unpass.X, unpass.Y, unpass.Z] = 1;

            List<PathFinderNode> pathNodes = FGAlgorythm.GetPathByMap(map,
                uCS2startPointInd, uCS2endPointInd, pathRequiment, collisionDetector, pointConverter);

            return pathNodes;
        }
    }
}
