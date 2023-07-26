using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    static class BoundingBoxCreator
    {
        public static List<BoundingBoxXYZ> Create()
        {
            List<BoundingBoxXYZ> boundingBoxes = new List<BoundingBoxXYZ>();

            int z;
            for (z = 0; z < SpaceZone.ZoneCountZ; z++)
            {
                double zMin = z * SpaceZone.ZoneSizeZ;
                double zMax = (z + 1) * SpaceZone.ZoneSizeZ;

                int y;
                for (y = 0; y < SpaceZone.ZoneCountY; y++)
                {
                    double yMin = y * SpaceZone.ZoneSizeY;
                    double yMax = (y + 1) * SpaceZone.ZoneSizeY;

                    int x;
                    for (x = 0; x < SpaceZone.ZoneCountX; x++)
                    {
                        double xMin = x * SpaceZone.ZoneSizeX;
                        double xMax = (x + 1) * SpaceZone.ZoneSizeX;

                        BoundingBoxXYZ boundingBoxXYZ = new BoundingBoxXYZ();
                        boundingBoxXYZ.Min = new XYZ(xMin, yMin, zMin) + ElementInfo.MinBoundPoint;
                        boundingBoxXYZ.Max = new XYZ(xMax, yMax, zMax) + ElementInfo.MinBoundPoint;

                        boundingBoxes.Add(boundingBoxXYZ);
                    }
                }
            }

            return boundingBoxes;
        }
    }
}
