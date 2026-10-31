using System.Collections.Concurrent;

namespace DOL.GS
{
    public static class GameServiceContext
    {
        private static readonly ConcurrentDictionary<IGameService, GameServiceSynchronizationContext> _contexts = new();

        public static GameServiceSynchronizationContext GetContextFor(IGameService service)
        {
            return service == null ? null : _contexts.GetOrAdd(service, static s => new(s));
        }
    }
}
