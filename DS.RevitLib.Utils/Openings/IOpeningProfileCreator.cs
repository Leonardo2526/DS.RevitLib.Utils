using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Openings
{
    /// <summary>
    /// The interface is used to extrude profile to <typeparamref name="T"/> object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProfileExtruder<T>
    {
        /// <summary>
        /// Extrude profile to <typeparamref name="T"/> object.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>
        /// <typeparamref name="T"/> that represents the extrusion of <paramref name="profile"/>.
        /// </returns>
        T TryExtrude(CurveLoop profile);
    }

    /// <summary>
    /// The interface is used to create profile of opening on the <see cref="Wall"/>.
    /// </summary>
    /// <typeparam name="TIntersectItem"></typeparam>
    public interface IOpeningProfileCreator<TIntersectItem>
    {
        /// <summary>
        /// Create profile of opening by <typeparamref name="TIntersectItem"/> on the <paramref name="wall"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="intersectItem"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.CurveLoop"/> that represents opening edges 
        /// to cross the <paramref name="wall"/> with <paramref name="intersectItem"/>.
        /// </returns>
        CurveLoop CreateProfile(Wall wall, TIntersectItem intersectItem);
    }

    /// <summary>
    /// The builder to create openings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TIntersectItem"></typeparam>
    public interface IOpeningBuilder<T, TIntersectItem> :
        IOpeningProfileCreator<TIntersectItem>, IProfileExtruder<T>
    { }

}
