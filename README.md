# UnityLogSearch
Unity Console Log Search

这个类部分代码来自与反汇编的UnityEditor.dll；
因为反汇编得到的代码需要因为特别多的internal内部类，而这个声明只能内部程序集内调用，dll外部调用不了，所以对一些功能的实现做了相应处理和重写，目前基本实现了日志查询和实时显示的功能，在几十条日志中查某个打印信息还是有点用的

已在Unity 4.7.2测试通过
