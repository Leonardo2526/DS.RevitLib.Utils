namespace DS.RevitLib.Utils.MEP.SystemTree
{
    internal abstract class AbstractConnectedBuilder<T>
    {
        protected readonly T _element;

        public AbstractConnectedBuilder(T element)
        {
            _element = element;
        }

    }
}
