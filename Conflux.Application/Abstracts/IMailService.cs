namespace Conflux.Application.Abstracts;

public interface IMailService {
    Task SendAccountConfirmationEmailAsync(string receiverEmail, string confirmationUrl);
}