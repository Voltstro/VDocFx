/**
 * Checks if an object is an array
 * @param obj The object to check
 * @returns Yea or nah
 */
function isArray(obj: any): boolean {
    return obj !== undefined && obj !== null && Array.isArray(obj);
}

/**
 * Checks if a string is a number
 * @param obj 
 * @returns 
 */
function isNumber(obj: string): boolean {
	return /^-?[\d.]+(?:e-?\d+)?$/.test(obj);
}

/**
 * Checks if something is an object
 * @param obj
 * @returns 
 */
function isObject(obj: any): boolean {
	return obj !== undefined && obj !== null && typeof obj === 'object';
}

/**
 * Checks if an object is a string
 * @param obj
 * @returns 
 */
function isString(obj: any): boolean {
	return (
		typeof obj == 'string' ||
		(typeof obj == 'object' && obj != null && obj != undefined && obj.constructor === String)
	);
}

function createHtmlId(input: any): string {
	return standardHtmlId(input);
}

/**
 * Creates a standard html ID from an object (that should be string)
 * @param input 
 * @returns 
 */
function standardHtmlId(input: any): string {
	if (!input) return '';
	if (isString(input)) {
		return normalizeHtmlId(input);
	}
	if (isObject(input) && input.uid) {
		return normalizeHtmlId(input.uid);
	}
	return '';
}

/**
 * Normalizes a string for a html id
 * @param input 
 * @returns 
 */
function normalizeHtmlId(input: string): string {
	input = input
		.toLowerCase()
		.replace(/\\/g, '')
		.replace(/'/g, '')
		.replace(/"/g, '')
		.replace(/%/g, '')
		.replace(/\^/g, '')
		.replace(/\[/g, '(')
		.replace(/\]/g, ')')
		.replace(/</g, '(')
		.replace(/>/g, ')')
		.replace(/{/g, '((')
		.replace(/}/g, '))');

	input = input.replace(/[^a-zA-Z0-9()@*]/g, '-');
	input = input.replace(/^-+/g, '');
	input = input.replace(/-+$/g, '');
	input = input.replace(/-+/g, '-');

	return input;
}

exports.createHtmlId = createHtmlId;
exports.isArray = isArray;
exports.isNumber = isNumber;
exports.isObject = isObject;
exports.isString = isString;