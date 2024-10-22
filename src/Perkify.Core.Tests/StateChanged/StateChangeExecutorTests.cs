namespace Perkify.Core.Tests
{
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
            mock.StateChanged += (sender, e) =>
            {
                e.Operation.Should().Be(typeof(StateChangeExecutorTests));
                e.From.Should().Be(0L);
                e.To.Should().Be(expected);
            };

            mock.Executor.Execute(typeof(StateChangeExecutorTests), () => mock.State = expected);
            mock.State.Should().Be(expected);
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
}
