namespace Conflux.Helpers;

public sealed class Debouncer(Func<CancellationToken, Task> callback, TimeSpan delay) : IAsyncDisposable, IDisposable {
    private CancellationTokenSource? _cts;

    public TimeSpan Delay => delay;

    public async Task Start() {
        try {
            if (_cts != null) {
                await _cts.CancelAsync();
                _cts.Dispose();
            }

            _cts = new();
            await Task.Delay(delay, _cts.Token);
            await callback(_cts.Token);
            
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        } catch (TaskCanceledException) {
        }
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore();

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
        if (disposing) {
            if (_cts != null) {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    ~Debouncer() {
        Dispose(false);
    }

    private async ValueTask DisposeAsyncCore() {
        if (_cts != null) {
            await _cts.CancelAsync();
            _cts.Dispose();

            _cts = null;
        }
    }
}