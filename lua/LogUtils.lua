LogUtils = LogUtils or {}
LogUtils.stackIndentList = {}
LogUtils.fileNameCacheDict = {}
LogUtils.lastSackIndentCount = -1
LogUtils.filterList = {
	"LogUtils -> c",
}
LogUtils.lockLogStackLength = -1
LogUtils.lockLogAfter = true --发生过滤后，后续Log是否继续输出
LogUtils.logging = true
LogUtils.logOutputCount = 1
LogUtils.stackLogCacheList = {} --Log缓存
LogUtils.logCount = 0
LogUtils.targetLogCount = 0

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
	while true do
		local _stackFrameLoop = debug.getinfo( _loopStackLevel, "nSl") 
		if _stackFrameLoop == nil then 
			break
		end
		_loopStackLevel = _loopStackLevel + 1
	end
	-- 缓存缩进空白
	local _stackIndentCount = _loopStackLevel - 3
	LogUtils.cacheStackIndent(_stackIndentCount)
	
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
	f(function() LogUtils.funcIn()
		e("eValue")
	end)
end


function f(fParameterFunc)
	LogUtils.funcIn()
	fParameterFunc()
end



a("aValue")
a("aValue")
LogUtils.lockLogAfter = false
a("aValue")
LogUtils.lockLogAfter = true
a("aValue")

