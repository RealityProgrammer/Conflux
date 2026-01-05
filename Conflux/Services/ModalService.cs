using Microsoft.AspNetCore.Components;

namespace Conflux.Services;

public sealed class ModalService {
    private readonly List<ModalInfo> _modals;
    public IReadOnlyList<ModalInfo> Modals => _modals;
    
    public event Action? OnModalChanged;

    public ModalService() {
        _modals = [];
    }
    
    public ModalInfo Add<T>(IDictionary<string, object>? parameters = null) where T : ComponentBase {
        var info = new ModalInfo(Guid.CreateVersion7(), typeof(T), parameters);
        
        _modals.Add(info);
        OnModalChanged?.Invoke();

        return info;
    }

    public bool Close(Guid modalId) {
        for (int i = 0; i < _modals.Count; i++) {
            if (_modals[i].Id == modalId) {
                _modals.RemoveAt(i);
                OnModalChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public readonly record struct ModalInfo(Guid Id, Type Type, IDictionary<string, object>? Parameters);
}