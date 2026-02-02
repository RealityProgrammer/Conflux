using Conflux.Web.Services.Abstracts;

namespace Conflux.Web.Core;

public readonly record struct Snackbar(Guid Id, SnackbarOptions Options);