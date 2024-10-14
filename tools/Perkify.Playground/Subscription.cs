namespace Perkify.Demo
{
    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Text;
    using Spectre.Console;

    using Perkify.Core;
    using System.Globalization;

    public class Subscription
    {
        private readonly IClock clock;

        public readonly long id;

        public readonly string renewal;

        public readonly string? grace;

        public readonly Expiry expiry;

        #region To simplify the demo, we are using faked data here to ignore dependencies (sub system): Product, Order & Recurring.

        public string OrderId => "Faked Order 123456";

        public string UserId => "Faked User 48526S";

        public string ProductId => "Faked Product 123236W";

        public string Metadata => "Faked Metadata ABC snapshoted/linking from product catalog...";


        public string RecurringPlan = "Faked Recurring Plan xyz snapshoted/linking from product catalog...";

        #endregion

        public Subscription(IClock clock, string renewal, string? grace)
        {
            this.clock = clock;
            this.id = Random.Shared.NextInt64(1001, 1999);
            this.renewal = renewal;
            this.grace = grace;

            this.expiry = new Expiry
            (
                expiryUtc: DateTime.UtcNow,
                grace: grace != null ? TimeSpan.Parse(grace, CultureInfo.InvariantCulture) : null
            );
            expiry.Renew(this.renewal);
        }

        public void Renew(int count = 1)
        {
            Enumerable.Repeat(() => this.expiry.Renew(this.renewal), count);
        }

        public void Deactivate()
        {
            //this.expiry.Deactivate();
        }

        public void Activate(bool extended)
        {
            /*
            if (this.subscription.expiry.IsActive)
            {
                AnsiConsole.MarkupLine($"[red]Please suspend the subscription.[/]");
                return;
            }

            this.expiry.Activate(extended: extended);
            */
        }

        public void Cancel(bool refund)
        {
            /*
            if (this.subscription.expiry.IsActive)
            {
                AnsiConsole.MarkupLine($"[red]Please suspend the subscription.[/]");
                return;
            }
            */

            if (refund)
            {
                /*
                var remaining = this.expiry.Remaining;
//                var duration = PeriodPattern.NormalizingIso.Parse(this.Renewal.Renewal).Value;
                var expiryUtc = this.expiry.ExpiryUtc.ToInstant().InUtc().LocalDateTime;
                var originUtc = expiryUtc - duration;
                var total = expiryUtc - originUtc;
                var price = 20;
                var currency = "USD";
                var ratio = 1.0 * remaining.Days / total.Days;
                AnsiConsole.MarkupLine($"Refunding started...");
                AnsiConsole.MarkupLine($"Remaining: [yellow]{this.expiry.Remaining}[/]");
//                AnsiConsole.MarkupLine($"Renewal: [yellow]{this.Renewal.Renewal}[/]");
//                AnsiConsole.MarkupLine($"Calendar: [yellow]{this.Renewal.Calendar}[/]");
                AnsiConsole.MarkupLine($"Ratio: [yellow]{ratio * 100}%[/]");
                AnsiConsole.MarkupLine($"Price: [yellow]{price} {currency}[/]");
                AnsiConsole.MarkupLine($"Refund: [yellow]{price * ratio} {currency}[/]");
                */
                AnsiConsole.MarkupLine($"Refunding completed...");
            }
        }
    }
}
