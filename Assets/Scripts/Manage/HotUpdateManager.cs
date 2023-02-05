using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Manage
{
    class ABInfo
    {
        public string name;
        public long size;
        public string md5;

        public ABInfo(string name, string size, string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }

        public override bool Equals(object obj)
        {
            return (obj as ABInfo).md5 == md5;
        }

        public override int GetHashCode()
        {
            return md5.GetHashCode();
        }
    }

    public class HotUpdateManager : MonoBehaviour
    {
        private readonly string _platform =
#if UNITY_ANDROID
    "Android";
#elif UNITY_IOS
    "IOS";
#else
    "Windows";
#endif
        private readonly string _ftpIp = "ftp://8.130.45.1";
        private readonly Dictionary<string, ABInfo> _remoteInfoDict = new();
        private readonly Dictionary<string, ABInfo> _localInfoDict = new();
        private readonly List<string> _downloadList = new();
        private ModalPanel _modalPanel = null;

        private static HotUpdateManager _instance;

        public static HotUpdateManager Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject obj = new("HotUpdateManager");
                    _instance = obj.AddComponent<HotUpdateManager>();
                }
                return _instance;
            }
        }

        public bool DownloadFile(string remotePath, string localPath)
        {
            try
            {
                FtpWebRequest request = WebRequest.Create(new Uri($"{_ftpIp}/{remotePath}")) as FtpWebRequest;
                request.Credentials = new NetworkCredential("ftpuser", "edward199473");
                request.Proxy = null;
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive= true;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                Stream respStream = request.GetResponse().GetResponseStream();
                using FileStream fileStream = File.Create($"{localPath}");
                byte[] bytes = new byte[4096];
                int length = respStream.Read(bytes, 0, bytes.Length);
                while (length > 0)
                {
                    fileStream.Write(bytes, 0, length);
                    length = respStream.Read(bytes, 0, bytes.Length);
                }
                fileStream.Close();
                respStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{_ftpIp}/{remotePath}" + " 下载失败 " + ex.Message);
                return false;
            }
        }

        private async void GetRemoteInfoList(Action<bool> callback)
        {
            //print(Application.persistentDataPath);
            string localDir = $"{Application.persistentDataPath}/AssetBundles/{_platform}";
            bool isOver = false;
            for (int i = 0; !isOver || i < 3; i++)
            {
                await Task.Run(() =>
                {
                    isOver = DownloadFile($"AssetBundles/{_platform}/ABInfoList.txt", $"{localDir}/ABInfoList_TMP.txt");
                });
            }
            callback(isOver);
        }

        private void ParseInfoList(string infoList, Dictionary<string, ABInfo> abInfos)
        {
            string[] info = infoList.Split("\n");
            string[] fields;
            for (int i = 0; i < info.Length; i++)
            {
                fields = info[i].Split("\t");
                abInfos.Add(fields[0], new ABInfo(fields[0], fields[1], fields[2]));
            }
        }

        private IEnumerator LoadInfoListAsync(string filePath, Action callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(filePath);
            yield return request.SendWebRequest();
            ParseInfoList(request.downloadHandler.text, _localInfoDict);
            callback();
        }

        private void GetLocalInfoList(Action callback)
        {
            string partPath = $"/AssetBundles/{_platform}/ABInfoList.txt";
            if (File.Exists($"{Application.persistentDataPath}{partPath}"))
                StartCoroutine(LoadInfoListAsync($"file:///{Application.persistentDataPath}{partPath}", callback));
            else if (File.Exists($"{Application.streamingAssetsPath}{partPath}"))
            {
                string streamPath =
#if UNITY_ANDROID
                $"{Application.streamingAssetsPath}{partPath}";
#else
                $"file:///{Application.streamingAssetsPath}{partPath}";
#endif
                StartCoroutine(LoadInfoListAsync(streamPath, callback));
            }
        }

        private async void DownloadAssetBundles(Action<bool> callback)
        {
            string localDir = $"{Application.persistentDataPath}/AssetBundles/{_platform}";
            int downloadedNum = 0, downloadCount = _downloadList.Count;
            List<string> tempList = new();
            for (int round = 0; _downloadList.Count > 0 || round < 3; round++)
            {
                for (int index = 0; index < _downloadList.Count; index++)
                {
                    bool isOver = false;
                    await Task.Run(() =>
                    {
                        isOver = DownloadFile($"AssetBundles/{_platform}/{_downloadList[index]}", $"{localDir}/{_downloadList[index]}");
                    });
                    if (isOver)
                    {
                        _modalPanel.SetMsg("更新进度：" + ++downloadedNum + "/" + downloadCount);
                        tempList.Add(_downloadList[index]);
                    }
                }

                for (int i = 0; i < tempList.Count; i++)
                    _downloadList.Remove(tempList[i]);
            }
            callback(_downloadList.Count == 0);
        }

        public void CheckUpdate()
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/AssetBundles/{_platform}/"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/AssetBundles/{_platform}/");
            _modalPanel = UIManager.Instance.GetPanel<ModalPanel>();
            _modalPanel.Open("检查更新", "", ModalPanelType.Hint);
            GetRemoteInfoList((isOver) =>
            {
                if (!isOver)
                    EventManager.Instance.Invoke(EventId.HotUpdated, isOver);
                else
                {
                    string remoteInfoList = File.ReadAllText($"{Application.persistentDataPath}/AssetBundles/{_platform}/ABInfoList_TMP.txt");
                    ParseInfoList(remoteInfoList, _remoteInfoDict);
                    GetLocalInfoList(() =>
                    {
                        foreach (var pair in _remoteInfoDict)
                        {
                            if (!_localInfoDict.ContainsKey(pair.Key))
                                _downloadList.Add(pair.Key);
                            else
                            {
                                if (!_localInfoDict[pair.Key].Equals(_remoteInfoDict[pair.Key]))
                                    _downloadList.Add(pair.Key);
                                _localInfoDict.Remove(pair.Key);
                            }
                        }
                        foreach (var pair in _localInfoDict)
                        {
                            if (File.Exists($"{Application.persistentDataPath}/AssetBundles/{_platform}/" + pair.Key))
                                File.Delete($"{Application.persistentDataPath}/AssetBundles/{_platform}/" + pair.Key);
                        }
                        DownloadAssetBundles((isOver) =>
                        {
                            if (isOver)
                            {
                                _modalPanel.SetMsg("更新完成");
                                File.WriteAllText($"{Application.persistentDataPath}/AssetBundles/{_platform}/ABInfoList.txt", remoteInfoList);
                            }
                            EventManager.Instance.Invoke(EventId.HotUpdated, isOver);
                        });
                    });
                }
            });
        }
    }
}
