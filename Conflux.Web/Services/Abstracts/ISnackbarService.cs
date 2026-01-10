namespace Conflux.Services.Abstracts;

public interface ISnackbarService {
    event Action OnChange;
    IReadOnlyList<Snackbar> Snackbars { get; }
    
    void Create(SnackbarOptions options);
    bool Remove(Guid id);
    void Clear();
}