using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Resolve.TaskResolvers
{
    internal class DummyTaskResolver : ITaskResolver<string, string>
    {
        public IEnumerable<string> Results => throw new NotImplementedException();

        public string TryResolve(string task)
        {
            return "Resolved";
        }

        public Task<string> TryResolveAsync(string task)
        {
            var result = Task.Run(() => "Resolved");
            return result;
        }
    }
}
