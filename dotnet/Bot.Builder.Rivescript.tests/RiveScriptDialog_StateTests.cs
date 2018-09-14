using Bot.Builder.Rivescript;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Rivescript.Tests.RivescriptTestUtilities;

namespace Microsoft.Bot.Builder.Rivescript.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    [TestCategory("RiveScript")]
    [TestCategory("RiveScript - State")]
    public class RiveScriptDialog_StateTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            CleanupTempFiles();
        }


        [TestMethod]
        public async Task RivescriptDialog_StateFromBotToRivescript()
        {
            string name = Guid.NewGuid().ToString();

            string fileName = CreateTempFile(
                         @"! version = 2.0

                           +hello bot
                           -Test <get name>");


            var conversationState = new ConversationState(new MemoryStorage());
            var bot = new TestBot(conversationState, fileName);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState))
                .Use(new InjectState(async (context) =>
                    {
                        var dict = await bot.RivescriptDialog.StateProperty.GetAsync(context, () => new RivescriptState());
                        dict["name"] = name;
                    })
                );

            await new TestFlow(adapter, (turnContext, cancellationToken) => bot.OnTurnAsync(turnContext, cancellationToken))
                .Send("Hello bot")
                .AssertReply("Test " + name)
                .StartTestAsync();
        }

        [TestMethod]
        public async Task RivescriptDialog_StateFromRivescriptToBot()
        {
            //Note: The dictionary coming back from the C# Rivescript implementation
            // eats the "-" in a GUID. This means if we send in "abcde-12345" we get
            // back "abcde12345". This behavior is confirmed to be scoped to the 
            // Rivescript implementation, and not caused by the BotBuilder Middleware.                         
            // To work around this, this test - which is just testing the BotBuilder 
            // code - doesn't use any "-". 
            string uglyGuid = Guid.NewGuid().ToString("N");
            bool validationRan = false;

            string fileName = CreateTempFile(
                         @"! version = 2.0

                           + value is *
                           - <set test=<star>>value is <get test>");

            var conversationState = new ConversationState(new MemoryStorage());
            var bot = new TestBot(conversationState, fileName);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState))
                .Use(new ValidateState(async (context) =>
                    {
                        var dict = await bot.RivescriptDialog.StateProperty.GetAsync(context, () => new RivescriptState());
                        Assert.IsTrue(
                            dict["test"] == uglyGuid,
                            $"Incorrect value. Expected '{uglyGuid}', found '{dict["test"]}'");
                        validationRan = true;
                    })
                );

            await new TestFlow(adapter, (turnContext, cancellationToken) => bot.OnTurnAsync(turnContext, cancellationToken))
                .Send("value is " + uglyGuid)
                    .AssertReply("value is " + uglyGuid)
                .StartTestAsync();

            // Make sure the state validator actually ran. 
            Assert.IsTrue(validationRan, "The State Validator did not run");
        }
    }
}
