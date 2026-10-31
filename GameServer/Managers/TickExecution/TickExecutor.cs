using System;
using System.Collections.Generic;

namespace DOL.GS
{
    public readonly record struct TickExecutorInitParams(Action<Exception> OnFatalError, Func<long> TickSource);

    public abstract class TickExecutor : IDisposable
    {
        [ThreadStatic] private static TickObjectPoolManager _tickObjectPoolManager;
        [ThreadStatic] private static TickListPoolManager _tickListPoolManager;
        [ThreadStatic] private static long _lastResetTick;

        protected readonly Action<Exception> _onFatalError;
        private readonly Func<long> _tickSource;

        protected TickExecutor(TickExecutorInitParams initParams)
        {
            ArgumentNullException.ThrowIfNull(initParams.OnFatalError);
            ArgumentNullException.ThrowIfNull(initParams.TickSource);

            _onFatalError = initParams.OnFatalError;
            _tickSource = initParams.TickSource;
            InitThreadStatics();
        }

        public virtual void Start() { }

        public abstract void ExecuteForEach<T>(List<T> items, int toExclusive, Action<T> action);
        public abstract void ExecuteForEachSharded<T>(List<T>[] shards, int[] shardStartIndices, int totalCount, Action<T> action);
        public abstract void Dispose();

        public T GetObjectForTick<T>() where T : ITickPooledObject<T>, new()
        {
            return _tickObjectPoolManager != null ? _tickObjectPoolManager.GetForTick<T>() : new();
        }

        public List<T> GetListForTick<T>() where T : ITickPooledList<T>
        {
            return _tickListPoolManager != null ? _tickListPoolManager.GetForTick<T>() : new();
        }

        protected virtual void InitWorker(object obj)
        {
            InitThreadStatics();
        }

        private void InitThreadStatics()
        {
            _tickObjectPoolManager = new();
            _tickListPoolManager = new();
            _lastResetTick = -1;
        }

        protected void CheckResetTick()
        {
            long tick = _tickSource();

            if (_lastResetTick == tick)
                return;

            _tickObjectPoolManager.Reset();
            _tickListPoolManager.Reset();
            _lastResetTick = tick;
        }
    }
}
