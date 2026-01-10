using Conflux.Services.Abstracts;

namespace Conflux.Services;

public readonly record struct Snackbar(Guid Id, SnackbarOptions Options);