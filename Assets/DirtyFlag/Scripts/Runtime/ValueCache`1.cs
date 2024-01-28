namespace AillieoUtils.EasyDirtyFlag
{
    using System;

    public sealed class ValueCache<T> : IDisposable
    {
        private T result;
        private readonly DirtyFlag flag;

        public ValueCache(Func<T> func)
        {
            this.flag = new DirtyFlag(
                CleanupMode.Manual,
                0,
                () => this.result = func(),
                true);
        }

        public T GetResult()
        {
            this.flag.EnsureUpdated();
            return result;
        }

        public void SetDirty()
        {
            this.flag.SetDirty();
        }

        public void Dispose()
        {
            this.flag.Dispose();
        }

        public bool IsDirty => flag.IsDirty;
    }
}
