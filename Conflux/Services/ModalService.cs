using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace Conflux.Services;

public sealed class ModalChangedEventArgs : EventArgs {
    public ModalService.ModalInfo Info { get; }
    public object? ReturnValue { get; }
    public ModalChangedEventType Type { get; }

    internal ModalChangedEventArgs(ModalService.ModalInfo info, object? returnValue, ModalChangedEventType type) {
        Info = info;
        ReturnValue = returnValue;
        Type = type;
    }
}

public enum ModalChangedEventType {
    Added,
    Closed,
}

public sealed class ModalService {
    private readonly List<ModalInfo> _modals;
    public IReadOnlyList<ModalInfo> Modals => _modals;
    
    public event Action<ModalChangedEventArgs>? OnModalChanged;

    public ModalService() {
        _modals = [];
    }
    
    public ModalInfo Open<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string? name = null, IDictionary<string, object>? parameters = null) where T : ComponentBase {
        Type type = typeof(T);
        var info = new ModalInfo(Guid.CreateVersion7(), name ?? type.FullName ?? type.Name, type, parameters);
        
        _modals.Add(info);
        OnModalChanged?.Invoke(new(info, null, ModalChangedEventType.Added));

        return info;
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

    public readonly record struct ModalInfo(Guid Id, string Name, Type Type, IDictionary<string, object>? Parameters);
}