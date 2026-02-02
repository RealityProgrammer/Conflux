using Conflux.Web.Core;
using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Services.Implementations;

public class SnackbarService : ISnackbarService {
    public event Action? OnChange;
    private readonly List<Snackbar> _snackbars = [];

    public IReadOnlyList<Snackbar> Snackbars => _snackbars;

    public void Create(SnackbarOptions options) {
        _snackbars.Add(new(Guid.CreateVersion7(), options));
        NotifyChanged();
    }

    public bool Remove(Guid id) {
        for (int i = 0; i < _snackbars.Count; i++) {
            if (_snackbars[i].Id == id) {
                _snackbars.RemoveAt(i);
                NotifyChanged();
                return true;
            }
        }

        return false;
    }

    public void Clear() {
        _snackbars.Clear();
        NotifyChanged();
    }

    private void NotifyChanged() {
        OnChange?.Invoke();
    }
}