using Conflux.Web.Services.Abstracts;
using MailKit.Net.Smtp;
using MimeKit;

namespace Conflux.Web.Services.Implementations;

internal sealed class MailService(IConfiguration config) : IMailService {
    public async Task SendAccountConfirmationEmailAsync(string receiverEmail, string confirmationUrl) {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(config["MailSettings:SenderName"], config["MailSettings:SenderEmail"]!));
        email.To.Add(MailboxAddress.Parse(receiverEmail));
        email.Subject = "Account Confirmation code for Conflux";
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = $"""
                   <div style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                       <h2 style="color: #333;">Confirm Your Account</h2>
                       <p style="color: #555; line-height: 1.6;">
                           Welcome to Conflux! To get started and join the conversation, please confirm your email address by clicking the button below.
                       </p>
                       
                       <table width="100%" cellspacing="0" cellpadding="0">
                           <tr>
                               <td align="center" style="padding: 20px 0;">
                                   <table cellspacing="0" cellpadding="0">
                                       <tr>
                                           <td align="center" style="border-radius: 5px;" bgcolor="#0d6efd">
                                               <a href="{confirmationUrl}" target="_blank" style="padding: 12px 24px; border: 1px solid #0d6efd; border-radius: 5px; font-family: Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; font-weight: bold; display: inline-block;">
                                                   Confirm Email Address
                                               </a>
                                           </td>
                                       </tr>
                                   </table>
                               </td>
                           </tr>
                       </table>
                   
                       <p style="color: #777; font-size: 12px; margin-top: 30px; text-align: center;">
                           If you didn't create a Conflux account, you can safely ignore this email.
                       </p>
                   </div>
                   """,
        };

        using var smtp = new SmtpClient();

        try {
            await smtp.ConnectAsync(config["MailSettings:Server"], int.Parse(config["MailSettings:Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(config["MailSettings:SenderEmail"], config["MailSettings:Password"]);
            await smtp.SendAsync(email);
        } finally {
            await smtp.DisconnectAsync(true);
        }
    }
}