namespace DS.RevitLib.Utils.Collisions.Models
{
    public abstract class Collision<T, P> : ICollision
    {
        public T Object1 { get; }
        public P Object2 { get; }

        public Collision(T object1, P object2)
        {
            Object1 = object1;
            Object2 = object2;
        }

        public CollisionStatus Status { get; set; } = CollisionStatus.ToResolve;
    }
}
