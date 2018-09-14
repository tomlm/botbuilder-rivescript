import { ActivityTypes, BotState, StatePropertyAccessor } from 'botbuilder-core';
import { Dialog, DialogContext, DialogTurnResult } from 'botbuilder-dialogs';
import { CreateRivescript } from './utils';
import RiveScript = require('rivescript');

export interface RiveScriptOptions {
    utf8?: boolean;
    debug?: boolean;
    onDebug?: (message: string) => void;
    errors?: { [key: string]: string };
}


/**
 * Bot Builder Rivescript Receiver 
 */
export class RivescriptDialog extends Dialog<RiveScriptOptions>
{
    private rsEnginePromise: Promise<RiveScript>;

    /**
     * creates a rivescript dialog for the rivescript files/folders
     * @param pathOrPaths path or paths to files/folders for .rive files
     * @param options standard rivescript options 
     */
    constructor(dialogId: string, pathOrPaths: string[] | string, botState?: BotState, public stateProperty?: StatePropertyAccessor<object>, options?: RiveScriptOptions) {
        super(dialogId);

        this.rsEnginePromise = CreateRivescript(pathOrPaths, options);
        
        if (!this.stateProperty)
            this.stateProperty = botState.createProperty(`state-${dialogId}`);
    }

    public static readonly NO_MATCH: string = "ERR: No Reply Matched";

    public async dialogBegin(dialogContext: DialogContext, options?: RiveScriptOptions): Promise<DialogTurnResult> {

        // Get the conversation state from the turn context
        if (dialogContext.context.activity.type == ActivityTypes.Message) {
            const activity = dialogContext.context.activity;
            const engine = await this.rsEnginePromise;

            var state = await this.stateProperty.get(dialogContext.context, () => { });

            engine.setUservars(activity.from.id, state);

            var reply = engine.reply(activity.from.id, activity.text);

            // send reply if matched
            if (reply != RivescriptDialog.NO_MATCH) {
                await dialogContext.context.sendActivity(reply);
            }
            return await dialogContext.end(reply);
        }
        return await dialogContext.end();
    }
}

