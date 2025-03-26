using System.Text;

namespace LitraLand.Web.Tasks
{
    public class HangfireTasks
    {
        private readonly IApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public HangfireTasks(IApplicationDbContext context,
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
                .Where(s => !s.IsBlackListed && s.Subscriptions.Any() && s.Subscriptions.Max(x => x.EndDate) == expirationDate)
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
        public async Task RentalsExpirationAlert()
        {
            var tomorrow = DateTime.Today.AddDays(1);

            var rentals = _context.Rentals
                    .Include(r => r.Subscriber)
                    .Include(r => r.RentalCopies)
                    .ThenInclude(c => c.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                    .Where(r => r.RentalCopies.Any(r => r.EndDate.Date == tomorrow && !r.ReturnDate.HasValue))
                    .ToList();

            var emailTasks = new List<Task>();
            var whatsappTasks = new List<Task>();

            foreach (var rental in rentals)
            {
                var expiredCopies = rental.RentalCopies.Where(c => c.EndDate.Date == tomorrow && !c.ReturnDate.HasValue).ToList();

                var message = new StringBuilder();
                message.AppendLine($"your rental for the below book(s) will be expired by tomorrow {tomorrow.ToString("dd MMM, yyyy")} 💔:");
                message.AppendLine("<ul>");

                foreach (var copy in expiredCopies)
                {
                    message.AppendLine($"<li>{copy.BookCopy!.Book!.Title}</li>");
                }
                message.AppendLine("</ul>");
                message.AppendLine("<br>📆 Please return the book(s) on time to avoid any late fees.<br>");
                message.AppendLine("🔄 Need an extension or have any questions? Feel free to reach out!<br>");
                message.AppendLine("📞 *Support:*+20 234 567 890<br>");
                message.AppendLine("📖 Happy Reading! ☺️");
                var placeholders = new Dictionary<string, string>()
                {
                    { "mediaUrl", "https://res.cloudinary.com/trojan74/image/upload/v1741402646/calendar_zfohjc_vdrflq.png" },
                    { "header", $"Hello {rental.Subscriber!.FirstName}," },
                    { "body", message.ToString() }
                };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);
                emailTasks.Add(_emailSender.SendEmailAsync(rental.Subscriber!.Email, "LitraLand Rental Expiration 🔔", body));

                if (rental.Subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>
                    {
                        new WhatsAppComponent
                        {
                            Type = "body",
                            Parameters = new List<object>
                            {
                                new WhatsAppTextParameter { Text = rental.Subscriber.FirstName },
                                new WhatsAppTextParameter { Text = tomorrow.ToString("dd MMM, yyyy") },
                                new WhatsAppTextParameter { Text = string.Join(", ", expiredCopies.Select(b => b.BookCopy!.Book!.Title)) },
                            }
                        }
                    };

                    var phoneNumber = _webHostEnvironment.IsDevelopment() ? "01129816608" : rental.Subscriber.PhoneNumber;
                    whatsappTasks.Add(_whatsAppClient.SendMessage(
                        $"2{phoneNumber}",
                        WhatsAppLanguageCode.English,
                        WhatsAppTemplates.RentalExpirationAlert,
                        components
                    ));
                }
            }

            //await Task.WhenAll(emailTasks);
            //await Task.WhenAll(whatsappTasks);
            await Task.WhenAll(emailTasks.Concat(whatsappTasks));
        }
    }
}