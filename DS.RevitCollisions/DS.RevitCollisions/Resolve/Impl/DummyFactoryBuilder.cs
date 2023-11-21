using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.TaskCreators;
using DS.RevitCollisions.TaskResolvers;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Impl
{
    internal class DummyFactoryBuilder : MEPCollisionFactoryBuilderBase<string, string>
    {
        protected override ITaskCreator<IMEPCollision, string> BuildTaskCreator()
        {
            return new DummyTaskCreator();
        }

        protected override ITaskResolver<string, string> BuildTaskResover()
        {
            return new DummyTaskResolver();
        }
    }
}
