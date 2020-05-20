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
|   MovieClip -> frameUpdate  
|   |   MovieClip -> updateCurrentFrame  
|   |   MovieClip -> updateChildrenOnStage  
|   |   MovieClip -> runActions  
|   |   |   MovieClip -> doFrameActionByFrameInt(frameInt_ : 2)  
|   |   MovieClip -> enterFrame  
|   |   |   MovieClip -> updateDisplayList(frameChanged_ : True)  
|   |   |   |   DisplayObject -> syncPropertys(parentCurrentFrame_ : 2)  
|   |   |   |   DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 2)  
|   |   |   |   Sprite -> frameUpdate  
|   |   |   |   |   |   Sprite -> updateRGB(parentCurrentFrame_ : 2)  
|   |   |   |   |   |   Sprite -> updateAlpha(parentCurrentFrame_ : 2,parentsAlpha_ : 1)  
|   |   |   |   |   |   DisplayObject -> frameUpdateEnd  
|   |   |   |   DisplayObject -> syncPropertys(parentCurrentFrame_ : 2)  
|   |   |   |   DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 2)  
|   |   |   |   Sprite -> frameUpdate  
|   |   |   |   |   |   Sprite -> updateRGB(parentCurrentFrame_ : 2)  
|   |   |   |   |   |   Sprite -> updateAlpha(parentCurrentFrame_ : 2,parentsAlpha_ : 1)  
|   |   |   |   |   |   DisplayObject -> frameUpdateEnd  
|   |   |   |   DisplayObject -> syncPropertys(parentCurrentFrame_ : 2)  
|   |   |   |   DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 2)  
|   |   |   |   MovieClip -> frameUpdate  
|   |   |   |   |   |   MovieClip -> updateCurrentFrame  
|   |   |   |   |   |   MovieClip -> updateChildrenOnStage  
|   |   |   |   |   |   MovieClip -> runActions  
|   |   |   |   |   |   MovieClip -> enterFrame  
|   |   |   |   |   |   MovieClip -> updateDisplayList(frameChanged_ : True)  
|   |   |   |   |   |   |   DisplayObject -> syncPropertys(parentCurrentFrame_ : 1)  
|   |   |   |   |   |   |   DisplayObject -> getCurrentFrameAlpha(parentCurrentFrame_ : 1)  
|   |   |   |   |   |   |   Sprite -> frameUpdate  
|   |   |   |   |   |   |   |   Sprite -> updateRGB(parentCurrentFrame_ : 1)  
|   |   |   |   |   |   |   |   Sprite -> updateAlpha(parentCurrentFrame_ : 1,parentsAlpha_ : 1)  
|   |   |   |   |   |   |   |   DisplayObject -> frameUpdateEnd  
|   |   |   |   |   |   DisplayObject -> frameUpdateEnd  
|   |   DisplayObject -> frameUpdateEnd  
...