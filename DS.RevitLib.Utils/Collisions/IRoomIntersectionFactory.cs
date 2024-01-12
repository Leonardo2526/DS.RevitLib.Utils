using Autodesk.Revit.DB.Architecture;
using DS.ClassLib.VarUtils.Collisons;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions
{
    /// <inheritdoc/>
    public interface IRoomIntersectionFactory<P> :
        ITIntersectionFactory<Room, P>
    { }
}
