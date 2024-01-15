using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// The interface is used to create objects to check items traversability through <see cref="Rooms"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRoomTraverable<T>
    {
        /// <summary>
        /// <see cref="Room"/>s to check traversability.
        /// </summary>
        IEnumerable<Room> Rooms { get; }

        /// <summary>
        /// Check if <paramref name="item"/> can be traversed through <see cref="Rooms"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> can be traversed through <see cref="Rooms"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        bool IsTraversable(T item);
    }
}
