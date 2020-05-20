using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
public class LogUtils {
	//过滤数组
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
	public static int lockLogStackLength = -1;
	//达到多少才输出
	public static int logOutputCount = 1;
	public static string logPath = "/Volumes/Files/develop/selfDevelop/Unity/Flash2Unity2018/C#Temp/C#Log";
	
	public static void cacheStackIndent(int stackFrameLength_){
		while (stackIndentList.Count < stackFrameLength_){
			StringBuilder _stackBlankPrefix =  new StringBuilder ();
			for (int _idx = 0; _idx < stackFrameLength_ - 1; _idx++){
				if (_idx == (stackFrameLength_ - 2)){
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
		StackTrace _stackTrace = new System.Diagnostics.StackTrace();
		
		int _stackIndentCount = _stackTrace.FrameCount;
		cacheStackIndent(_stackIndentCount);
		
		StackFrame _stackFrame = _stackTrace.GetFrame(1);
		if (lockLogStackLength != -1){
			if (!lockLogAfter){
				lockLogStackLength = -1;
			}else{
				if (_stackIndentCount > lockLogStackLength){
					return;
				}
			}
		}
		
		string _classAndFUncStr = isFilterFileAndFunc(className_,_stackFrame.GetMethod().Name);
		if (_classAndFUncStr == ""){
			if(lockLogAfter){
				lockLogStackLength = _stackIndentCount;
			}
			return;
		}
		
		StringBuilder _log = new StringBuilder ();
		_log.Append (stackIndentList[ _stackIndentCount - 1]);
		_log.Append (_classAndFUncStr);//拼接 类 -> 方法
		if (parameters_ != ""){//拼接参数
			_log.Append(parameters_);
		}
		_stackLogCacheList.Add(_log);//缓存Log
		if(_stackLogCacheList.Count >= logOutputCount){//当缓存大于指定数值
			StringBuilder _logCache = new StringBuilder();//log缓存的拼接
			for (int _idx = 0; _idx < logOutputCount; _idx++){
				StringBuilder _tempLog = _stackLogCacheList[_idx];//当前Log
				_tempLog.Append ("\n");//每个Log间添加换行
				_logCache.Append(_tempLog);	//拼接
			}
			_stackLogCacheList.Clear();//清理Log
			string _logCacheStr = _logCache.ToString();//转换成字符串
			Console.Write(_logCacheStr);
			//System.IO.File.AppendAllText(logPath,_logCacheStr);
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
