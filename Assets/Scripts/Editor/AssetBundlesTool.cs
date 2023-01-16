using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetBundlesTool : EditorWindow
{
    private string _ftpIp = "ftp://8.130.45.1";
    private int _index = 0;
    private readonly string[] _platforms = { "Windows", "Android", "IOS" };

    [MenuItem("Tools/AssetBundlesTool")]
    private static void OpenWindow()
    {
        AssetBundlesTool win = GetWindowWithRect(typeof(AssetBundlesTool), new Rect(0, 0, 400, 230)) as AssetBundlesTool;
        win.Show();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 120, 30), "平台选择");
        _index = GUI.Toolbar(new Rect(150, 20, 230, 30), _index, _platforms);
        GUI.Label(new Rect(20, 60, 120, 30), "远端地址");
        _ftpIp = GUI.TextField(new Rect(150, 60, 230, 30), _ftpIp);
        if (GUI.Button(new Rect(20, 100, 360, 30), "创建对比文件"))
            CreateABInfoList();
        if (GUI.Button(new Rect(20, 140, 360, 30), "保存到流资源"))
            MoveToStreamingAssets();
        if (GUI.Button(new Rect(20, 180, 360, 30), "上传热更资源"))
            UploadAssetBundles();
    }

    private string GetMD5(string filePath)
    {
        using FileStream stream = new(filePath, FileMode.Open);
        byte[] md5 = new MD5CryptoServiceProvider().ComputeHash(stream);
        stream.Close();
        StringBuilder sb = new();
        for (int i = 0; i < md5.Length; i++)
            sb.Append(md5[i].ToString("x2"));
        return sb.ToString();
    }

    private void CreateABInfoList()
    {
        DirectoryInfo directory = Directory.CreateDirectory($"{Application.dataPath}/AssetBundles/{_platforms[_index]}");
        FileInfo[] files = directory.GetFiles();
        string abInfoList = "";
        foreach (FileInfo file in files)
            if (file.Extension.Equals(""))
                abInfoList += file.Name + "\t" + file.Length + "\t" + GetMD5(file.FullName) + "\n";
        abInfoList = abInfoList[..^1];
        File.WriteAllText($"{Application.dataPath}/AssetBundles/{_platforms[_index]}/ABInfoList.txt", abInfoList);
        AssetDatabase.Refresh();
    }

    private void MoveToStreamingAssets()
    {
        UnityEngine.Object[] selectAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        if (selectAssets.Length > 0)
        {
            string abInfoList = "";
            foreach (UnityEngine.Object asset in selectAssets)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                string fileName = assetPath[assetPath.LastIndexOf('/')..];
                if (fileName.IndexOf('.') == -1)
                {
                    AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets/AssetBundles/" + _platforms[_index] + fileName);
                    FileInfo file = new($"{Application.streamingAssetsPath}/AssetBundles/{_platforms[_index]}{fileName}");
                    abInfoList += file.Name + "\t" + file.Length + "\t" + GetMD5(file.FullName) + "\n";
                }
            }
            abInfoList = abInfoList[..^1];
            File.WriteAllText($"{Application.streamingAssetsPath}/AssetBundles/{_platforms[_index]}/ABInfoList.txt", abInfoList);
            AssetDatabase.Refresh();
        }
    }

    private async void FtpUploadFile(string filePath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest request = WebRequest.Create(new Uri($"{_ftpIp}/AssetBundles/{_platforms[_index]}/{fileName}")) as FtpWebRequest;
                request.Credentials = new NetworkCredential("ftpuser", "edward199473");
                request.Proxy = null;
                request.KeepAlive = false;
                request.UseBinary = true;
                request.UsePassive = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                Stream reqStream = request.GetRequestStream();
                using FileStream fileStream = File.OpenRead(filePath);
                byte[] bytes = new byte[4096];
                int length = fileStream.Read(bytes, 0, bytes.Length);
                while (length > 0)
                {
                    reqStream.Write(bytes, 0, length);
                    length = fileStream.Read(bytes, 0, bytes.Length);
                }
                fileStream.Close();
                reqStream.Close();
                Debug.Log(fileName + " 上传成功");
            }
            catch (Exception ex)
            {
                Debug.LogError(fileName + " 上传失败 " + ex.Message);
            }
        });
    }

    private void UploadAssetBundles()
    {
        DirectoryInfo directory = Directory.CreateDirectory($"{Application.dataPath}/AssetBundles/{_platforms[_index]}/");
        FileInfo[] files = directory.GetFiles();
        foreach (FileInfo file in files)
            if (file.Extension.Equals("") || file.Extension.Equals(".txt"))
                FtpUploadFile(file.FullName, file.Name);
    }
}
