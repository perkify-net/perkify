namespace Perkify.Demo
{
    using CommandLine;

    [Verb("view", HelpText = "View the faked clock.")]
    public class ViewFakedClockOptions
    {
        [Option('l', "local", Required = false, Default = false, HelpText = "If the local time should be displayed.")]
        public required bool Local { get; set; }
    }

    [Verb("advance", HelpText = "Advance the faked clock.")]
    public class AdvanceFakedClockOptions
    {
        [Option('d', "duration", Required = false, Default = "P7D", HelpText = "The advancing duration.")]
        public required string Duration { get; set; }
    }

    [Verb("reset", HelpText = "Reset the faked clock.")]
    public class ResetFakedClockOptions
    {
        [Option('n', "now", Required = false, Default = null, HelpText = "Fake now in UTC zone.")]
        public required DateTime? FakedNowUtc { get; set; }
    }
}
