namespace Perkify.Demo
{
    using CommandLine;

    [Verb("switch", HelpText = "Switch across different REPL modes: Subscription, PayAsYouGo, Coupon or Clock")]
    public class SwitchModeOptions
    {
        [Option('m', "mode", Required = false, Default = null, HelpText = "Demo mode.")]
        public Mode? Mode { get; set; }
    }
}
