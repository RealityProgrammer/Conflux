using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Conflux.Components.Pages.Settings;

public sealed class ProfileInputModel {
    [StringLength(32, MinimumLength = 8), Required] public required string DisplayName { get; set; }
    [StringLength(maximumLength: 32)] public string? Pronouns { get; set; }
    [StringLength(255)] public string? Bio { get; set; }

    public AvatarModel Avatar { get; set; } = new();
    
    public sealed class AvatarModel : INotifyPropertyChanged {
        private IBrowserFile? _file;
        private bool _requestDelete;
        
        public IBrowserFile? File {
            get => _file;
            set {
                _file = value;
                OnPropertyChanged();
            }
        }

        public bool RequestDelete {
            get => _requestDelete;
            set {
                if (_requestDelete != value) {
                    _requestDelete = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}