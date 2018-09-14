const assert = require('assert');
const rs = require('../');
const builder = require('botbuilder');
const process = require('process');
const path = require('path');
const dialogs = require('botbuilder-dialogs');
const rsd = require('../lib/rivescriptDialog');

const RIVE_DIALOG = "RivescriptDialog";

class TestBot {

    constructor(botState, path) {
        this.RivescriptDialog = new rsd.RivescriptDialog(RIVE_DIALOG, path, botState, null, {});
        this.Dialogs = new dialogs.DialogSet(botState.createProperty("mainDialogSet"))
            .add(this.RivescriptDialog);
    }

    async OnTurnAsync(turnContext) {
        var dialogContext = await this.Dialogs.createContext(turnContext);

        var result = await dialogContext.continue();
        if (result.Status == dialogs.DialogTurnStatus.Empty)
            result = await dialogContext.begin(RIVE_DIALOG);

        if (result.Result == rsd.RivescriptDialog.NO_MATCH) {
            // opportunity to handle rivescript not recognizing input
        }
    }
}


describe('RiveScriptReceiver', function () {
    it('Load and execute basic scripts', async function () {
        let convState = new builder.ConversationState(new builder.MemoryStorage());
        let bot = new TestBot(convState, path.join(__dirname, 'test.rive'));
        await new builder.TestAdapter(async (turnContext) => await bot.OnTurnAsync(turnContext))
            .use(new builder.AutoSaveStateMiddleware(convState))
            .send('hello bot')
                .assertReply('Hello human!')
            .startTest();
    });

    it('Load and execute complex scripts', async function () {
        let convState = new builder.ConversationState(new builder.MemoryStorage());
        let bot = new TestBot(convState, path.join(__dirname, 'complex.rive'));
        await new builder.TestAdapter(async (turnContext) => await bot.OnTurnAsync(turnContext))
            .use(new builder.AutoSaveStateMiddleware(convState))
            .send('my name is Tom')
                .assertReplyOneOf(['Nice to meet you, Tom.', 'Tom, nice to meet you.'], 'remember something')
            .send('what is my name?')
                .assertReplyOneOf([
                    'Your name is Tom.',
                    'You told me your name is Tom.',
                    'Aren\'t you Tom?'], 'memory test')
            .startTest();

    });

    it('routeToRiveScript simple', async function () {
        let convState = new builder.ConversationState(new builder.MemoryStorage());
        let bot = new TestBot(convState, path.join(__dirname, 'complex.rive'));
        await new builder.TestAdapter(async (turnContext) => await bot.OnTurnAsync(turnContext))
            .use(new builder.AutoSaveStateMiddleware(convState))
            .send('my name is Tom')
            .assertReplyOneOf([
                'Nice to meet you, Tom.',
                'Tom, nice to meet you.'], 'remember something')
            .send('what is my name?')
            .assertReplyOneOf([
                'Your name is Tom.',
                'You told me your name is Tom.',
                'Aren\'t you Tom?'], 'memory test')
            .startTest();
    });
})
