var shared = require('./shared.js');
var dotnet = require('./partials/dotnet/transform.js');

exports.transform = function (model: any): any {
	model._op_templateFilename = 'NetEnum';
	dotnet.updateTitle(model, true, model.__global.enumInSubtitle);

	dotnet.updateAssemblies(model);
	dotnet.updatePackages(model);
	dotnet.setRootName(model);
	dotnet.createChildHtmlIds(model);
	dotnet.updateMonikerizedProperties(model);
	dotnet.updateUWPProperties(model);
	dotnet.updateAdditionalRequirements(model);

	dotnet.updateEditButton(model);

	updateChildrenNE(model);

	if (model.isFlags && !model.summary) {
		model.summary = ' ';
	}
	return model;
};

function updateChildrenNE(model: any): any {
	if (model.fields) {
		model.fieldsInfo = dotnet.getTypeChildrenInfo(model, model.fields);
		for (var i = 0; i < model.fields.length; i++) {
			if (!model.fields[i].literalValue) {
				model.fields[i].literalValue = null;
			}
			if (!model.fields[i].summary) {
				model.fields[i].summary = null;
			}
			model.fields[i].htmlId = shared.createHtmlId(model.fields[i].uid);
		}
	}
}
