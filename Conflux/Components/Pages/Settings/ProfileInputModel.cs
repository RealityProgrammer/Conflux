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
        private double _zoomX = 1.0;
        private double _zoomY = 1.0;
        private IBrowserFile? _file;
        private bool _requestDelete;

        [Range(0.25, 5)]
        public double ZoomX {
            get => Math.Round(_zoomX, 2);
            set {
                if (Math.Abs(value - _zoomX) > 0.01) {
                    _zoomX = Math.Clamp(value, 0.25, 5);
                    OnPropertyChanged();
                }
            }
        }

        [Range(0.25, 5)]
        public double ZoomY {
            get => Math.Round(_zoomY, 2);
            set {
                if (Math.Abs(value - _zoomY) > 0.01) {
                    _zoomY = Math.Clamp(value, 0.25, 5);
                    OnPropertyChanged();
                }
            }
        }

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