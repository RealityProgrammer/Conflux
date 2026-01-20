using Microsoft.AspNetCore.Components.Forms;

namespace Conflux.Web.Components.Shared;

public sealed class AccumulatedInputFileChangeEventArgs : EventArgs {
    public readonly InputFileChangeEventArgs ChangeEventArgs;
    public readonly InputFile Owner;

    public AccumulatedInputFileChangeEventArgs(InputFileChangeEventArgs changeEventArgs, InputFile owner) {
        ChangeEventArgs = changeEventArgs;
        Owner = owner;
    }
}