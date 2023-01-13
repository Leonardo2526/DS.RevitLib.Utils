namespace DS.RevitLib.Utils.Transforms
{
    public abstract class TransformBuilder
    {
        protected readonly object _sourceObject;
        protected readonly object _targetObject;

        protected TransformBuilder(object sourceObject, object targetObject)
        {
            _sourceObject = sourceObject;
            _targetObject = targetObject;
        }

        public abstract TransformModel Build();
    }
}
