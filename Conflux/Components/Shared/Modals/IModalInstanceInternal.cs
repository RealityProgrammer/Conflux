namespace Conflux.Components.Shared.Modals;

public interface IModalInstanceInternal : IModalInstance {
    IModalComponent? Component { get; set; }
}