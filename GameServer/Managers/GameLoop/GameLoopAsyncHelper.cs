using System;
using System.Threading;
using System.Threading.Tasks;

namespace DOL.GS
{
    public static class GameLoopAsyncHelper
    {
        public static T GetResult<T>(Task<T> task)
        {
            if (SynchronizationContext.Current is GameServiceSynchronizationContext && !task.IsCompleted)
                ProcessServicePostedActionsUntilCompletion(task);

            return task.GetAwaiter().GetResult();
        }

        public static void Wait(Task task)
        {
            if (SynchronizationContext.Current is GameServiceSynchronizationContext && !task.IsCompleted)
                ProcessServicePostedActionsUntilCompletion(task);

            task.GetAwaiter().GetResult();
            return;
        }

        private static void ProcessServicePostedActionsUntilCompletion(Task task)
        {
            if (SynchronizationContext.Current is not GameServiceSynchronizationContext ctx)
                throw new InvalidOperationException("Not running on a Game Service context.");

            IGameService targetService = ctx.TargetService;
            SpinWait spinWait = new();

            while (!task.IsCompleted)
            {
                targetService.ProcessPostedActions();
                spinWait.SpinOnce(-1);
            }
        }
    }
}
