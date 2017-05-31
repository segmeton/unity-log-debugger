using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//based on Plug-in Unity Logs Viewer 1.6 by Dream Makers Group

namespace Segmeton.UnityDebugger
{
    public class LogManager : MonoBehaviour
    {
        #region Variables
        private static LogManager instance;

        private bool isInitialized = false;
        private bool isLogShown = false;
        private bool isClearOnNewScene = false;

        private List<LogData> threadedLogs = new List<LogData>();
        private List<LogData> logs = new List<LogData>();

        private string deviceModel;
        private string deviceType;
        private string deviceName;
        private string systemMemorySize;
        private string operatingSystem;

        private Dictionary<string, string> cachedString = new Dictionary<string, string>();

        private string currentSceneName;

        private float fps;
        private int frames = 0;
        private bool firstTime = true;
        private float lastUpdate = 0f;
        private const int requiredFrames = 10;
        private const float updateInterval = 0.25f;

        public GameObject panelLog;
        public GameObject scrollViewLog;
        public Slider sliderLog;
        public Text textArea;
        public Text FpsCounter;
        #endregion

        #region Init
        private void Init()
        {
            if (isInitialized)
            {
                Destroy(this.gameObject);
                return;
            }

            Application.logMessageReceivedThreaded += CaptureLogThread;

            deviceModel = SystemInfo.deviceModel;
            deviceType = SystemInfo.deviceType.ToString();
            deviceName = SystemInfo.deviceName;
            systemMemorySize = SystemInfo.systemMemorySize.ToString();
            operatingSystem = SystemInfo.operatingSystem;

            Color tempPanelLogColor = panelLog.GetComponent<Image>().color;
            Color tempScrollViewLogColor = scrollViewLog.GetComponent<Image>().color;

            scrollViewLog.GetComponent<Image>().color = new Color(tempScrollViewLogColor.r, tempScrollViewLogColor.g, tempScrollViewLogColor.b, tempPanelLogColor.a);

            sliderLog.value = tempPanelLogColor.a;

            isLogShown = panelLog.activeInHierarchy;

            SceneManager.sceneLoaded += LoadedSceneDelegate;

            instance = this;

            isInitialized = true;
        }
        #endregion

        #region Unity
        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            SystemInfoLogs();
        }

        private void Update()
        {
            FpsCounter.text = fps.ToString("0.000");

            if (threadedLogs.Count > 0)
            {
                lock (threadedLogs)
                {
                    for (int i = 0; i < threadedLogs.Count; i++)
                    {
                        LogData log = threadedLogs[i];
                        AddLog(log.Condition, log.StackTrace, log.LogType);
                    }
                    threadedLogs.Clear();
                }
            }

            fps = UpdateFpsCounter();
        }

        private void OnLevelWasLoaded(int level)
        {
            if (level == 0)
            {
                Debug.Log("Lv 0");
            }
        }
        #endregion

        #region Logs
        private void CaptureLogThread(string condition, string stacktrace, LogType logType)
        {
            LogData log = new LogData();
            log.Init(condition, stacktrace, logType);

            lock (threadedLogs)
            {
                threadedLogs.Add(log);
            }
        }

        private void AddLog(string condition, string stacktrace, LogType type)
        {
            string _condition = "";
            if (cachedString.ContainsKey(condition))
            {
                _condition = cachedString[condition];
            }
            else
            {
                _condition = condition;
                cachedString.Add(_condition, _condition);
            }

            string _stacktrace = "";
            if (cachedString.ContainsKey(stacktrace))
            {
                _stacktrace = cachedString[stacktrace];
            }
            else
            {
                _stacktrace = stacktrace;
                cachedString.Add(_stacktrace, _stacktrace);
            }

            LogData log = new LogData();
            log.Init(condition, stacktrace, type);

            logs.Add(log);

            //UpdatelogsUI(log.Condition, log.StackTrace, log.LogType);

        }
        #endregion

        #region Logs UI
        private void UpdatelogsUI(string text, string stacktrace, LogType type)
        {
            string logText = "";
            switch (type)
            {
                case LogType.Assert:
                    logText = string.Format("\n<color=red>Assert</color> {0}\n{1}\n", text, stacktrace);
                    break;
                case LogType.Error:
                    logText = string.Format("<color=red>Error</color> {0}\n{1}\n", text, stacktrace);
                    break;
                case LogType.Exception:
                    logText = string.Format("<color=red>Exception</color> {0}\n{1}\n", text, stacktrace);
                    break;
                case LogType.Log:
                    logText = string.Format("<color=green>Log</color> {0}\n", text);
                    break;
                case LogType.Warning:
                    logText = string.Format("<color=yellow>Warning</color> {0}\n", text);
                    break;

            }
            textArea.text += logText;
        }

        private void SystemInfoLogs()
        {
            string deviceModelText = string.Format("<color=blue>DeviceModel</color> {0}\n", deviceModel);
            string deviceTypeText = string.Format("<color=blue>Device Type</color> {0}\n", deviceType);
            string deviceNameText = string.Format("<color=blue>Device Name</color> {0}\n", deviceName);
            string systemMemorySizeText = string.Format("<color=blue>System Memory Size</color> {0}\n", systemMemorySize);
            string operatingSystemText = string.Format("<color=blue>Operating System</color> {0}\n", operatingSystem);
            string currentSceneNameText = string.Format("<color=blue>Current Scene</color> {0}\n", currentSceneName);

            textArea.text += string.Format("{0}{1}{2}{3}{4}{5}\n", deviceModelText, deviceTypeText, deviceNameText, systemMemorySizeText, operatingSystemText, currentSceneNameText);
        }

        public void ToggleLogsVisibility()
        {
            if (panelLog != null)
            {
                if (panelLog.activeInHierarchy == false)
                {
                    panelLog.SetActive(true);
                    isLogShown = true;
                }
                else
                {
                    panelLog.SetActive(false);
                    isLogShown = false;
                }
            }
        }

        public void SetAlphaTransparency()
        {
            Color tempPanelLogColor = panelLog.GetComponent<Image>().color;
            Color tempScrollViewLogColor = scrollViewLog.GetComponent<Image>().color;
            tempPanelLogColor.a = sliderLog.value;
            tempScrollViewLogColor.a = sliderLog.value;
            panelLog.GetComponent<Image>().color = tempPanelLogColor;
            scrollViewLog.GetComponent<Image>().color = tempScrollViewLogColor;
        }

        public void ClearLog()
        {
            logs.Clear();
            textArea.text = "";
            System.GC.Collect();
            SystemInfoLogs();
        }

        private void LoadedSceneDelegate(Scene scene, LoadSceneMode loadSceneMode)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            if (isClearOnNewScene)
            {
                ClearLog();
            }
        }
        #endregion

        #region FPS Counter
        private float UpdateFpsCounter()
        {
            float fps = 0.00f;

            if (firstTime)
            {
                Debug.Log("first time");
                firstTime = false;
                lastUpdate = Time.realtimeSinceStartup;
                frames = 0;
                return fps;
            }

            Debug.Log("add frames calculate fps " + frames);
            frames++;
            float dt = Time.realtimeSinceStartup - lastUpdate;
            if (dt > updateInterval && frames > requiredFrames)
            {
                fps = (float)frames / dt;
                lastUpdate = Time.realtimeSinceStartup;
                frames = 0;
            }
            Debug.Log(fps);
            return fps;
        }
        #endregion

        #region Setter Getter
        public static LogManager Instance
        {
            get { return instance; }
        }

        public string DeviceModel
        {
            get { return deviceModel; }
        }

        public string DeviceType
        {
            get { return deviceType; }
        }

        public string DeviceName
        {
            get { return deviceName; }
        }

        public string SystemMemorySize
        {
            get { return systemMemorySize; }
        }

        public string OperatingSystem
        {
            get { return operatingSystem; }
        }

        public bool IsLogShown
        {
            set { isLogShown = value; }
            get { return isLogShown; }
        }

        public bool IsClearOnNewScreen
        {
            set { isClearOnNewScene = value; }
            get { return isClearOnNewScene; }
        }

        public string CurrentSceneName
        {
            get { return currentSceneName; }
        } 
        #endregion
    }
}