namespace AillieoUtils.EasyDirtyFlag
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    [Serializable]
    public sealed class DirtyFlagComp : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent action;
        [SerializeField]
        private CleanupMode mode;
        [SerializeField]
        private int modeParameter;

        private DirtyFlag dirtyFlag;
        
        private void Awake()
        {
            this.dirtyFlag = new DirtyFlag(
                mode,
                modeParameter,
                () => action?.Invoke(),
                true);
        }

        private void OnEnable()
        {
            this.dirtyFlag.SetActive(true);
        }

        private void OnDisable()
        {
            this.dirtyFlag.SetActive(false);
        }

        private void OnDestroy()
        {
            this.dirtyFlag.Dispose();
        }
    }
}
