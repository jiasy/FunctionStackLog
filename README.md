方法运行时堆栈LOG  
目的 : 是为了简化代码的阅读，通过实际运行的Log查看调用情况。  
用途 : 接手其他人的项目，入职新公司阅读代码，查阅第三方库的运行状态。  
用法 : VSCode / SublimeText / 实现Ptyhon代码，对代码进行正则匹配。  
      为方法的第一行添加LogUtils输出。  
说明 : 辅助阅读代码的工具，也用于测试阶段的全量LOG，并不关注运行效率。  
实例图如下:  
    <img src="https://github.com/jiasy/FunctionStackLog/blob/master/C%23/C%23Result.png" width="1516" height="802">  
    <img src="https://github.com/jiasy/FunctionStackLog/blob/master/js/jsResult.png" width="1213" height="523">  
    <img src="https://github.com/jiasy/FunctionStackLog/blob/master/lua/LuaResult.png" width="1261" height="527">  
  
  
用于 https://github.com/jiasy/Flash2Unity/ 时，运行时的部分输出:  
...  
FlashManager -> Update  
|___MovieClip -> frameUpdate  
|___|___MovieClip -> updateCurrentFrame  
|___|___MovieClip -> updateChildrenOnStage  
|___|___MovieClip -> runActions  
|___|___|___MovieClip -> doFrameActionByFrameInt(frameInt_ : 2)  
|___|___MovieClip -> enterFrame  
|___|___|___MovieClip -> updateDisplayList(frameChanged_ : True)  
|___|___|___|___DisplayObject -> syncPropertys(parentCurrentFrame_ : 2)  
|___|___|___|___DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 2)  
|___|___|___|___Sprite -> frameUpdate  
|___|___|___|___|___|___Sprite -> updateRGB(parentCurrentFrame_ : 2)  
|___|___|___|___|___|___Sprite -> updateAlpha(parentCurrentFrame_ : 2,parentsAlpha_ : 1)  
|___|___|___|___|___|___DisplayObject -> frameUpdateEnd  
|___|___|___|___DisplayObject -> syncPropertys(parentCurrentFrame_ : 2)  
|___|___|___|___DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 2)  
|___|___|___|___Sprite -> frameUpdate  
|___|___|___|___|___|___Sprite -> updateRGB(parentCurrentFrame_ : 2)  
|___|___|___|___|___|___Sprite -> updateAlpha(parentCurrentFrame_ : 2,parentsAlpha_ : 1)  
|___|___|___|___|___|___DisplayObject -> frameUpdateEnd  
|___|___|___|___DisplayObject -> syncPropertys(parentCurrentFrame_ : 2)  
|___|___|___|___DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 2)  
|___|___|___|___MovieClip -> frameUpdate  
|___|___|___|___|___|___MovieClip -> updateCurrentFrame  
|___|___|___|___|___|___MovieClip -> updateChildrenOnStage  
|___|___|___|___|___|___MovieClip -> runActions  
|___|___|___|___|___|___MovieClip -> enterFrame  
|___|___|___|___|___|___MovieClip -> updateDisplayList(frameChanged_ : True)  
|___|___|___|___|___|___|___DisplayObject -> syncPropertys(parentCurrentFrame_ : 1)  
|___|___|___|___|___|___|___DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 1)  
|___|___|___|___|___|___|___Sprite -> frameUpdate  
|___|___|___|___|___|___|___|___Sprite -> updateRGB(parentCurrentFrame_ : 1)  
|___|___|___|___|___|___|___|___Sprite -> updateAlpha(parentCurrentFrame_ : 1,parentsAlpha_ : 1)  
|___|___|___|___|___|___|___|___DisplayObject -> frameUpdateEnd  
|___|___|___|___|___|___DisplayObject -> frameUpdateEnd  
|___|___DisplayObject -> frameUpdateEnd  
...
