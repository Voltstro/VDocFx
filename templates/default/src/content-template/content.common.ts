var shared = require('./shared.js');

var breakTextRegexDots: RegExp = /([A-Z]\.|[a-z]\.)([A-Z]|[a-z])/g;
var breakTextRegexCase: RegExp = /([a-z])([A-Z]+[a-z])/g;
var breakTextReplace: string = '$1<wbr>$2';

function breakText(str: string, dotsOnly: boolean): string {
	if (!str || str.length === 0) {
		return str;
	}
	str = str.replace(breakTextRegexDots, breakTextReplace);
	if (dotsOnly) {
		return str;
	}
	return str.replace(breakTextRegexCase, breakTextReplace);
}

exports.breakText = breakText;
exports.createHtmlId = shared.createHtmlId;