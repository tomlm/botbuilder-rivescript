using Bot.Builder.Rivescript;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Rivescript.Tests.RivescriptTestUtilities;

namespace Microsoft.Bot.Builder.Rivescript.Tests
{
    [TestClass]
    [TestCategory("RivescriptDialog")]
    [TestCategory("RiveScript")]
    [TestCategory("RiveScript - Basic")]
    public class RivescriptDialog_Tests
    {
        [TestCleanup]
        public void Cleanup()
        {
            CleanupTempFiles();
        }

        const string RIVE_DIALOG = "RivescriptDialog";

        private const string script = @"! version = 2.0
                    
                    + hello bot
                    - Hello, human!

                   + my name is *
                   - Nice to meet you, <star1>!

                   + cool
                   - yes it is

                   + hot
                   - steamy!";

        [TestMethod]
        public async Task RivescriptDialog_HelloBot()
        {

            // RiveScript Sample file, taken from their 
            // tutorial at: https://www.rivescript.com/docs/tutorial
            string fileName = CreateTempFile(script);

            var conversationState = new ConversationState(new MemoryStorage());
            var bot = new TestBot(conversationState, fileName);
            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            await new TestFlow(adapter, (turnContext, cancellationToken) => bot.OnTurnAsync(turnContext, cancellationToken))
                .Send("hello bot")
                    .AssertReply("Hello, human!")
                .Send("my name is giskard")
                    .AssertReply("Nice to meet you, giskard!")
                .Send("xyzpdq")
                    .AssertReply("woo!")
                .Send("cool")
                    .AssertReply("yes it is")
                .Send("hot")
                    .AssertReply("steamy!")
                .StartTestAsync();
        }
    }
}
