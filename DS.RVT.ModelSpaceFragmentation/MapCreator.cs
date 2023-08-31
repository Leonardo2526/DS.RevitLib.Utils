using DS.ClassLib.VarUtils.GridMap;
using System.Windows.Media.Media3D;

namespace DS.RVT.ModelSpaceFragmentation
{
    class MapCreator : IMap
    {
        public Point3D MinPoint { get; set; }
        public Point3D MaxPoint { get; set; }
        public int[,,] Matrix { get; set; }
    }
}
