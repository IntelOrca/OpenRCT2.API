using FluentEmail.Core;
using FluentEmail.Mailgun;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Configuration;

namespace OpenRCT2.API.Services
{
    public class Emailer
    {
        private const string SenderAddress = "no-reply@openrct2.io";
        private const string SenderName = "OpenRCT2.io";
        private readonly MailgunSender _sender;

        public Emailer(IOptions<EmailConfig> emailOptions)
        {
            var emailConfig = emailOptions.Value;
            _sender = new MailgunSender(emailConfig.Domain, emailConfig.Secret);
        }

        public IFluentEmail Email
        {
            get => new Email(
                FluentEmail.Core.Email.DefaultRenderer,
                _sender,
                SenderAddress,
                SenderName);
        }
    }
}
