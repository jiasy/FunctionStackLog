using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LogUtils {
	//上一个Log输出的层级
	public static string logPath = "/Volumes/Files/develop/selfDevelop/Unity/Flash2Unity2018/C#Temp/C#Log";
	//过滤数组，按照 类名 -> 方法名 的格式进行过滤
	public static string[] filterList = {
		"MainClass -> F3_IDX3",
		"MainClass -> F4_IDX2"
	};
	//前端空白拼接的缓存
	public static List<StringBuilder> stackIndentList = new List<StringBuilder> ();
	//实际LOG的缓存
	public static List<StringBuilder> stackLogCacheList = new List<StringBuilder> ();
	//Log输出中
    public static bool logging = false;
	//发生过滤后，后续Log是否继续输出
	public static bool lockLogAfter = false;
	//当发生堆栈锁后，后续的高于指定层级的Log将不再输出，直至层数跌回指定层级以下
	public static int lockLogStackLength = -1;
	//达到多少才输出
	public static int logOutputCount = 1;
	//显示没有添加过输出，但是在实际调用中发生的Log
    public static bool recoverLog = false;
	//当前执行堆栈
	public static List<string> currentStackList;
	//上一次执行堆栈
	public static List<string> lastStackList = new List<string> ();

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
    public static void toggleLog(){
        logging = !logging;
    }
    public static string isFilterFileAndFunc(string className_, string funcName_)
    {
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
	public static int lastSameIdx(List<string> currentList_,List<string> lastList_){
		int _sameIdx = 0;
		for (int _idx = 0; _idx < lastList_.Count; _idx++){
			string _funcStr = lastList_[_idx];
			if (_idx >= currentList_.Count){
				return _sameIdx;
			}
			if (_funcStr == currentList_[_idx]){
				_sameIdx = _idx;
			}
		}
		return _sameIdx;
	}
	// 调用方式
	// LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName," ( aKey_ = aValue , bKey_ = bValue ) ");
	public static void FuncIn(string className_,string parameters_ = ""){
		
		StackTrace stackTraceInstance = new System.Diagnostics.StackTrace();//当前堆栈
		int _stackIndentCount = stackTraceInstance.FrameCount;//层数
		cacheStackIndent(_stackIndentCount);//创建对应层数的前缀
		
		if (lockLogStackLength != -1){//层级锁
			if (!lockLogAfter){//不是后续不显示
				lockLogStackLength = -1;//层级锁还原
			}else{//是后续不显示
				if (_stackIndentCount > lockLogStackLength){//大于层级就过滤
					return;
				}
			}
		}

		StackFrame _stackFrame = stackTraceInstance.GetFrame(1);//调用LogUtils.FunIn的方法
		string _classAndFuncStr = isFilterFileAndFunc(className_,_stackFrame.GetMethod().Name);
		if (_classAndFuncStr == ""){//如果是过滤方法的话
			if(lockLogAfter){//是后续不显示的话
				lockLogStackLength = _stackIndentCount;//层级锁开启
			}
			return;
		}
		
		if (recoverLog){
			//当前堆栈方法名队列
			currentStackList = new List<string>();
			for (int _idx = 0; _idx < _stackIndentCount; _idx++){//不算最后一个LogUitls.FunIn
				string _funcStr = stackTraceInstance.GetFrame(_stackIndentCount - _idx - 1).GetMethod().Name;
				currentStackList.Add(_funcStr);
			}
			int _lastSameIdx = lastSameIdx(currentStackList,lastStackList);
			int _startIdx = (_lastSameIdx + 1);
			int _endLength = ( _stackIndentCount - 2);
			if (_startIdx<_endLength){
				for (int _idx = _startIdx; _idx <_endLength; _idx++){
					StringBuilder _logRecover = new StringBuilder ();
					_logRecover.Append (stackIndentList[_idx + 1]);
                    _logRecover.Append("[?]");
                    _logRecover.Append(" -> ");
					_logRecover.Append (currentStackList[_idx]);// 方法
					stackLogCacheList.Add(_logRecover);//缓存Log
				}
			}
		}

		StringBuilder _log = new StringBuilder ();
		_log.Append (stackIndentList[ _stackIndentCount - 1]);
		_log.Append (_classAndFuncStr);//拼接 类 -> 方法
		if (parameters_ != ""){//拼接参数
			_log.Append(parameters_);
		}
		stackLogCacheList.Add(_log);//缓存Log
		if(stackLogCacheList.Count >= logOutputCount){//当缓存大于指定数值
			StringBuilder _logCache = new StringBuilder();//log缓存的拼接
			for (int _idx = 0; _idx < stackLogCacheList.Count; _idx++){
				StringBuilder _tempLog = stackLogCacheList[_idx];//当前Log
                string _tempLogStr = _tempLog.ToString();
                if (logging == false && loggingUntil != null && loggingUntil == _tempLogStr){
                    logging = true;
                }
                if (!logging){
                    continue;
                }
				_tempLog.Append ("\n");//每个Log间添加换行
                _logCache.Append("C# >"); //拼接
				_logCache.Append(_tempLog);	//拼接
			}
			stackLogCacheList.Clear();//清理Log
            if (logging){
                string _logCacheStr = _logCache.ToString(); //转换成字符串
                //追加内容 只能以写的方式
                FileStream fs = new FileStream(logPath,FileMode.Append,FileAccess.Write);
                byte[] bs = System.Text.Encoding.Default.GetBytes(_logCacheStr);
                fs.Seek(0, SeekOrigin.End);
                fs.Write(bs,0,bs.Length);
                fs.Flush();//清除缓冲区，把所有数据写入文件
                fs.Close();
                fs.Dispose();
                //System.IO.File.AppendAllText(logPath, _logCacheStr);
            }
        }

        lastStackList = currentStackList;
    }
}


class MainClass{
	static void Main(string[] args){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		F5_IDX1("aValue");
		F4_IDX2("aValue");
		F1_IDX5("aValue");
	}
	static void F1_IDX5(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
	}
	static void F2_IDX4(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		F1_IDX5("1Value");
	}
	static void F3_IDX3(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		F2_IDX4("2Value");
	}
	static void F4_IDX2(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		F3_IDX3("3Value");
	}
	static void F5_IDX1(string bKey_){
		LogUtils.FuncIn(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName);
		F4_IDX2("4Value");
	}
}
