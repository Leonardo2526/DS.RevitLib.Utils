using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DS.RVT.ModelSpaceFragmentation
{
    interface IElement
    {
        Element GetElement();
    }

    interface IOutputElementInfo
    {
        void GetInfo();

    }

    interface IModifyElement
    {
       
    }
}
