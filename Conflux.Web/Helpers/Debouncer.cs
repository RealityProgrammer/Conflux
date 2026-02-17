namespace Conflux.Web.Helpers;

public sealed class Debouncer(Func<CancellationToken, Task> callback, TimeSpan delay) : IAsyncDisposable, IDisposable {
    private CancellationTokenSource? _cts;

    public TimeSpan Delay => delay;

    public async Task Start() {
        try {
            var newCancellationToken = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _cts, newCancellationToken);

            if (oldCts != null) {
                await oldCts.CancelAsync();
                oldCts.Dispose();
            }

            await Task.Delay(delay, newCancellationToken.Token);
            await callback(newCancellationToken.Token);
        } catch (TaskCanceledException) {
        } catch (OperationCanceledException) {
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
            var activeCts = Interlocked.Exchange(ref _cts, null);
            if (activeCts != null) {
                activeCts.Cancel();
                activeCts.Dispose();
            }
        }
    }

    ~Debouncer() {
        Dispose(false);
    }

    private async ValueTask DisposeAsyncCore() {
        var activeCts = Interlocked.Exchange(ref _cts, null);
        if (activeCts != null) {
            await activeCts.CancelAsync();
            activeCts.Dispose();
        }
    }
}