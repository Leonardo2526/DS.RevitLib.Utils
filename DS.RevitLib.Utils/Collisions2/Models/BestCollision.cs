namespace DS.RevitLib.Collisions2
{
    /// <summary>
    /// An object that represents collision between two objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public abstract class BestCollision<T, P> : IBestCollision
    {
        /// <summary>
        /// First object of collision.
        /// </summary>
        public T Object1 { get; }

        /// <summary>
        /// Second object of collision.
        /// </summary>
        public P Object2 { get; }

        /// <summary>
        /// Instantiate an object to build collision.
        /// </summary>
        /// <param name="object1">First object of collision.</param>
        /// <param name="object2">Second object of collision.</param>
        public BestCollision(T object1, P object2)
        {
            Object1 = object1;
            Object2 = object2;
        }
    }
}
