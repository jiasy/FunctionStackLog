var LogUtils = {}
LogUtils.currentStack = 0;
LogUtils.stackIndentList = [];
LogUtils.lastSackIndentCount = -1;
LogUtils.filterList = [
	"C -> c"
];
LogUtils.lockLogStackLength = -1;
LogUtils.lockLogAfter = true;//发生过滤后，后续Log是否继续输出
LogUtils.logging = true;//当前是否处于log状态
LogUtils.logOutputCount = 1;//凑够多少条再输出
LogUtils.stackLogCacheList = [];//Log缓存
LogUtils.logCount = 0;
LogUtils.targetLogCount = 0;

LogUtils.doLog = function (logStr_) {
	LogUtils.stackLogCacheList.push(logStr_);
	if (LogUtils.stackLogCacheList.length >= LogUtils.logOutputCount){
		while(LogUtils.stackLogCacheList.length > 0){
			console.log(LogUtils.stackLogCacheList.shift());
		}
	}
}

LogUtils.funcIn = function (fileName_,callArguments_) { 
	if (!LogUtils.logging){
		return
	}
	var _currentFunc = null;//递归传递用
	var _fileAndFuncName = null;
	var _currentCount = 0;//递归层数
	var _callArguments = null;
	if(arguments.length != 4){
		_currentFunc = arguments.callee.caller;
		_fileAndFuncName = fileName_ + " -> " + _currentFunc.name;
		_currentCount = 0;
		_callArguments = callArguments_;
	}else{
		_currentFunc = arguments[0];
		_fileAndFuncName = arguments[1];
		_currentCount = arguments[2];
		_callArguments = arguments[3];
	}
	_currentCount ++;
	if (_currentFunc == null){//结束，到底
		var _stackLength = _currentCount - 2;
		if (LogUtils.lockLogStackLength != -1){//有层级输出限制
			if (!LogUtils.lockLogAfter){
				LogUtils.lockLogStackLength = -1;
			}else{
				if (_stackLength > LogUtils.lockLogStackLength ){//大于输出限制的Log，直接过滤掉
					return;
				}
				if (_stackLength <= LogUtils.lockLogStackLength){//在过滤后续Log的选项下，堆栈长度小于限制长的时候				
					LogUtils.lockLogStackLength = -1;//就是退出了需要过滤的方法,需要还原限制
				}
			}
		}else{
			if (LogUtils.filterList.indexOf(_fileAndFuncName) >= 0){//存在文件名和方法的过滤
				if (LogUtils.lockLogAfter){//后续log不输出
					LogUtils.lockLogStackLength = _stackLength;
				}
				return;
			}
		}
		while (LogUtils.stackIndentList.length < _stackLength){
			var _stackIndentStr = "";
			for (var _idx = 0 , _len = (LogUtils.stackIndentList.length + 1); _idx < _len; _idx++) {
				if (_idx == (_len - 1)){
					_stackIndentStr += "   "
				}else{
					_stackIndentStr += "   |"	
				}
			} 
			LogUtils.stackIndentList.push(_stackIndentStr);
		}
		//参数
		var _parameterStr = "";
		if (_callArguments.length > 0){
			_parameterStr += " ( ";
			for (var _idx = 0 , _len = _callArguments.length; _idx < _len; _idx++) {
				var _argument = _callArguments[_idx];
				if (_idx != (_len - 1)){
					_parameterStr += _argument.toString() + " , ";
				}else{
					_parameterStr += _argument.toString();
				}
			}
			_parameterStr += " )";
		}
		//Log 输出
		LogUtils.doLog(LogUtils.stackIndentList[_stackLength - 1] + _fileAndFuncName + _parameterStr)
		return;
	}else{
		arguments.callee(_currentFunc.caller, _fileAndFuncName,_currentCount,_callArguments);
	}
}

function a(aKey_) {
	LogUtils.funcIn("A",arguments);
	b("bValue");
}
function b(bKey_) {
	LogUtils.funcIn("B",arguments);
	c("cValue");
}
function c(cKey_) {
	LogUtils.funcIn("C",arguments);
	d("dValue");
}
function d(dKey_) {
	LogUtils.funcIn("D",arguments);
	var e = function (eKey_) {
		LogUtils.funcIn("E",arguments);
	}
	e("eValue");
}

a("aValue");
a("aValue");
LogUtils.lockLogAfter = false
a("aValue");
LogUtils.lockLogAfter = true
a("aValue");
