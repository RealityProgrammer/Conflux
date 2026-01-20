namespace Conflux.Web.Components.Shared.Modals;

public interface IModalInstanceInternal : IModalInstance {
    IModalComponent? Component { get; set; }
}