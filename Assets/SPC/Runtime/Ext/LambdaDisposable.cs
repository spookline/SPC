using System;

namespace Spookline.SPC.Ext {
    public readonly struct LambdaDisposable : IDisposable
    {
        private readonly Action _onDispose;
        public LambdaDisposable(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose?.Invoke();
    }
}