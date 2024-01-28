namespace AillieoUtils.EasyDirtyFlag
{
    using System;
    using UnityEngine.Assertions;

    public sealed class DirtyFlag : IDisposable
    {
        internal readonly CleanupMode mode;
        internal readonly int modeParameter;
        internal int modeParameter0;

        private bool dirty;
        private Action action;

        private State state = State.Active;

        public static DirtyFlag Union(params DirtyFlag[] flags)
        {
            throw new NotImplementedException();
        }
        
        private enum State
        {
            Active,
            Deactivated,
            Dead,
        }

        public DirtyFlag(Action action, bool dirty = false)
            : this(CleanupMode.Update, 0, action, dirty)
        {
        }

        public DirtyFlag(CleanupMode mode, int modeParameter, Action action, bool dirty = false)
        {
            this.mode = mode;
            this.modeParameter = modeParameter;
            this.action = action;
            this.dirty = dirty;

            DirtyFlagCleaner.Instance.Register(this, false);
        }

        public void SetDirty()
        {
            Assert.AreNotEqual(State.Dead, this.state);

            this.dirty = true;
        }

        public void ForceUpdate()
        {
            Assert.AreNotEqual(State.Dead, this.state);


            this.dirty = false;

            try
            {
                this.action?.Invoke();
            }
            catch (Exception e)
            {
                this.dirty = false;
                UnityEngine.Debug.LogException(e);
            }
        }

        public void EnsureUpdated()
        {
            Assert.AreNotEqual(State.Dead, this.state);

            if (!dirty)
            {
                return;
            }

            this.dirty = false;

            try
            {
                this.action?.Invoke();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public void Dispose()
        {
            this.state = State.Dead;
            this.action = null;
        }

        public void SetActive(bool active)
        {
            Assert.AreNotEqual(State.Dead, this.state);
            this.state = active ? State.Active : State.Deactivated;
        }

        internal bool Active => this.state == State.Active;

        public bool Dead => this.state == State.Dead;

        public bool IsDirty => this.dirty;
    }
}
