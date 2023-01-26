var chromeCommon = require('./chrome.common.js')

exports.transform = function (model: any): any {
    model.layout = "Home";
    Object.assign(model, model.metadata);

    chromeCommon.makeGitHubRepoLink(model);

    return {
        content: JSON.stringify(model)
    }
}