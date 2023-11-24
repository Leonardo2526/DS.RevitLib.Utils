using DS.RevitLib.Utils.Connections.PointModels;

namespace DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder
{
    public interface IPointCreationStrategy
    {
        IConnectionPoint GetPoint(int pointId = 1);
    }
}