namespace Perkify.Core.Tests;

public class StateChangeExecutorTests
{
    public class MockStateChange : IStateChanged<long, Type>
    {
        public long State { get; set; }

        public event EventHandler<StateChangeEventArgs<long, Type>>? StateChanged;

        public StateChangeExecutor<long, Type, MockStateChange> Executor => new(this, this.StateChanged)
        {
            StateRecorder = () => this.State,
        };
    }

    [Theory]
    [InlineData(100)]
    public void TestExecutor(long expected)
    {
        var mock = new MockStateChange { State = 0L };
        StateChangeEventArgs<long, Type>? stateChangedEvent = null;
        mock.StateChanged += (sender, e) =>
        {
            stateChangedEvent = e;
        };

        mock.Executor.Execute(typeof(StateChangeExecutorTests), () => mock.State = expected);
        mock.State.Should().Be(expected);
        stateChangedEvent.Should().NotBeNull();
        stateChangedEvent!.Operation.Should().Be(typeof(StateChangeExecutorTests));
        stateChangedEvent!.From.Should().Be(0L);
        stateChangedEvent!.To.Should().Be(expected);
    }

    [Theory]
    [InlineData(100)]
    public void TestExecutorNoHooking(long expected)
    {
        var mock = new MockStateChange { State = 0L };
        mock.Executor.Execute(typeof(StateChangeExecutorTests), () => mock.State = expected);
        mock.State.Should().Be(expected);
    }
}
