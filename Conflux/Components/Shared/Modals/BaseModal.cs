using Conflux.Services;
using Microsoft.AspNetCore.Components;

namespace Conflux.Components.Shared.Modals;

public abstract class BaseModal : ComponentBase, IModalComponent {
    [Inject] protected ModalService ModalService { get; set; } = null!;

    [CascadingParameter] private IModalInstance Instance { get; set; } = null!;
    
    public void CloseModal(object? returnValue = null) {
        ModalService.Close(Instance.Id, returnValue);
    }

    void IModalComponent.StateHasChanged() {
        StateHasChanged();
    }
}