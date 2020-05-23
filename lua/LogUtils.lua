LogUtils = LogUtils or {}
LogUtils.stackIndentList = {}
LogUtils.fileNameCacheDict = {}
LogUtils.lastSackIndentCount = -1
LogUtils.filterList = {
	"LogUtils -> b",
	"LogUtils -> c",
}
LogUtils.lockLogStackLength = -1
LogUtils.lockLogAfter = false --发生过滤后，后续Log是否继续输出
LogUtils.logging = true
LogUtils.recoverLog = true
LogUtils.logOutputCount = 1
LogUtils.stackLogCacheList = {} --Log缓存
LogUtils.logCount = 0
LogUtils.targetLogCount = 0
LogUtils.lastStackList = {}

function LogUtils.split(input, delimiter)--字符串切割
	input = tostring(input)
	delimiter = tostring(delimiter)
	if (delimiter=='') then 
		return false
	end
	local pos,arr = 0, {}
	for st,sp in function() return string.find(input, delimiter, pos, true) end do
		table.insert(arr, string.sub(input, pos, st - 1))
		pos = sp + 1
	end
	table.insert(arr, string.sub(input, pos))
	return arr
end

function LogUtils.cacheStackIndent(stackIndentCount_)
	while #LogUtils.stackIndentList < stackIndentCount_ do
		local _indentLength =  #LogUtils.stackIndentList -- 有移除操作的不要用这个写法
		local _indentStr = ""
		for _idx = 1 , _indentLength do
			if _idx == _indentLength then
				_indentStr = _indentStr .. "   "
			else
				_indentStr = _indentStr .. "   |"
			end
		end
		table.insert(LogUtils.stackIndentList, _indentStr)
	end
end

function LogUtils.getFileShortName(fileName_)
	local _fileShortName = LogUtils.fileNameCacheDict[fileName_]
	if _fileShortName == nil then
		local _fileNameArr = LogUtils.split(fileName_,"@")
		_fileShortName = table.remove(_fileNameArr,#_fileNameArr)
		_fileNameArr = LogUtils.split(_fileShortName,".lua")
		_fileShortName = _fileNameArr[1]
		LogUtils.fileNameCacheDict[fileName_] = _fileShortName
	end
	return _fileShortName
end

function LogUtils.isFilterFileAndFunc(fileName_,funcName_)
	local _fileShortName = LogUtils.getFileShortName(fileName_)-- 文件短名
	local _fileAndFuncName = _fileShortName .. " -> " .. funcName_-- 文件名 -> 方法名
	for _idx = 1,table.getn(LogUtils.filterList) do-- 判断是否过滤
		if _fileAndFuncName == LogUtils.filterList[_idx] then 
			return nil
		end
	end
	return _fileAndFuncName
end

function LogUtils.doLog(logStr_)
	table.insert(LogUtils.stackLogCacheList, logStr_)
	if #LogUtils.stackLogCacheList >= LogUtils.logOutputCount then
		while #LogUtils.stackLogCacheList > 0 do
			print(table.remove(LogUtils.stackLogCacheList,1))
		end
	end
end

function LogUtils.reverseTable(sourceList_)
	local _tmpList = {}
	for _idx = 1, #sourceList_ do
		local key = #sourceList_
		_tmpList[_idx] = table.remove(sourceList_)
	end
	return _tmpList
end

function LogUtils.lastSameIdx(currentList_,lastList_)
	local _sameIdx = 1
	local _length =  #lastList_
	for _idx = 1 , _length do
		local _funcStr = lastList_[_idx]
		if _idx > #currentList_ then
			return _sameIdx
		end
		if _funcStr == currentList_[_idx] then
			_sameIdx = _idx
		end
	end
	return _sameIdx
end

function LogUtils.funcIn()
	if not LogUtils.logging then
		return
	end
	--0表示getinfo本身
	--1表示调用getinfo的函数(printCallStack)
	--2表示调用 LogUtils.funcIn 的函数,可以想象一个 getinfo(0级) 在顶的栈.
	local _stackLevel = 2
	-- 当前调用方法的堆栈
	local _stackFrame = debug.getinfo( _stackLevel, "nSl")
	-- 文件名，去掉前缀和文件后缀名，判断是否过滤
	local _fileAndFuncName = LogUtils.isFilterFileAndFunc(_stackFrame.source,_stackFrame.name)
	
	-- 取得堆栈长度
	local _loopStackLevel = _stackLevel
	local _currentStackList = {}
	while true do
		local _stackFrameLoop = debug.getinfo( _loopStackLevel, "nSl") 
		if _stackFrameLoop == nil then 
			break
		end
		_loopStackLevel = _loopStackLevel + 1
		table.insert(_currentStackList,_stackFrameLoop.name) -- 记录堆栈
	end
	-- 缓存缩进空白
	LogUtils.cacheStackIndent(_loopStackLevel)
	local _stackIndentCount = _loopStackLevel - 3

	if LogUtils.lockLogStackLength ~= -1 then
		if not LogUtils.lockLogAfter then
			LogUtils.lockLogStackLength = -1
		else
			if _stackIndentCount > LogUtils.lockLogStackLength then
				return
			end
		end
	end
	
	if _fileAndFuncName == nil then
		if LogUtils.lockLogAfter then
			LogUtils.lockLogStackLength = _stackIndentCount
		end
		return
	end
	
	if LogUtils.recoverLog then --追溯中间的LOG
		_currentStackList = LogUtils.reverseTable(_currentStackList) -- 数组倒叙
		local _lastSameIdx = LogUtils.lastSameIdx(_currentStackList,LogUtils.lastStackList) 
		local _startIdx = _lastSameIdx + 1
		if _startIdx < (_stackIndentCount - 2 )then
			for _idx = _startIdx , (_stackIndentCount - 2) do
				LogUtils.doLog(string.format("%s%s%s",LogUtils.stackIndentList[_idx + 1] ,"? -> ",_currentStackList[_idx]))
			end
		end
		LogUtils.lastStackList = _currentStackList
	end
	
	-- 参数
	local _parameterIdx = 1
	local _parameters = {}
	while true do
		local _key, _value = debug.getlocal( _stackLevel, _parameterIdx )
		if _key == nil then break end
		local _valueType = type( _value )
		local _valueStr
		if _valueType == 'string' then
			_valueStr = _value
		elseif _valueType == "number" then
			_valueStr = string.format("%.2f", _value)
		end
		if _valueStr ~= nil then
			table.insert(_parameters, string.format( "%s = %s", _key, _value ))	
		end
		_parameterIdx = _parameterIdx + 1
	end
	local _parameterStr = ""
	local _length = #_parameters
	if _length > 0 then
		_parameterStr = _parameterStr .. " ( "
		for _idx = 1 , _length do
			local _parameter = _parameters[_idx]
			if _idx ~= 1 then
				_parameterStr = _parameterStr .. " , " .. _parameter
			else
				_parameterStr = _parameterStr .. _parameter
			end
		end
		_parameterStr = _parameterStr .. " )"
	end
	
	LogUtils.doLog(string.format("%s%s%s",LogUtils.stackIndentList[_stackIndentCount],_fileAndFuncName,_parameterStr))
end


function a(aKey)
	LogUtils.funcIn()
	b("bValue",12)
end
function b(bKey,bKey2)
	LogUtils.funcIn()
	c("cValue")
end
function c(cValue)
	LogUtils.funcIn()
	d("dValue")
end
function d(dValue)
	LogUtils.funcIn()
	function e(eValue)
		LogUtils.funcIn()
	end
	e("eValue")
	f(function() 
		LogUtils.funcIn()
		e("eValue")
	end)
end


function f(fParameterFunc)
	LogUtils.funcIn()
	fParameterFunc()
end



a("aValue")
--a("aValue")
--LogUtils.lockLogAfter = false
--a("aValue")
--LogUtils.lockLogAfter = true
--a("aValue")

