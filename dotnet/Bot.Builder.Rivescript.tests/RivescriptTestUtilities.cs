using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Bot.Builder.Rivescript;
using System.Threading;

namespace Microsoft.Bot.Builder.Rivescript.Tests
{
    /// <summary>
    /// Allows an Action<BotContext> to be run when a Context is created. This Action
    /// has full access to the Context, and can inject state that is then used by the pipeline. 
    /// </summary>
    public class InjectState : IMiddleware
    {
        private readonly Action<ITurnContext> _action;

        public InjectState(Action<ITurnContext> action)
        {
            _action = action;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            _action(turnContext);
            await next(cancellationToken);
        }
    }

    /// <summary>
    /// Allows an Action<BotContext> to be run upon the completion of a Middleware Pipeline. This Action
    /// has full access to the Context, and can validate state is as expected.
    /// </summary>
    public class ValidateState : IMiddleware
    {
        private readonly Action<ITurnContext> _action;

        public ValidateState(Action<ITurnContext> action)
        {
            _action = action;
        }


        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            await next(cancellationToken);
            _action(turnContext);
        }
    }

    public static class RivescriptTestUtilities
    {
        private static List<string> tempFileNames = new List<string>();

        public static string CreateTempFile(string riveScriptText)
        {
            var tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, riveScriptText);
            tempFileNames.Add(tempFile);

            return tempFile;
        }

        public static void CleanupTempFiles()
        {
            foreach (string fileName in tempFileNames)
            {
                System.IO.File.Delete(fileName);
            }

            tempFileNames.Clear();
        }
    }
}
