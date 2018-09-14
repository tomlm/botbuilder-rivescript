using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using RiveScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Rivescript
{
    public class RivescriptOptions
    {
        public bool Utf8 { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Strict { get; set; } = false;
    }

    public class RivescriptState : Dictionary<string, string>
    {
    }

    public class RivescriptDialog : Dialog, RiveScript.IObjectHandler
    {
        private readonly RiveScript.RiveScript rsEngine;
        public const string RivescriptState = "rivescript";

        public RivescriptDialog(string dialogId, string path, BotState botState)
            : this(dialogId, path, botState.CreateProperty<RivescriptState>(dialogId), new RivescriptOptions())
        {
        }

        public RivescriptDialog(string dialogId, string path, IStatePropertyAccessor<RivescriptState> stateProperty)
            : this(dialogId, path, stateProperty, new RivescriptOptions())
        {
        }

        public RivescriptDialog(string dialogId, string path, IStatePropertyAccessor<RivescriptState> stateProperty, RivescriptOptions options)
            : base(dialogId)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.rsEngine = this.CreateRivescript(path.Trim(), options);
            this.StateProperty = stateProperty;
        }

        public IStatePropertyAccessor<RivescriptState> StateProperty { get; set; }

        public const string NO_MATCH = "ERR: No Reply Matched";

        public override async Task<DialogTurnResult> DialogBeginAsync(DialogContext dialogContext, Object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get the conversation state from the turn context
            if (dialogContext.Context.Activity?.Type == ActivityTypes.Message)
            {
                IMessageActivity activity = dialogContext.Context.Activity.AsMessageActivity();
                var state = await this.StateProperty.GetAsync(dialogContext.Context, () => new RivescriptState());

                this.rsEngine.setUservars(activity.From.Id, state);

                var reply = this.rsEngine.reply(activity.From.Id, activity.Text);

                // send reply if matched
                if (reply != NO_MATCH)
                {
                    await dialogContext.Context.SendActivityAsync(reply);
                }
                return await dialogContext.EndAsync(reply);
            }
            return await dialogContext.EndAsync();
        }


        private RiveScript.RiveScript CreateRivescript(string path, RivescriptOptions options)
        {
            RiveScript.RiveScript engine = new RiveScript.RiveScript(options.Debug, options.Utf8, options.Strict);

            // set ourselves as a "script language" so that we can do the glue via reflection
            engine.setHandler("dialog", this);

            // only define methods from child class
            var ignoreMethods = new List<string>(typeof(RivescriptDialog).GetMethods().Select(m => m.Name));

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
            {
                engine.loadDirectory(path);
            }
            else
            {
                engine.loadFile(path);
            }

            // sort 
            engine.sortReplies();

            return engine;
        }

        public bool onLoad(string name, string[] code)
        {
            return true;
        }

        public String onCall(String name, RiveScript.RiveScript rs, String[] args)
        {
            var methodInfo = this.GetType().GetMethod(name);
            if (methodInfo != null)
            {
                // call it
                return methodInfo.Invoke(this, new object[] { this.rsEngine, args })?.ToString();
            }

            return null;
        }
    }
}