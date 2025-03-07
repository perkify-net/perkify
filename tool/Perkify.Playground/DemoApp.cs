namespace Perkify.Playgournd;

using System.Text.RegularExpressions;
using CommandLine;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Testing;
using NodaTime.Text;
using Spectre.Console;

public class DemoApp
{
    private Mode mode = Mode.Subscription;

    private Subscription? subscription;

    private readonly FakeClock clock = new FakeClock(DateTime.UtcNow.ToInstant());

    public DemoApp Welcome()
    {
        AnsiConsole.MarkupLine("Welcome to [red]Perkify.Demo[/] Playground.");
        AnsiConsole.MarkupLine("This is a DEMO CLI application running in REPL mode.");
        AnsiConsole.MarkupLine("");
        return this;
    }

    public DemoApp Help()
    {
        var help = new StringWriter();
        var parser = new Parser(with =>
        {
            with.HelpWriter = help;
            with.AutoVersion = false;
            with.IgnoreUnknownArguments = true;
        });
        parser.ParseArgumentsInMode(this.mode, ["--help"]);
        var helpText = help.ToString();
        helpText = Regex.Replace(helpText, @"^\s*$", string.Empty, RegexOptions.Multiline);
        AnsiConsole.MarkupLine("USAGE:");
        AnsiConsole.MarkupLine(help.ToString());
        return this;
    }

    public void Repl()
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>("[green]$[/]");
            Parser.Default
                .ParseArgumentsInMode(this.mode, input.Split(' '))
                .WithParsedInMode(this.mode, this)
                .WithNotParsed(errors => AnsiConsole.MarkupLine("[red]Invalid command. Try again![/]"));
        }
    }

    #region Subscription Commands

    public void CreateSubscription(CreateSubscriptionOptions opts)
    {
        this.subscription = new Subscription(this.clock, opts.Renewal, opts.Grace);
    }

    public void RenewSubscription(RenewSubscriptionOptions opts)
    {
        if (this.subscription is null)
        {
            AnsiConsole.MarkupLine("[red]No subscription found![/]");
            return;
        }

        /*
        if (!this.subscription.expiry.IsActive)
        {
            AnsiConsole.MarkupLine($"[red]The subscription has already been suspended.[/]");
            return;
        }
        */

        this.subscription.Renew(opts.Count);
        AnsiConsole.MarkupLine("[green]Subscription renewed![/]");
    }

    public void DeactivateSubscription(DeactivateSubscriptionOptions opts)
    {
        if (this.subscription is null)
        {
            AnsiConsole.MarkupLine("[red]No subscription found![/]");
            return;
        }

        /*
        if (!this.subscription.expiry.IsActive)
        {
            AnsiConsole.MarkupLine($"[red]The subscription has already been suspended.[/]");
            return;
        }
        */

        this.subscription.Deactivate();
        AnsiConsole.MarkupLine("[green]Subscription suspended![/]");
    }

    public void ActivateSubscription(ActivateSubscriptionOptions opts)
    {
        if (this.subscription is null)
        {
            AnsiConsole.MarkupLine("[red]No subscription found![/]");
            return;
        }

        this.subscription.Activate(opts.Extended);
        AnsiConsole.MarkupLine("[green]Subscription resumed![/]");
    }

    public void CancelSubscription(CancelSubscriptionOptions opts)
    {
        if (this.subscription is null)
        {
            AnsiConsole.MarkupLine("[red]No subscription found![/]");
            return;
        }

       this.subscription.Cancel(opts.Refund);
        AnsiConsole.MarkupLine("[green]Subscription cancelled![/]");
    }

    public void ViewSubscription(ViewSubscriptionOptions opts)
    {
        if (subscription is null)
        {
            AnsiConsole.MarkupLine("[red]No subscription found![/]");
            return;
        }

        AnsiConsole.MarkupLine($"Subscription ID: [yellow]{this.subscription.id}[/]");
        AnsiConsole.MarkupLine($"Order ID: [yellow]{this.subscription.OrderId}[/]");
        AnsiConsole.MarkupLine($"Product ID: [yellow]{this.subscription.ProductId}[/]");
        AnsiConsole.MarkupLine($"Metadata: [yellow]{this.subscription.Metadata}[/]");
        AnsiConsole.MarkupLine($"Recurring Plan: [yellow]{this.subscription.RecurringPlan}[/]");
        AnsiConsole.MarkupLine($"Renewal: [yellow]{this.subscription.renewal}[/]");
        AnsiConsole.MarkupLine($"Grace: [yellow]{this.subscription.grace}[/]");
        AnsiConsole.MarkupLine($"Expiry.ExpiryUtc: [yellow]{this.subscription.expiry.ExpiryUtc}[/]");
        AnsiConsole.MarkupLine($"Expiry.Eligible: [yellow]{this.subscription.expiry.IsEligible}[/]");
//            AnsiConsole.MarkupLine($"Expiry.IsActive: [yellow]{this.subscription.expiry.IsActive}[/]");
        AnsiConsole.MarkupLine($"Expiry.Remaining: [yellow]{this.subscription.expiry.Remaining}[/]");
        AnsiConsole.MarkupLine($"Expiry.Ovedue: [yellow]{this.subscription.expiry.Overdue}[/]");
    }

    #endregion

    #region Faked Clock Commands

    public void ViewFakedClock(ViewFakedClockOptions opts)
    {
        var fakedNow = this.clock.GetCurrentInstant();
        var prompt = opts.Local ? "Local" : "UTC";
        var zone = opts.Local ? fakedNow.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()) : fakedNow.InUtc();
        AnsiConsole.MarkupLine($"{prompt} Time: [yellow]{zone}[/]");
    }

    public void AdvanceFakedClock(AdvanceFakedClockOptions opts)
    {
        var duration = PeriodPattern.NormalizingIso.Parse(opts.Duration).Value.ToDuration();
        this.clock.Advance(duration);

        AnsiConsole.MarkupLine("[green]Faked Clock advanced![/]");
    }

    public void ResetFakedClock(ResetFakedClockOptions opts)
    {
        var nowUtc = opts.FakedNowUtc.HasValue
            ? opts.FakedNowUtc.Value.ToInstant()
            : SystemClock.Instance.GetCurrentInstant();
        this.clock.Reset(nowUtc);

        AnsiConsole.MarkupLine("[green]Faked Clock synchornized![/]");
    }

    #endregion

    #region General Commands: Mode, Exit

    public void Switch(SwitchModeOptions options)
    {
        AnsiConsole.WriteLine($"Current REPL mode is [yellow]{this.mode}[/] mode.");
        if (options.Mode.HasValue)
        {
            this.mode = options.Mode.Value;
            AnsiConsole.WriteLine($"Switch to [yellow]{this.mode}[/] mode.");
        }
    }

    public void Exit()
    {
        Console.WriteLine("Exiting REPL...");
        Console.WriteLine("Goodbye!");
        Environment.Exit(0);
    }

    #endregion
}
