using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve.TaskCreators;
using DS.RevitCollisions.Resolve.TaskResolvers;

namespace DS.RevitCollisions.Resolve.ResolveFactories
{
    internal class DummyFactoryBuilder : FactoryBuilderBase<IMEPCollision, string, string>
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
