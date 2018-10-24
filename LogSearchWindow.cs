using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;

public class LogSearchWindow : EditorWindow
{
    private string keywordText;
    private string logSearchText;
    private List<string> logAll = new List<string>();
    private List<string> logSearchResult = new List<string>();
    private static string logDetail = "";
    // private static bool isCollapse = true;
    // private static bool isClearOnPlay = true;
    // private static bool isErrorPause = true;
    private static Rect cursorChangeRect;
    private static bool resize = false;
    private static float currentScrollViewHeight = 300;
    private static LogSearchWindow mLogSearchWindow;
    private static int selectRow = -1;

    public static bool hasInit;
    //ICON
    public static Texture2D iconInfo;
    public static Texture2D iconWarn;
    public static Texture2D iconError;
    public static Texture2D iconInfoSmall;
    public static Texture2D iconWarnSmall;
    public static Texture2D iconErrorSmall;
    public static Texture2D iconInfoMono;
    public static Texture2D iconWarnMono;
    public static Texture2D iconErrorMono;
    //GUIStyle
    public static GUIStyle Box;
    public static GUIStyle Button;
    public static GUIStyle MiniButton;
    public static GUIStyle MiniButtonLeft;
    public static GUIStyle MiniButtonMiddle;
    public static GUIStyle MiniButtonRight;
    public static GUIStyle LogStyle;
    public static GUIStyle WarningStyle;
    public static GUIStyle ErrorStyle;
    public static GUIStyle EvenBackground;
    public static GUIStyle OddBackground;
    public static GUIStyle MessageStyle;
    public static GUIStyle StatusError;
    public static GUIStyle StatusWarn;
    public static GUIStyle StatusLog;
    public static GUIStyle Toolbar;
    public static GUIStyle CountBadge;
    public Vector2 vectorTextScroll = Vector2.zero;
    public Vector2 scrollPos;

    void OnEnable() {
        mLogSearchWindow = this;
        //日志监控
        Application.RegisterLogCallback(HandleLog);
    }
    void OnDisable() {
        //关闭日志监控
        Application.RegisterLogCallback(null);
    }

    public void Init(){
        if (!hasInit){
            hasInit = true;
            //Icon
            /*iconInfo = LoadIcon("console.infoicon");
            iconWarn = LoadIcon("console.warnicon");
            iconError = LoadIcon("console.erroricon");
            iconInfoSmall = LoadIcon("console.infoicon.sml");
            iconWarnSmall = LoadIcon("console.warnicon.sml");
            iconErrorSmall = LoadIcon("console.erroricon.sml");
            iconInfoMono = LoadIcon("console.infoicon.sml");
            iconWarnMono = LoadIcon("console.warnicon.inactive.sml");
            iconErrorMono = LoadIcon("console.erroricon.inactive.sml"); */
            //Style
            Box = "CN Box";
            Button = "Button";
            MiniButton = "ToolbarButton";
            MiniButtonLeft = "ToolbarButton";
            MiniButtonMiddle = "ToolbarButton";
            MiniButtonRight = "ToolbarButton";
            Toolbar = "Toolbar";
            LogStyle = "CN EntryInfo";
            WarningStyle = "CN EntryWarn";
            ErrorStyle = "CN EntryError";
            EvenBackground = "CN EntryBackEven";
            OddBackground = "CN EntryBackodd";
            MessageStyle = "CN Message";
            StatusError = "CN StatusError";
            StatusWarn = "CN StatusWarn";
            StatusLog = "CN StatusInfo";
            CountBadge = "CN CountBadge";
            //size
            currentScrollViewHeight = 300;
            cursorChangeRect = new Rect(0, currentScrollViewHeight, EditorGUIUtility.currentViewWidth, 1f);
        }
    }

    //获取图标
    public static Texture2D LoadIcon(string name){
        Texture2D texture2D = (EditorGUIUtility.Load("icon.png") as Texture2D);
        return texture2D;
    }

    [MenuItem("LogSearch/Log Search")]
    public static void InitMenuItem()
    {
        LogSearchWindow window = LogSearchWindow.GetWindow<LogSearchWindow>("Log Search");
        window.Show();
    }

    public void OnGUI(){
        Event current = Event.current;
        //初始化
        Init();
        //绘画顶部Toolbar-Begin
        GUILayout.BeginHorizontal(Toolbar);
        if(GUILayout.Button("Clear All",MiniButton)){
            logAll.Clear();
            logSearchResult.Clear();
            logDetail = "";
        }
        EditorGUILayout.Space();
        /*isCollapse = GUILayout.Toggle(isCollapse, "Collapse", MiniButtonLeft);
        isClearOnPlay =GUILayout.Toggle(isClearOnPlay, "Clear on Play", MiniButtonMiddle);
        isErrorPause = GUILayout.Toggle(isErrorPause, "Error Pause", MiniButtonRight); */
        EditorGUILayout.Space();
        keywordText = EditorGUILayout.TextField(keywordText);
        if(GUILayout.Button("Search",MiniButton)){
            logDetail = "";
            logSearchResult.Clear();
            if(string.IsNullOrEmpty(keywordText)){
                logSearchResult = logAll;
            }else{
                try{
                    foreach(string outString in logAll){
                        Regex r = new Regex(".*"+keywordText+".*");
                        Match m = r.Match(outString);
                        if(m.Success){
                            logSearchResult.Add(outString);
                        }
                    }
                }finally{

                }
            }
            mLogSearchWindow.Repaint();
        }
        /*if(GUILayout.Button("Next",MiniButton)){

        }
        if(GUILayout.Button("Last",MiniButton)){

        } */
        if(GUILayout.Button("Clear Search",MiniButton)){
            logSearchResult.Clear();
            logDetail = "";
        }
        GUILayout.FlexibleSpace();
        // GUILayout.Toggle(flag, new GUIContent("10",iconInfoSmall), MiniButtonLeft);
        // GUILayout.Toggle(flag, new GUIContent("16",iconWarnSmall), MiniButtonLeft);
        // GUILayout.Toggle(flag, new GUIContent("3",iconErrorSmall), MiniButtonLeft);
        GUILayout.EndHorizontal();
        //绘画顶部Toolbar-End

        //日志列表
        GUILayout.BeginVertical();
        scrollPos =  EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Height(currentScrollViewHeight-20));
        GUILayout.Label("",GUILayout.Height(logSearchResult.Count*35));
        EditorGUIUtility.SetIconSize(new Vector2(32f, 32f));
        GUIContent gUIContent = new GUIContent();
        int controlID = GUIUtility.GetControlID(FocusType.Native);
        int y = 0;
        int index = 0;
        try{
            foreach (string outString in logSearchResult)
            {
                Rect position = new Rect(0, y, EditorGUIUtility.currentViewWidth - 20, 35);
                GUIStyle gUIStyle = (index % 2 != 0) ? EvenBackground : OddBackground;
                GUIStyle styleForErrorMode = LogStyle;
                if(current.type == EventType.Repaint){
                    if(index == selectRow){
                        gUIStyle = "IN SelectedLine";
                    }
                    //画背景
                    gUIStyle.Draw(position, isHover: false, isActive: false, on:false, hasKeyboardFocus: true);
                    //画图标及日志内容
                    gUIContent.text = outString;
                    styleForErrorMode.Draw(position, gUIContent, controlID, true);
                    //画右侧日志重复数值
                    /*if (isCollapse)
                    {
                        gUIContent.text = "80";
                        Vector2 vector = CountBadge.CalcSize(gUIContent);
                        position.xMin = position.xMax - vector.x;
                        position.yMin += (position.yMax - position.yMin - vector.y) * 0.5f;
                        position.x -= 5f;
                        GUI.Label(position, gUIContent, CountBadge);
                    } */
                }
                if (current.type == EventType.mouseDown && position.Contains(Event.current.mousePosition)){
                    selectRow = index;
                    logDetail = outString;
                    mLogSearchWindow.Repaint();
                }
                y += 35;
                index += 1;
            }
            if(logSearchResult.Count == 0){
                selectRow = -1;
                logDetail = "";
            }
        }finally{

        }
        EditorGUILayout.EndScrollView();
        //画一条可以拖拽滚动区域大小的线
        Color colorDefault = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(cursorChangeRect, EditorGUIUtility.whiteTexture);
        Rect mouseCursorRect = new Rect(cursorChangeRect.x,cursorChangeRect.y-10,cursorChangeRect.width,cursorChangeRect.height+20);
        EditorGUIUtility.AddCursorRect(mouseCursorRect, MouseCursor.ResizeVertical);
        if (Event.current.type == EventType.mouseDown && mouseCursorRect.Contains(Event.current.mousePosition)){
            resize = true;
        }
        if (resize){
            currentScrollViewHeight = Event.current.mousePosition.y;
            cursorChangeRect.Set(cursorChangeRect.x, currentScrollViewHeight, cursorChangeRect.width, cursorChangeRect.height);
        }
        if (Event.current.type == EventType.MouseUp){
            resize = false;
        }
        GUI.color = colorDefault;
            
        //日志详情
        //vectorTextScroll = GUILayout.BeginScrollView(vectorTextScroll,Box);
        EditorGUILayout.SelectableLabel("\n"+logDetail, MessageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(50));
        //GUILayout.EndScrollView();
        GUILayout.EndVertical();
        
        // GUIStyle inputStyle = GUI.skin.textField;
        // inputStyle.richText = true;
        // logSearchText = EditorGUILayout.TextField("匹配结果",logSearchText,inputStyle,GUILayout.ExpandHeight(true));
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        logAll.Add(logString);
        if(string.IsNullOrEmpty(keywordText)){
            logSearchResult.Add(logString);
            mLogSearchWindow.Repaint();
        }else{
            Regex r = new Regex(".*"+keywordText+".*");
            Match m = r.Match(logString);
            if(m.Success){
                logSearchResult.Add(logString);
                mLogSearchWindow.Repaint();
            }
        }
    }

}
