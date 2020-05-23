var LogUtils = {}
LogUtils.currentStack = 0;
LogUtils.stackIndentList = [];
LogUtils.lastSackIndentCount = -1;
LogUtils.filterList = [
	"C3 -> f3",
	"C2 -> f2",
];
LogUtils.lockLogStackLength = -1;
LogUtils.lockLogAfter = true;//发生过滤后，后续Log是否继续输出
LogUtils.logging = true;//当前是否处于log状态
LogUtils.recoverLog = false;
LogUtils.logOutputCount = 1;//凑够多少条再输出
LogUtils.stackLogCacheList = [];//Log缓存
LogUtils.logCount = 0;
LogUtils.targetLogCount = 0;
LogUtils.lastStackList = [];


LogUtils.doLog = function (logStr_) {
	LogUtils.stackLogCacheList.push(logStr_);
	if (LogUtils.stackLogCacheList.length >= LogUtils.logOutputCount){
		while(LogUtils.stackLogCacheList.length > 0){
			console.log(LogUtils.stackLogCacheList.shift());
		}
	}
}

LogUtils.lastSameIdx = function(currentList_,lastList_){
	var _sameIdx = 0;
	for (var _idx = 0; _idx < lastList_.length; _idx++){
		var _funcStr = lastList_[_idx];
		if (_idx >= currentList_.length){
			return _sameIdx;
		}
		if (_funcStr == currentList_[_idx]){
			_sameIdx = _idx;
		}
	}
	return _sameIdx;
}

LogUtils.cacheStackIndent = function (stackFrameLength_) {
	while (LogUtils.stackIndentList.length < stackFrameLength_){
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
}

LogUtils.funcIn = function (fileName_,callArguments_) {
	var _fileAndFuncName = fileName_ + " -> " + arguments.callee.caller.name;
	var _currentCount = 0;//递归层数
	var _parameterStr = "";//参数
	var _currentStackList = [];
	
	if (callArguments_.length > 0){
		_parameterStr += " ( ";
		for (var _idx = 0 , _len = callArguments_.length; _idx < _len; _idx++) {
			var _argument = callArguments_[_idx];
			if (_idx != (_len - 1)){
				_parameterStr += _argument.toString() + " , ";
			}else{
				_parameterStr += _argument.toString();
			}
		}
		_parameterStr += " )";
	}

	var _calleeStackFunc = function (caller_) {
		_currentCount ++;
		LogUtils.cacheStackIndent(_currentCount);
		var _stackLength = _currentCount - 2;
		if (LogUtils.lockLogStackLength != -1){//有层级输出限制
			if (!LogUtils.lockLogAfter){//不是后续不输出
				LogUtils.lockLogStackLength = -1;//还原变量
			}else{
				if (_stackLength > LogUtils.lockLogStackLength ){//大于输出限制的Log，直接过滤掉
					return;
				}
			}
		}else{//没有有层级限制
			if (LogUtils.filterList.indexOf(_fileAndFuncName) >= 0){//存在文件名和方法的过滤
				if (LogUtils.lockLogAfter){//后续log不输出
					LogUtils.lockLogStackLength = _currentCount;
				}
				return;
			}
		}
		if (caller_ == null){//结束，到底
			if (LogUtils.lockLogStackLength != -1){//有层级输出限制
				if (LogUtils.lockLogAfter){//不是后续不输出
					if (_stackLength <= LogUtils.lockLogStackLength){//在过滤后续Log的选项下，堆栈长度小于限制长的时候				
						LogUtils.lockLogStackLength = -1;//就是退出了需要过滤的方法,需要还原限制
					}
				}
			}
			if (LogUtils.recoverLog){//追溯中间的LOG
				_currentStackList.reverse();//数组倒叙
				var _lastSameIdx = LogUtils.lastSameIdx(_currentStackList,LogUtils.lastStackList);
				var _startIdx = (_lastSameIdx + 1);
				if (_startIdx < _stackLength){
					for (var _idx = _startIdx; _idx < _stackLength; _idx++){
						LogUtils.doLog(LogUtils.stackIndentList[_idx - 1] + "? -> " +_currentStackList[_idx]);//缓存Log
					}
				}
				LogUtils.lastStackList = _currentStackList;
			}
			LogUtils.doLog(LogUtils.stackIndentList[_stackLength - 1] + _fileAndFuncName + _parameterStr)
		}else{
			_currentStackList.push(caller_.name)
			_calleeStackFunc(caller_.caller);
		}
	}
	_calleeStackFunc(arguments.callee.caller)
}

function f1(aKey_) {
	LogUtils.funcIn("C1",arguments);
	f2("bValue");
}
function f2(bKey_) {
	LogUtils.funcIn("C2",arguments);
	f3("cValue");
}
function f3(cKey_) {
	LogUtils.funcIn("C3",arguments);
	f4("dValue");
}
function f4(dKey_) {
	LogUtils.funcIn("C4",arguments);
	var f5 = function (eKey_) {
		LogUtils.funcIn("C4_E",arguments);
	}
	f5("eValue");
}

f1("aValue");
