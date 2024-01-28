namespace AillieoUtils.EasyDirtyFlag
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    internal sealed class DirtyFlagCleaner : MonoBehaviour
    {
        private readonly Queue<DirtyFlag> pendingAdd = new Queue<DirtyFlag>();
        private bool isRunning = false;
        private int anyDead = 0;

        private readonly List<DirtyFlag> updateBuffer = new List<DirtyFlag>();
        private readonly List<DirtyFlag>lateUpdateBuffer = new List<DirtyFlag>();
        private readonly List<DirtyFlag> fixedUpdateBuffer = new List<DirtyFlag>();
        private readonly List<DirtyFlag> nextXFramesBuffer = new List<DirtyFlag>();
        private readonly List<DirtyFlag> nextXSecondsBuffer = new List<DirtyFlag>();

        private static DirtyFlagCleaner instance;
        public static DirtyFlagCleaner Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject();
                    instance = go.AddComponent<DirtyFlagCleaner>();
                }

                return instance;
            }
        }

        internal void Register(DirtyFlag dirtyFlag, bool weakRef)
        {
            if(isRunning)
            {
                this.pendingAdd.Enqueue(dirtyFlag);
            }
            else
            {
                this.ProcessNewFlag(dirtyFlag);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
            }
        }

        private void Update()
        {
            isRunning = true;

            foreach(var flag in updateBuffer)
            {
                if (flag.Dead)
                {
                    anyDead &= (1 << (int)CleanupMode.Update);
                }
                else if (flag.Active)
                {
                    flag.EnsureUpdated();
                }
            }

            foreach(var flag in nextXFramesBuffer)
            {
                if (flag.Dead)
                {
                    anyDead &= (1 << (int)CleanupMode.NextXFrames);
                }
                else if (flag.Active)
                {
                    flag.modeParameter0++;
                    if (flag.modeParameter0 > flag.modeParameter)
                    {
                        flag.modeParameter0 = 0;
                        flag.EnsureUpdated();
                    }
                }
            }

            int dt = Mathf.RoundToInt(Time.deltaTime * 1000);
            foreach(var flag in nextXSecondsBuffer)
            {
                if (flag.Dead)
                {
                    anyDead &= (1 << (int)CleanupMode.NextXSeconds);
                }
                else if (flag.Active)
                {
                    flag.modeParameter0 += dt;
                    if (flag.modeParameter0 > flag.modeParameter)
                    {
                        flag.modeParameter0 = 0;
                        flag.EnsureUpdated();
                    }
                }
            }
            
            isRunning = false;
        }

        private void FixedUpdate()
        {
            isRunning = true;

            foreach(var flag in fixedUpdateBuffer)
            {
                if (flag.Dead)
                {
                    anyDead &= (1 << (int)CleanupMode.NextXSeconds);
                }
                else if (flag.Active)
                {
                    flag.EnsureUpdated();
                }
            }

            isRunning = false;
        }

        private void LateUpdate()
        {
            isRunning = true;

            foreach(var flag in lateUpdateBuffer)
            {
                if (flag.Dead)
                {
                    anyDead &= (1 << (int)CleanupMode.NextXSeconds);
                }
                else if (flag.Active)
                {
                    flag.EnsureUpdated();
                }
            }

            isRunning = false;

            this.ProcessPendingNewFlags();
            this.RemoveDead();
        }

        private void ProcessPendingNewFlags()
        {
            while (this.pendingAdd.Count > 0)
            {
                ProcessNewFlag(this.pendingAdd.Dequeue());
            }
        }

        private void ProcessNewFlag(DirtyFlag dirtyFlag)
        {
            switch (dirtyFlag.mode)
            {
                case CleanupMode.Update:
                    this.updateBuffer.Add(dirtyFlag);
                    break;
                case CleanupMode.LateUpdate:
                    this.lateUpdateBuffer.Add(dirtyFlag);
                    break;
                case CleanupMode.FixedUpdate:
                    this.fixedUpdateBuffer.Add(dirtyFlag);
                    break;
                case CleanupMode.NextXFrames:
                    this.nextXFramesBuffer.Add(dirtyFlag);
                    break;
                case CleanupMode.NextXSeconds:
                    this.nextXSecondsBuffer.Add(dirtyFlag);
                    break;
                default:
                    break;
            }
        }
        
        private void RemoveDead()
        {
            if (anyDead != 0)
            {
                if ((anyDead & (1 << (int)CleanupMode.Update)) != 0)
                {
                    updateBuffer.RemoveAll(df => df.Dead);
                }

                if ((anyDead & (1 << (int)CleanupMode.LateUpdate)) != 0)
                {
                    lateUpdateBuffer.RemoveAll(df => df.Dead);
                }
                
                if ((anyDead & (1 << (int)CleanupMode.FixedUpdate)) != 0)
                {
                    fixedUpdateBuffer.RemoveAll(df => df.Dead);
                }
                
                if ((anyDead & (1 << (int)CleanupMode.NextXFrames)) != 0)
                {         
                    nextXFramesBuffer.RemoveAll(df => df.Dead);
                }
                
                if ((anyDead & (1 << (int)CleanupMode.NextXSeconds)) != 0)
                {
                    nextXSecondsBuffer.RemoveAll(df => df.Dead);
                }
                
                anyDead = 0;
            }
        }
    }
}
