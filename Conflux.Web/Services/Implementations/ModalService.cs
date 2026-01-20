using Conflux.Web.Components.Shared.Modals;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Web.Services.Implementations;

public sealed class ModalChangedEventArgs : EventArgs {
    public IModalInstance Instance { get; }
    public object? ReturnValue { get; }
    public ModalChangedEventType Type { get; }

    internal ModalChangedEventArgs(IModalInstance instance, object? returnValue, ModalChangedEventType type) {
        Instance = instance;
        ReturnValue = returnValue;
        Type = type;
    }
}

public enum ModalChangedEventType {
    Added,
    Closed,
}

public sealed class ModalService {
    private readonly List<IModalInstanceInternal> _modals;
    internal IReadOnlyList<IModalInstanceInternal> Modals => _modals;
    
    public event Action<ModalChangedEventArgs>? OnModalChanged;

    public ModalService() {
        _modals = [];
    }
    
    public IModalInstance Open<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? name = null, IDictionary<string, object?>? parameters = null) where T : BaseModal {
        Type type = typeof(T);
        ModalInstance instance = new(Guid.CreateVersion7(), name ?? type.FullName ?? type.Name, type, parameters!);
        _modals.Add(instance);
        
        OnModalChanged?.Invoke(new(instance, null, ModalChangedEventType.Added));

        return instance;
    }

    public bool Close(Guid modalId, object? returnValue = null) {
        for (int i = 0; i < _modals.Count; i++) {
            if (_modals[i].Id == modalId) {
                var info = _modals[i];
                
                _modals.RemoveAt(i);
                OnModalChanged?.Invoke(new(info, returnValue, ModalChangedEventType.Closed));
                return true;
            }
        }

        return false;
    }
}

file sealed class ModalInstance : IModalInstanceInternal {
    public Guid Id { get; }
    public string Name { get; }
    public Type ModalType { get; }
    public IDictionary<string, object?> Parameters { get; set; }
    public IModalComponent? Component { get; set; }

    public ModalInstance(Guid id, string name, Type modalType, IDictionary<string, object?> parameters) {
        Id = id;
        Name = name;
        ModalType = modalType;
        Parameters = parameters;
    }
    
    public void ReplaceParameter(string key, object? value) {
        if (Parameters.ContainsKey(key)) {
            Parameters[key] = value;
            Component?.StateHasChanged();
        }
    }
    
    public void OverwriteParameters(IDictionary<string, object?> parameters) {
        Parameters = parameters;
        Component?.StateHasChanged();
    }
}