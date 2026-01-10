namespace Conflux.Components.Shared.Modals;

public interface IModalInstance {
    Guid Id { get; }
    string Name { get; }
    Type ModalType { get; }
    IDictionary<string, object?> Parameters { get; }
    
    void ReplaceParameter(string key, object? value);
    void OverwriteParameters(IDictionary<string, object?> parameters);
}