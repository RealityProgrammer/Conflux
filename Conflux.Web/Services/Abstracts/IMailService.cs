namespace Conflux.Web.Services.Abstracts;

public interface IMailService {
    Task SendAccountConfirmationEmailAsync(string receiverEmail, string confirmationUrl);
}