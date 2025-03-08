namespace LitraLand.Web.Tasks
{
    public class HangfireTasks
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;

        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public HangfireTasks(ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IWhatsAppClient whatsAppClient,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _whatsAppClient = whatsAppClient;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

        public async Task PrepareExpirationAlerts()
        {
            var expirationDate = DateTime.Today.AddDays(5);

            var subscribers = await _context.Subscribers
                .Include(s => s.Subscriptions)
                .Where(s => s.Subscriptions.Any() && s.Subscriptions.Max(x => x.EndDate) == expirationDate)
                .ToListAsync();

            var emailTasks = new List<Task>();
            var whatsappTasks = new List<Task>();

            foreach (var subscriber in subscribers)
            {
                var latestSubscriptionEndDate = subscriber.Subscriptions.Max(x => x.EndDate).ToString("dd MMM, yyyy");

                var placeholders = new Dictionary<string, string>
                {
                    { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1741402646/calendar_zfohjc_vdrflq.png" },
                    { "header", $"Hello {subscriber.FirstName}," },
                    { "body", $"Your subscription is set to expire on {latestSubscriptionEndDate}.😔 " +
                              $"Renew now to continue enjoying our services without interruption. " }
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);
                emailTasks.Add(_emailSender.SendEmailAsync(subscriber.Email, "Subscription Expiration Reminder!!", body));

                if (subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>
                            {
                                new WhatsAppTextParameter { Text = subscriber.FirstName },
                                new WhatsAppTextParameter { Text = latestSubscriptionEndDate }
                            }
                        }
                    };

                    var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : subscriber.PhoneNumber;
                    whatsappTasks.Add(_whatsAppClient.SendMessage(
                        $"2{phoneNumber}",
                        WhatsAppLanguageCode.English,
                        WhatsAppTemplates.SubscriptionExpirationReminder,
                        components
                    ));
                }
            }

            await Task.WhenAll(emailTasks);
            await Task.WhenAll(whatsappTasks);
        }
    }
}
