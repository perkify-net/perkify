namespace Perkify.Demo
{
    using CommandLine;
    using Spectre.Console;

    public enum Mode
    {
        Subscription,
        PayAsYouGo,
        Coupon,
        Clock
    }

    public static class ParserExtensions
    {
        public static ParserResult<object> ParseArgumentsInMode(this Parser parser, Mode mode, IEnumerable<string> args)
            => mode switch
            {
                Mode.Subscription => parser.ParseArguments
                <
                    CreateSubscriptionOptions,
                    RenewSubscriptionOptions,
                    DeactivateSubscriptionOptions,
                    ActivateSubscriptionOptions,
                    CancelSubscriptionOptions,
                    ViewSubscriptionOptions,

                    SwitchModeOptions,
                    ExitOptions
                >(args),

                Mode.PayAsYouGo => throw new NotImplementedException("PayAsYouGo mode is not ready"),

                Mode.Coupon => throw new NotImplementedException("Coupon mode is not ready"),

                Mode.Clock => parser.ParseArguments
                <
                    ViewFakedClockOptions,
                    AdvanceFakedClockOptions,
                    ResetFakedClockOptions,

                    SwitchModeOptions,
                    ExitOptions
                >(args),

                _ => throw new NotImplementedException(),
            };

        public static ParserResult<object> WithParsedInMode(this ParserResult<object> result, Mode mode, DemoApp app)
            => mode switch
            {
                Mode.Subscription => result
                    .WithParsed<CreateSubscriptionOptions>(opts => app.CreateSubscription(opts))
                    .WithParsed<RenewSubscriptionOptions>(opts => app.RenewSubscription(opts))
                    .WithParsed<ViewSubscriptionOptions>(opts => app.ViewSubscription(opts))
                    .WithParsed<DeactivateSubscriptionOptions>(opts => app.DeactivateSubscription(opts))
                    .WithParsed<ActivateSubscriptionOptions>(opts => app.ActivateSubscription(opts))
                    .WithParsed<CancelSubscriptionOptions>(opts => app.CancelSubscription(opts))

                    .WithParsed<SwitchModeOptions>(opts => app.Switch(opts))
                    .WithParsed<ExitOptions>(opts => app.Exit()),

                Mode.PayAsYouGo => throw new NotImplementedException("PayAsYouGo mode is not ready"),

                Mode.Coupon => throw new NotImplementedException("Coupon mode is not ready"),

                Mode.Clock => result
                    .WithParsed<ViewFakedClockOptions>(opts => app.ViewFakedClock(opts))
                    .WithParsed<AdvanceFakedClockOptions>(opts => app.AdvanceFakedClock(opts))
                    .WithParsed<ResetFakedClockOptions>(opts => app.ResetFakedClock(opts))

                    .WithParsed<SwitchModeOptions>(opts => app.Switch(opts))
                    .WithParsed<ExitOptions>(opts => app.Exit()),

                _ => throw new NotImplementedException(),
            };
    }
}
