using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Collections.Frozen;

namespace SomeChattingPlatform.Components.Services;

internal sealed class ApplicationRedirectManager(NavigationManager navigationManager, ProtectedSessionStorage sessionStorage)  {
    private static readonly CookieBuilder ShortLivedCookieBuilder = new() {
        SameSite = SameSiteMode.Strict,
        HttpOnly = true,
        IsEssential = true,
        MaxAge = TimeSpan.FromSeconds(5),
    };

    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly ProtectedSessionStorage _sessionStorage = sessionStorage;

    public RedirectBuilder To(string? uri) {
        uri ??= "";

        // Prevent open redirects.
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative)) {
            uri = _navigationManager.ToBaseRelativePath(uri);
        }
        
        return new(this, uri);
    }

    public struct RedirectBuilder {
        private readonly ApplicationRedirectManager _manager;
        private string _uri;
        private IReadOnlyDictionary<string, object?> _queryParameters;

        internal RedirectBuilder(ApplicationRedirectManager manager, string uri) {
            _manager = manager;
            _uri = uri;
            _queryParameters = FrozenDictionary<string, object?>.Empty;
        }

        public RedirectBuilder WithQuery(string key, object? value) {
            if (_queryParameters.Count == 0) {
                _queryParameters = new Dictionary<string, object?> {
                    [key] = value,
                };
            } else {
                ((Dictionary<string, object?>)_queryParameters)[key] = value;
            }
            
            return this;
        }

        public RedirectBuilder WithQueries(IEnumerable<KeyValuePair<string, object?>> queryParameters) {
            if (_queryParameters.Count == 0) {
                _queryParameters = queryParameters.ToDictionary();
            } else {
                foreach ((var key, var value) in queryParameters) {
                    ((Dictionary<string, object?>)_queryParameters)[key] = value;
                }
            }

            return this;
        }
        
        public void Execute() {
            var uriWithoutQuery = _manager._navigationManager.ToAbsoluteUri(_uri).GetLeftPart(UriPartial.Path);
            var fullUri = _manager._navigationManager.GetUriWithQueryParameters(uriWithoutQuery, _queryParameters);
            
            // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
            // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
            _manager._navigationManager.NavigateTo(fullUri);
        }
    }
    
    // [DoesNotReturn]
    // public void RedirectTo(string? uri) {
    //     uri ??= "";
    //
    //     // Prevent open redirects.
    //     if (!Uri.IsWellFormedUriString(uri, UriKind.Relative)) {
    //         uri = _navigationManager.ToBaseRelativePath(uri);
    //     }
    //
    //     // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
    //     // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
    //     _navigationManager.NavigateTo(uri);
    //     throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    // }
    //
    // [DoesNotReturn]
    // public void RedirectTo(string uri, Dictionary<string, object?> queryParameters) {
    //     var uriWithoutQuery = _navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
    //     var newUri = _navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
    //     RedirectTo(newUri);
    // }
    //
    // [DoesNotReturn]
    // public void RedirectToWithStatus(string uri, string message, HttpContext context) {
    //     context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
    //     RedirectTo(uri);
    // }
    //
    // private string CurrentPath => _navigationManager.ToAbsoluteUri(_navigationManager.Uri).GetLeftPart(UriPartial.Path);
    //
    // [DoesNotReturn]
    // public void RedirectToCurrentPage() => RedirectTo(CurrentPath);
    //
    // [DoesNotReturn]
    // public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
    //     => RedirectToWithStatus(CurrentPath, message, context);
}