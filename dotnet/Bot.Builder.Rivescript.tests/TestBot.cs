using Bot.Builder.Rivescript;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Rivescript.Tests
{
    internal class TestBot : IBot
    {
        private const string RIVE_DIALOG = "RivescriptDialog";

        public TestBot(BotState botState, string fileName)
        {
            this.RivescriptDialog = new RivescriptDialog(RIVE_DIALOG, fileName, botState);

            this.Dialogs = new DialogSet(botState.CreateProperty<DialogState>("mainDialogSet"))
                .Add(this.RivescriptDialog);

        }

        public DialogSet Dialogs { get; set; }

        public RivescriptDialog RivescriptDialog { get; set; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialogContext = await this.Dialogs.CreateContextAsync(turnContext);

            var result = await dialogContext.ContinueAsync();

            if (result.Status == DialogTurnStatus.Empty)
                result = await dialogContext.BeginAsync(RIVE_DIALOG);

            if (result.Result == RivescriptDialog.NO_MATCH)
            {
                // alternate path
                await turnContext.SendActivityAsync("woo!");
            }
        }
    }
}
