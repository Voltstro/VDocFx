var chromeCommon = require('./chrome.common.js')

exports.transform = function (model: any): any {
    model.toc_rel = model._tocRel;
    model.layout = model.layout || "Conceptual";
    delete model.conceptual;

    var contrib = model._op_gitContributorInformation;
    if (contrib && contrib.author) {
        contrib.contributors = contrib.contributors || [];
        contrib.contributors.unshift(contrib.author);
    }

    chromeCommon.makeGitHubRepoLink(model);

    return {
        content: JSON.stringify(model)
    }
}