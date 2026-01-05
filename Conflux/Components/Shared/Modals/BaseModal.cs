using Conflux.Services;
using Microsoft.AspNetCore.Components;

namespace Conflux.Components.Shared.Modals;

public abstract class BaseModal : ComponentBase {
    [Inject] protected ModalService ModalService { get; set; } = null!;
    
    [CascadingParameter] private ModalService.ModalInfo ModalInfo { get; set; }

    public void CloseModal() {
        ModalService.Close(ModalInfo.Id);
    }
}