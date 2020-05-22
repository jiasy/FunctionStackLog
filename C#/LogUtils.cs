using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LogUtils {
	//过滤数组，按照 类名 -> 方法名 的格式进行过滤
	public static string[] filterList = {
		"MainClass -> c"
	};
	//前端空白拼接的缓存
	public static List<StringBuilder> stackIndentList = new List<StringBuilder> ();
	//实际LOG的缓存
	public static List<StringBuilder> _stackLogCacheList = new List<StringBuilder> ();
	//Log输出中
	public static bool logging = true;
	//发生过滤后，后续Log是否继续输出
	public static bool lockLogAfter = true;
	//当发生堆栈锁后，后续的高于指定层级的Log将不再输出，直至层数跌回指定层级以下
	public static int lockLogStackLength = -1;
	//达到多少才输出
	public static int logOutputCount = 1;
	//上一个Log输出的层级
	public static string logPath = "/Volumes/Files/develop/selfDevelop/Unity/Flash2Unity2018/C#Temp/C#Log";
	//记录Log的堆栈方法名
	public static List<string> addressStackList = new List<string> ();
	//显示没有添加过输出，但是在实际调用中发生的Log
	public static bool showAllLog = true;

	public static void cacheStackIndent(int stackFrameLength_){
		while (stackIndentList.Count < stackFrameLength_){
			StringBuilder _stackBlankPrefix =  new StringBuilder ();
			for (int _idx = 0; _idx < stackIndentList.Count; _idx++){
				if (_idx == (stackIndentList.Count - 1)){
					_stackBlankPrefix.Append ("   ");//最后一个是紧贴的
				}else{
					_stackBlankPrefix.Append ("   |");//前面需要加层级
				}
			}
			stackIndentList.Add(_stackBlankPrefix);
		} 
	}
	public static string isFilterFileAndFunc(string className_, string funcName_) {
		string[] _classNameArr;
		string _className = className_;
		if (isAContainsB(_className,"+")){
			_classNameArr = splitAWithB(_className,"+");
			_className = _classNameArr[_classNameArr.Length - 1];
		}else if (isAContainsB(_className,".")){
			_classNameArr = splitAWithB(_className,".");
			_className = _classNameArr[_classNameArr.Length - 1];
		}

		StringBuilder _classAndFunc = new StringBuilder ();//拼接类方法 
		_classAndFunc.Append (_className);
		_classAndFunc.Append (" -> ");
		_classAndFunc.Append (funcName_);

		string _classAndFuncStr = _classAndFunc.ToString();
		for(int _idx = 0;_idx < filterList.Length; _idx++){
			if (filterList[_idx] == _classAndFuncStr){
				return "";
			}
		}
		
		return _classAndFuncStr;
	}
	public static string getMemory(object o) {// 获取引用类型的内存地址方法    
		GCHandle h = GCHandle.Alloc(o, GCHandleType.WeakTrackResurrection);
		IntPtr addr = GCHandle.ToIntPtr(h);
		return addr.ToString("X");
	}
	//切开字符串
	public static string[] splitAWithB (string a_, string b_) {
		Char[] _bChars = b_.ToCharArray ();
		string[] _aList = a_.Split (_bChars);
		return _aList;
	}
	
	//包含判断
	public static bool isAContainsB (string a_, string b_, StringComparison comp_ = StringComparison.OrdinalIgnoreCase) {
		return a_.IndexOf (b_, comp_) >= 0;
	}
	// 调用方式
	// LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName," ( aKey_ = aValue , bKey_ = bValue ) ");
	public static void FuncIn(string className_,string parameters_ = ""){
		if (logging == false){
			return;
		}
		
		StackTrace stackTraceInstance = new System.Diagnostics.StackTrace();
		int _stackIndentCount = stackTraceInstance.FrameCount;
		cacheStackIndent(_stackIndentCount);
		
		//System.IO.File.AppendAllText(logPath,"_stackIndentCount : " + _stackIndentCount + "\n");
		
		if (lockLogStackLength != -1){
			if (!lockLogAfter){
				lockLogStackLength = -1;
			}else{
				if (_stackIndentCount > lockLogStackLength){
					return;
				}
			}
		}
		
		//如果Log输出，发生越级，这里会追溯当前的堆栈，知道找出最后一个方法名和当前堆栈位置的方法名相同的层级。补全Log输出
		if(showAllLog){
			int _currentPreFrameCount = 0;
			while(_currentPreFrameCount < _stackIndentCount){
				if(addressStackList.Count > _currentPreFrameCount){
					StackFrame _preFrame = stackTraceInstance.GetFrame(_currentPreFrameCount);
					int _idx = addressStackList.Count - _currentPreFrameCount - 1;
					//地址一致，找到前一层
					if (addressStackList[_idx] == _preFrame.GetMethod().Name){
						break;
					}
				}
				_currentPreFrameCount += 1;
			}
			
			//补充中间的Log
			if (_currentPreFrameCount > 2){
				//最后两个一个是给LogUtils. 一个是给 stackTraceInstance.GetFrame(1)
				for (int _count = 0 ;_count < (_currentPreFrameCount - 2);_count ++){
					StackFrame _currentStackFrame = stackTraceInstance.GetFrame(_currentPreFrameCount - _count - 1);
					string _fileName = _currentStackFrame.GetFileName();
					string _fileAndFuncStr = null;
					if (_fileName == null){
						_fileAndFuncStr = isFilterFileAndFunc("<UNKNOW>",_currentStackFrame.GetMethod().Name);
					}else{
						_fileAndFuncStr = isFilterFileAndFunc(_fileName,_currentStackFrame.GetMethod().Name);
					}
					StringBuilder _logFill = new StringBuilder ();
					cacheStackIndent(_count + 1);
					_logFill.Append (stackIndentList[_count + 1]);
					_logFill.Append (_fileAndFuncStr);//拼接 类 -> 方法
					_stackLogCacheList.Add(_logFill);//缓存Log
				}
			}
		}

		StackFrame _stackFrame = stackTraceInstance.GetFrame(1);
		string _classAndFuncStr = isFilterFileAndFunc(className_,_stackFrame.GetMethod().Name);
		if (_classAndFuncStr == ""){
			if(lockLogAfter){
				lockLogStackLength = _stackIndentCount;
			}
			return;
		}
		
		StringBuilder _log = new StringBuilder ();
		_log.Append (stackIndentList[ _stackIndentCount - 1]);
		_log.Append (_classAndFuncStr);//拼接 类 -> 方法
		if (parameters_ != ""){//拼接参数
			_log.Append(parameters_);
		}
		_stackLogCacheList.Add(_log);//缓存Log
		if(_stackLogCacheList.Count >= logOutputCount){//当缓存大于指定数值
			StringBuilder _logCache = new StringBuilder();//log缓存的拼接
			for (int _idx = 0; _idx < _stackLogCacheList.Count; _idx++){
				StringBuilder _tempLog = _stackLogCacheList[_idx];//当前Log
				_tempLog.Append ("\n");//每个Log间添加换行
				_logCache.Append(_tempLog);	//拼接
			}
			_stackLogCacheList.Clear();//清理Log
			string _logCacheStr = _logCache.ToString();//转换成字符串
			Console.Write(_logCacheStr);
			//System.IO.File.AppendAllText(logPath,_logCacheStr);
		}

		if (showAllLog){
			//同步方法名，不够就就找到够为止
			while (addressStackList.Count < _stackIndentCount){
				int _idx = _stackIndentCount - addressStackList.Count - 1;
				StackFrame _stackFrameSupplement = stackTraceInstance.GetFrame(_idx);
				addressStackList.Add(_stackFrameSupplement.GetMethod().Name);
			}
			//多了删到正好为止
			while (addressStackList.Count > _stackIndentCount){
				addressStackList.RemoveAt(addressStackList.Count - 1);
			}
		}
	}
}

class MainClass{
	static void Main(string[] args){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		a("aValue");
		a("aValue");
		LogUtils.lockLogAfter = false;
		a("aValue");
		LogUtils.lockLogAfter = true;
		a("aValue");
	}
	static void a(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		b("bValue");
	}
	static void b(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		c("cValue");
	}
	static void c(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		d("dValue");
	}
	static void d(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		e("eValue");
	}
	static void e(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
	}
}
