namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <summary>
    /// An object that represents collision between two objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public abstract class Collision<T, P> : ICollision
    {
        /// <summary>
        /// First (resolving) object of collision.
        /// </summary>
        public T Object1 { get; }

        /// <summary>
        /// Second (state) object of collision.
        /// </summary>
        public P Object2 { get; }

        /// <summary>
        /// Instantiate an object to build collision.
        /// </summary>
        /// <param name="object1">First object of collision.</param>
        /// <param name="object2">Second object of collision.</param>
        /// <remarks>
        /// <paramref name="object1"/> - First (resolving) object of collision.
        /// <para></para>
        /// <paramref name="object2"/> - Second (state) object of collision.
        /// </remarks>
        public Collision(T object1, P object2)
        {
            Object1 = object1;
            Object2 = object2;
        }
    }
}
