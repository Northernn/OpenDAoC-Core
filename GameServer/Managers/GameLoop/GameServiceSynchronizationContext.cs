using System.Threading;

namespace DOL.GS
{
    public class GameServiceSynchronizationContext : SynchronizationContext
    {
        public IGameService TargetService { get; }

        public GameServiceSynchronizationContext(IGameService targetService)
        {
            TargetService = targetService;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            TargetService.Post(static state => state.d(state.state), (d, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            // Deadlock prevention for game loop cross-talk.
            if (Current is GameServiceSynchronizationContext)
            {
                d(state);
                return;
            }

            using ManualResetEvent completed = new(false);

            TargetService.Post(static state =>
            {
                try
                {
                    state.d(state.state);
                }
                finally
                {
                    state.completed.Set();
                }
            }, (d, state, completed));

            completed.WaitOne();
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}
