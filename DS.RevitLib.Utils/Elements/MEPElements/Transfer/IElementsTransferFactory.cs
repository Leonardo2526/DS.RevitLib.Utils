using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.MEPElements.Transfer
{  
    /// <summary>
    /// The interface used to create factories to transfer <see cref="Autodesk.Revit.DB.Element"/>'s to <see cref="MEPSystemModel"/>.
    /// </summary>  
    public interface IElementsTransferFactory : ITransferFactory<List<Element>, MEPSystemModel>
    { }

    /// <summary>
    /// The interface used to create factories to transfer <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s to <see cref="MEPSystemModel"/>.
    /// </summary>  
    public interface IFamilyInstTransferFactory : ITransferFactory<List<FamilyInstance>, MEPSystemModel>
    { }
}
