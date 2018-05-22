using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using RiveScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Rivescript
{
    public class RiveScriptOptions
    {
        public bool Utf8 { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Strict { get; set; } = false;
    }

    public class RivescriptState : Dictionary<string,string>
    {
    }

    public class RivescriptMiddleware : IMiddleware, IObjectHandler
    {
        private readonly RiveScript.RiveScript _engine;
        public const string RivescriptState = "rivescript";

        public RivescriptMiddleware(string path) : this(path, new RiveScriptOptions())
        {
        }
        public RivescriptMiddleware(string path, RiveScriptOptions options)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            _engine = CreateRivescript(path.Trim(), options);
        }

        public string onCall(string name, RiveScript.RiveScript rs, string[] args)
        {
            var methodInfo = this.GetType().GetMethod(name);
            if (methodInfo != null)
                // call it
                return methodInfo.Invoke(this, new object[] { _engine, args })?.ToString();

            return null;
        }

        public bool onLoad(string name, string[] code)
        {
            return true;
        }

        public static IDictionary<string, string> StateDictionary(ITurnContext context)
        {
            var rivescriptState = context.GetConversationState<RivescriptState>();
            return rivescriptState;
        }


        private RiveScript.RiveScript CreateRivescript(string path, RiveScriptOptions options)
        {
            RiveScript.RiveScript engine = new RiveScript.RiveScript(options.Debug, options.Utf8, options.Strict);

            // set ourselves as a "script language" so that we can do the glue via reflection
            engine.setHandler("dialog", this);

            // only define methods from child class
            var ignoreMethods = new List<string>(typeof(RivescriptMiddleware).GetMethods().Select(m => m.Name));

            // build method bindings using the "dialog" language
            StringBuilder sb = new StringBuilder();
            foreach (var method in this.GetType().GetMethods())
            {
                if (!ignoreMethods.Contains(method.Name))
                {
                    // this is  "dialog" language method definition.  Turns out that's all we need
                    sb.AppendLine($"> object {method.Name} dialog");
                    sb.AppendLine($"< object\n");
                }
            }
            // load method bindings
            engine.stream(sb.ToString());

            // load referred path
            if (Directory.Exists(path))
                engine.loadDirectory(path);
            else
                engine.loadFile(path);

            // sort 
            engine.sortReplies();

            return engine;
        }

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            // Get the conversation state from the turn context
            if (context.Activity != null && context.Activity.Type == ActivityTypes.Message)
            {
                IDictionary<string,string> conversationState = context.GetConversationState<RivescriptState>();
                _engine.setUservars(context.Activity.From.Id, conversationState);

                var reply = _engine.reply(context.Activity.From.Id, context.Activity.AsMessageActivity().Text);

                await context.SendActivity(context.Activity.CreateReply(reply));
            }

            await next().ConfigureAwait(false);
        }
    }
}