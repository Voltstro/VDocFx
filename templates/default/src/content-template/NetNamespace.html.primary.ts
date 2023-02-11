var contentCommon = require('./content.common.js');
var shared = require('./shared.js');
var dotnet = require('./partials/dotnet/transform.js');

exports.transform = function (model: any): any {
	model._op_templateFilename = 'NetNamespace';
	dotnet.updateTitle(model, true, model.__global.namespace);
	createChildId(model);

	dotnet.updateEditButton(model);

	return model;
};

function createChildId(model: any): any {
	if (model.classes) {
		model.classes = model.classes.map(mapUidToObject);
		model.classesInfo = dotnet.getTypeChildrenInfo(model, model.classes);
	}

	if (model.structs) {
		model.structs = model.structs.map(mapUidToObject);
		model.structsInfo = dotnet.getTypeChildrenInfo(model, model.structs);
	}

	if (model.interfaces) {
		model.interfaces = model.interfaces.map(mapUidToObject);
		model.interfacesInfo = dotnet.getTypeChildrenInfo(model, model.interfaces);
	}

	if (model.enums) {
		model.enums = model.enums.map(mapUidToObject);
		model.enumsInfo = dotnet.getTypeChildrenInfo(model, model.enums);
	}

	if (model.delegates) {
		model.delegates = model.delegates.map(mapUidToObject);
		model.delegatesInfo = dotnet.getTypeChildrenInfo(model, model.delegates);
	}
}

function mapUidToObject(item: any): any {
	return {
		uid: item.uid,
		id: shared.createHtmlId(item.uid),
		monikers: item.monikers
	};
}
