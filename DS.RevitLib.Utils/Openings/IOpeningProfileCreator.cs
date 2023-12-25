using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Openings
{
    public interface IProfileExtruder<T>
    {
        T TryExtrude(CurveLoop profile);
    }

    public interface IOpeningProfileCreator
    {
        CurveLoop CreateProfile(Wall wall, MEPCurve mEPCurve);
    }

    public interface IOpeningBuilder<T> : IOpeningProfileCreator, IProfileExtruder<T>
    { }

}
