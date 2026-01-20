using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Services;

public readonly record struct Snackbar(Guid Id, SnackbarOptions Options);