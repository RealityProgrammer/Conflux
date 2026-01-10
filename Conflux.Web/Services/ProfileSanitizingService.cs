using Vereyon.Web;

namespace Conflux.Services;

public sealed class ProfileSanitizingService {
    private readonly HtmlSanitizer _sanitizer;

    public ProfileSanitizingService() {
        _sanitizer = HtmlSanitizer.SimpleHtml5Sanitizer();
        _sanitizer.Tag("u").RemoveEmpty();
        _sanitizer.Tag("del").RemoveEmpty();
    }

    public string Sanitize(string htmlString) {
        return _sanitizer.Sanitize(htmlString);
    }
}