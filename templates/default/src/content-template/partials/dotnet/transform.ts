var isNode = typeof process !== 'undefined';
var contentCommon = require(isNode ? '../../content.common.js' : './content.common.js');
var shared = require(isNode ? '../../shared.js' : './shared.js');

function updateAssemblies(model: any): any {
	if (model.assembliesWithMoniker && model.assembliesWithMoniker.length > 0) {
		for (var i = 0; i < model.assembliesWithMoniker.length; i++) {
			if (model.assembliesWithMoniker[i].value.indexOf(',') === -1) {
				model.assembliesWithMoniker[i].assemblyTitle = model.__global.assembly;
			} else {
				model.assembliesWithMoniker[i].assemblyTitle = model.__global.assemblies;
			}
		}
	}
}

function updatePackages(model: any): any {
	if (model.packagesWithMoniker && model.packagesWithMoniker.length > 0) {
		for (var i = 0; i < model.packagesWithMoniker.length; i++) {
			if (model.packagesWithMoniker[i].value.indexOf(',') === -1) {
				model.packagesWithMoniker[i].packageTitle = model.__global.package;
			} else {
				model.packagesWithMoniker[i].packageTitle = model.__global.packages;
			}
		}
	}
}

function setRootName(model: any): any {
	//useful against handlebars bubbling
	model.rootName = htmlEncodeTypeName(model.name);
}

function setThreadSafety(model: any): any {
	if (!model.threadSafety) {
		model.threadSafety = null;
		return;
	}

	model.threadSafety = model.threadSafety.customizedContent
		? model.threadSafety.customizedContent
		: model.threadSafety.isSupported
		? model.__global['threadMemberScope_' + model.threadSafety.memberScope] ||
		  model.__global['threadMemberScope_all']
		: model.__global['threadMemberScope_unsupported'];
}

function htmlEncodeTypeName(str: string): string {
	return str.replace(/</gi, '&lt;').replace(/>/gi, '&gt;');
}

function createChildHtmlIds(model: any): any {
	if (model.fields) {
		model.fields.htmlId = contentCommon.createHtmlId(model.__global.fieldsInSubtitle);
	}

	if (model.properties) {
		model.properties.htmlId = contentCommon.createHtmlId(model.__global.propertiesInSubtitle);
	}

	if (model.methods) {
		model.methods.htmlId = contentCommon.createHtmlId(model.__global.methodsInSubtitle);
	}

	if (model.examples) {
		model.examples.htmlId = contentCommon.createHtmlId(model.__global.examples);
	}
	if (model.description) {
		model.description.htmlId = contentCommon.createHtmlId(model.__global.description);
	}

	if (model.additionalNotes) {
		if (model.additionalNotes.implementer) {
			model.additionalNotes.implementer.htmlId = contentCommon.createHtmlId(
				model.__global.notesToImplementers
			);
		}
		if (model.additionalNotes.inheritor) {
			model.additionalNotes.inheritor.htmlId = contentCommon.createHtmlId(
				model.__global.notesToInheritors
			);
		}
		if (model.additionalNotes.caller) {
			model.additionalNotes.caller.htmlId = contentCommon.createHtmlId(
				model.__global.notesToCallers
			);
		}
	}

	if (model.seeAlso) {
		model.seeAlso.htmlId = contentCommon.createHtmlId(model.__global.seeAlso);
	}
	if (model.threadSafety) {
		model.threadSafety.htmlId = contentCommon.createHtmlId(model.__global.threadSafety);
	}
}

function updateDerivedClasses(model: any): void {
	var maxDerivedDisplayCount = 5;
	if (model.derivedClassesWithMoniker) {
		if (model.derivedClassesWithMoniker.length > maxDerivedDisplayCount) {
			var hiddenDerivedClassesWithMoniker = [];
			while (model.derivedClassesWithMoniker.length > maxDerivedDisplayCount) {
				hiddenDerivedClassesWithMoniker.unshift(model.derivedClassesWithMoniker.pop());
			}

			model.hiddenDerivedClassesWithMoniker = hiddenDerivedClassesWithMoniker;
		}
	}
}

function updateReturns(model: any): void {
	if (model.returnsWithMoniker) {
		for (var i = 0; i < model.returnsWithMoniker.type.length; i++) {
			model.returnsWithMoniker.type[i].value = convertStringToInlineMD(
				model.returnsWithMoniker.type[i].value
			);

			if (typeof model.returnsWithMoniker.type[i].monikers === 'undefined') {
				model.returnsWithMoniker.type[i].monikers = null;
			}
		}
	}
}

function updateTypeParameters(model: any): void {
	updateParameterList(model.typeParameters);
}

function updateParameters(model: any): void {
	updateParameterList(model.parameters);
}

function updateExceptions(model: any): void {
	if (model.exceptions) {
		model.hasExceptions = true;
		updateParameterList(model.exceptions);
	}
}

function updatePermissions(model: any): void {
	if (model.permissions) {
		model.hasPermissions = true;
		for (var k = 0; k < model.permissions.length; k++) {
			model.permissions[k].type = convertStringToInlineMD(model.permissions[k].type);
			model.permissions[k].description = convertStringToInlineMD(model.permissions[k].description);
		}
	}
}

function convertStringToInlineMD(ref: string): string {
	// this function is used to remove <p> tag
	// V3 support inline markdown and if we mark the content is inline markdown in schema,
	// the render result will not contain <p> tag.
	// V2 not support this yet, so this is just a workaround and will update code when v2 support inline markdown also
	if (ref) {
		return ref.replace(/<p.*?>(.*?)<\/p>/gi, '$1').trim();
	}

	return null;
}

// This applies to implements/inheritances/derived classes and so on, which all share the same schema and html structure.
// They are array of markdown text pieces which needs to remove <p> tag. See convertStringToInlineMD() for details.
function makeTypeReferencesInlineMD(refArray: any[]): void {
	if (refArray && refArray.length > 0) {
		for (var k = 0; k < refArray.length; k++) {
			if (shared.isString(refArray[k])) {
				refArray[k] = convertStringToInlineMD(refArray[k]);
			} else if (shared.isObject(refArray[k]) && refArray[k].value) {
				refArray[k].value = convertStringToInlineMD(refArray[k].value);
			}
		}
	}
}

// This applies to parameters, type parameters and exceptions, which all share the same schema and html structure.
// `type` is a piece of markdown text which needs to remove <p> tag. See convertStringToInlineMD() for details.
// `className` is used to add extra space between multiple parameters/type parameters/exceptions.
function updateParameterList(paramList: any[]): void {
	if (paramList && paramList.length > 0) {
		paramList[0].type = convertStringToInlineMD(paramList[0].type);
		for (var k = 1; k < paramList.length; k++) {
			paramList[k].type = convertStringToInlineMD(paramList[k].type);
			paramList[k].className = ' stack';
		}
	}
}

function appendInheritedMembers(model: any): any {
	if (!model.inheritedMembers) {
		return;
	}

	for (var i = 0; i < model.inheritedMembers.length; i++) {
		if (!model.inheritedMembers[i].memberType || !model.inheritedMembers[i].uid) {
			continue;
		}

		switch (model.inheritedMembers[i].memberType) {
			case 'method':
				appendInheritedMembersToMember(model.methods, model.inheritedMembers[i].uid);
				break;
			case 'eii':
				appendInheritedMembersToMember(model.eiis, model.inheritedMembers[i].uid);
				break;
			case 'property':
				appendInheritedMembersToMember(model.properties, model.inheritedMembers[i].uid);
				break;
			case 'event':
				appendInheritedMembersToMember(model.events, model.inheritedMembers[i].uid);
				break;
			case 'field':
				appendInheritedMembersToMember(model.fields, model.inheritedMembers[i].uid);
				break;
			default:
				appendInheritedMembersToMember(model.methods, model.inheritedMembers[i].uid);
				break;
		}
	}
}

function appendInheritedMembersToMember(obj: any, inheritedMemberUid: any): void {
	if (!obj) {
		obj = [];
	}

	obj = obj.push(inheritedMemberUid);
}

function updateTitle(model: any, breakText: string, typeStr: string): void {
	if (model.name) {
		model.title = htmlEncodeTypeName(model.name);
		if (breakText) {
			model.title = contentCommon.breakText(model.title);
		}
		model.title = model.title + ' ' + typeStr;
	}
}

function updateTitleForMeta(model: any, typeStr: string): void {
	if (model.name) {
		model.title = model.name + ' ' + typeStr;
	}
	addNamespaceToTitle(model);
}

function addNamespaceToTitle(model) {
	if (model.namespace) {
		if (shared.isString(model.namespace)) {
			model.title = model.title + ' (' + model.namespace + ')';
		} else if (shared.isObject(model.namespace)) {
			model.title = model.title + ' (' + model.namespace.uid + ')';
		}
	}
}

function updateEditButton(obj: any): void {
	if (obj.source) {
		var sourceObj = {
			remote: obj.source
		};
		obj.editLink = contentCommon.buildEditLink(sourceObj);
	}
}

function updateMemberTitle(model: any, breakText: string): void {
	if (model.type === 'constructor') {
		var memberTypeStr = model.isOverload
			? model.__global['constructorsInSubtitle']
			: model.__global['constructorInSubtitle'];
		model.title = extractMemberTitle(model, 'name', breakText) + ' ' + memberTypeStr;
	} else {
		var camelCaseType = model.type;
		if (camelCaseType === 'attachedproperty') {
			camelCaseType = 'attachedProperty';
		} else if (camelCaseType === 'attachedevent') {
			camelCaseType = 'attachedEvent';
		}
		model.title =
			extractMemberTitle(model, 'nameWithType', breakText) + ' ' + model.__global[camelCaseType];
	}
}

function updateMonikerizedArray(arr: any[]): void {
	if (arr && arr.length > 0) {
		for (var k = 0; k < arr.length; k++) {
			if (!arr[k].monikers) {
				arr[k].monikers = null;
			}
		}
	}
}

function updateInheritanceWithMoniker(inheritanceWithMoniker: any): void {
	makeTypeReferencesInlineMD(inheritanceWithMoniker.values);
	if (inheritanceWithMoniker.valuesPerLanguage) {
		makeTypeReferencesInlineMD(inheritanceWithMoniker.valuesPerLanguage);
		inheritanceWithMoniker.hasPerLanguageInheritancesWithMonikers = true;
	}
}

function updateMonikerizedProperties(model: any): void {
	if (model.attributesWithMoniker && model.attributesWithMoniker.length > 0) {
		model.hasAttributesWithMoniker = true;
		updateMonikerizedArray(model.attributesWithMoniker);
	}

	if (model.syntaxWithMoniker && model.syntaxWithMoniker.length > 0) {
		for (var j = 0; j < model.syntaxWithMoniker.length; j++) {
			updateMonikerizedArray(model.syntaxWithMoniker[j]);
		}
	}

	if (model.namesWithMoniker && model.namesWithMoniker.length > 0) {
		for (var j = 0; j < model.namesWithMoniker.length; j++) {
			updateMonikerizedArray(model.namesWithMoniker[j]);
		}
	}

	if (model.inheritancesWithMoniker && model.inheritancesWithMoniker.length > 0) {
		model.hasInheritancesWithMonikers = true;
		updateMonikerizedArray(model.inheritancesWithMoniker);
		for (var k = 0; k < model.inheritancesWithMoniker.length; k++) {
			updateInheritanceWithMoniker(model.inheritancesWithMoniker[k]);
		}
	}

	if (model.implementsWithMoniker && model.implementsWithMoniker.length > 0) {
		model.hasImplementsWithMoniker = true;
		updateMonikerizedArray(model.implementsWithMoniker);
		updateImplementsWithMoniker(model.implementsWithMoniker);
	}

	if (model.derivedClassesWithMoniker && model.derivedClassesWithMoniker.length > 0) {
		model.hasDerivedClassesWithMoniker = true;
		updateMonikerizedArray(model.derivedClassesWithMoniker);
		if (model.hiddenDerivedClassesWithMoniker && model.hiddenDerivedClassesWithMoniker.length > 0) {
			updateMonikerizedArray(model.hiddenDerivedClassesWithMoniker);
		}
	}
}

function updateImplementsWithMoniker(implementsWithMoniker: any[]) {
	makeTypeReferencesInlineMD(implementsWithMoniker);
	for (var i = 0; i < implementsWithMoniker.length; i++) {
		makeTypeReferencesInlineMD(implementsWithMoniker[i]);
		if (
			implementsWithMoniker[i].valuePerLanguage &&
			implementsWithMoniker[i].valuePerLanguage.length > 0
		) {
			makeTypeReferencesInlineMD(implementsWithMoniker[i].valuePerLanguage);
		}
	}
}

function extractMemberTitle(model: any, titleProperty: any, breakText: string): string {
	var name = model[titleProperty];
	var parenthesesPart = '';
	if (model.members && model.members.length === 1) {
		name = model.members[0][titleProperty];
	}
	if (breakText) {
		name = htmlEncodeTypeName(name);
	}
	var parenthesesIdx = name.indexOf('(');
	if (parenthesesIdx > 0) {
		parenthesesPart = name.substr(parenthesesIdx);
		if (parenthesesPart === '()') {
			parenthesesPart = '';
		}
		name = name.substr(0, parenthesesIdx);
	}

	if (breakText) {
		name = contentCommon.breakText(name);
	}
	return name + parenthesesPart;
}

function getTypeChildrenInfo(model: any, children: any[]): any {
	if (children) {
		var info: {
			monikers: any[],
			isInAllMonikers?: boolean;
		} = {
			monikers: [],
		};
		for (var i = 0; i < children.length; i++) {
			const child = children[i];
			if (child.inheritedFrom) {
				child.isInheritedFrom = true;
			}
			if (shared.isArray(child.monikers)) {
				if (model.monikers) {
					child.isInAllMonikers = arraysAreEqual(child.monikers, model.monikers);
				}
				for (var j = 0; j < child.monikers.length; j++) {
					if (info.monikers.indexOf(child.monikers[j]) === -1) {
						info.monikers.push(child.monikers[j]);
					}
				}
			}
		}
		if (info.monikers && model.monikers) {
			info.isInAllMonikers = arraysAreEqual(info.monikers, model.monikers);
		}
		return info;
	}
}

function arraysAreEqual(arr1: any[], arr2: any[]): boolean {
	if (arr1.length != arr2.length) return false;

	for (var i = 0; i < arr1.length; i++) {
		if (arr1[i] != arr2[i]) {
			return false;
		}
	}
	return true;
}

function updateAdditionalRequirements(model: any): void {
	if (model.sdkRequirements || model.osRequirements) {
		model.displayAdditionalRequirements = true;
	}
}

function updateUWPProperties(model: any, introducedInText: string): void {
	if (model.capabilities) {
		model.displayUWPRequirements = true;
	}

	if (!introducedInText && model.__global) {
		introducedInText = model.__global.win10_introducedIn;
	}

	if (model.uwpRequirements && introducedInText) {
		model.displayUWPRequirements = true;

		for (var i = 0; i < model.uwpRequirements.deviceFamilies.length; i++) {
			var family = model.uwpRequirements.deviceFamilies[i];
			family.introducedInVersion = introducedInText.replace('{version}', family.version);
		}
		for (var i = 0; i < model.uwpRequirements.apiContracts.length; i++) {
			var contract = model.uwpRequirements.apiContracts[i];
			contract.introducedInVersion = introducedInText.replace('{version}', 'v' + contract.version);
		}
	}
}

exports.updateAssemblies = updateAssemblies;
exports.updatePackages = updatePackages;
exports.setRootName = setRootName;
exports.setThreadSafety = setThreadSafety;
exports.createChildHtmlIds = createChildHtmlIds;
exports.updateDerivedClasses = updateDerivedClasses;
exports.appendInheritedMembers = appendInheritedMembers;
exports.htmlEncodeTypeName = htmlEncodeTypeName;
exports.updateTypeParameters = updateTypeParameters;
exports.updateParameters = updateParameters;
exports.updateExceptions = updateExceptions;
exports.updatePermissions = updatePermissions;
exports.updateReturns = updateReturns;
exports.addNamespaceToTitle = addNamespaceToTitle;
exports.updateMemberTitle = updateMemberTitle;
exports.updateTitle = updateTitle;
exports.updateTitleForMeta = updateTitleForMeta;
exports.updateMonikerizedProperties = updateMonikerizedProperties;
exports.getTypeChildrenInfo = getTypeChildrenInfo;
exports.updateEditButton = updateEditButton;
exports.updateAdditionalRequirements = updateAdditionalRequirements;
exports.updateUWPProperties = updateUWPProperties;
