namespace Perkify.Demo
{
    using CommandLine;

    [Verb("create", HelpText = "Create a new subscription.")]
    public class CreateSubscriptionOptions
    {
        [Option('d', "duration", Required = false, Default = "P1M", HelpText = "The recurring duration.")]
        public required string Duration { get; set; }

        [Option('c', "calendar", Required = false, Default = true, HelpText = "If the recurring duration is calendar based.")]
        public bool Calendar { get; set; }

        [Option('g', "grace", Required = false, Default = null, HelpText = "The grace period.")]
        public string? Grace { get; set; }
    }

    [Verb("renew", HelpText = "Renew the exisitng subscription.")]
    public class RenewSubscriptionOptions
    {
        [Option('c', "count", Required = false, Default = "1", HelpText = "The recurring interval.")]
        public int Count { get; set; }
    }

    [Verb("deactivate", HelpText = "Suspend the exisitng subscription.")]
    public class DeactivateSubscriptionOptions
    {
    }

    [Verb("activate", HelpText = "Resume the exisitng subscription.")]
    public class ActivateSubscriptionOptions
    {
        [Option('e', "extended", Required = false, Default = false, HelpText = "If the subscription should be extended.")]
        public bool Extended { get; set; }
    }

    [Verb("cancel", HelpText = "Cancel the suspended subscription.")]
    public class CancelSubscriptionOptions
    {
        [Option('r', "refund", Required = false, Default = true, HelpText = "If the subscription should be refunded.")]
        public bool Refund { get; set; }
    }

    [Verb("view", HelpText = "View the exisitng subscription.")]
    public class ViewSubscriptionOptions
    {
        [Option('v', "verbose", Required = false, Default = false, HelpText = "If additional details should be displayed.")]
        public bool Verbose { get; set; }
    }
}
