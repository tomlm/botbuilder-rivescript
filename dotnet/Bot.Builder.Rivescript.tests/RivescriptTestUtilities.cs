using Microsoft.Bot.Builder.Adapters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Bot.Builder.Rivescript;
using Microsoft.Bot.Builder.Core.Extensions;

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

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {

            _action(context);
            await next();
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

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            await next();
            _action(context);
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
        public static TestAdapter CreateSimpleRivescriptBot(string fileName)
        {
            var adapter = new TestAdapter()
                .Use(new ConversationState<RivescriptState>(new MemoryStorage()))
                .Use(new RivescriptMiddleware(fileName));

            return adapter;
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
