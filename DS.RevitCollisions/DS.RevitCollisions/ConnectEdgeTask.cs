using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers.ResolveTasks;
using DS.GraphUtils.Entities;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    internal class ConnectEdgeTask : ConnectTask<IEdge<IVertex>>
    {
    }
}
