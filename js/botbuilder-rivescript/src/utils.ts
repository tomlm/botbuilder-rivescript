import RiveScript = require('rivescript');

export interface RiveScriptOptions {
    utf8?: boolean;
    debug?: boolean;
    onDebug?: (message: string) => void;
    errors?: { [key: string]: string };
}

/**
 * Helper class to instantiate a RiveScript engine as a promise making it easy to be consumed by bot builder
 * @param pathOrPaths path to  rivescript file, or array of paths to files
 * @param options standard rivescript option object
 */
export function CreateRivescript(pathOrPaths: string[] | string, options?: RiveScriptOptions): Promise<RiveScript> {
    return new Promise<RiveScript>((resolve, reject) => {
        let rsEngine: RiveScript = new RiveScript(options || {});
        let paths: string[] = [];
        if (Array.isArray(pathOrPaths))
            paths = pathOrPaths;
        else
            paths.push(<string>pathOrPaths);

        let count = paths.length;
        for (let iPath in paths) {
            let path = paths[iPath];

            rsEngine.loadFile(path, (batchCount) => {
                if (--count == 0) {
                    rsEngine.sortReplies();
                    resolve(rsEngine);
                }
            }, (err, batchCount) => {
                console.log("Error loading batch #" + batchCount + ": " + err + "\n");
                if (--count == 0) {
                    rsEngine.sortReplies();
                    resolve(rsEngine);
                }
            });
        }
    });
}

