using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs.Validators
{
    public interface IVertexValidatorSet : IEnumerable<IValidator<IVertex>>
    {
        IVertexValidatorSet Create();
    }
}
