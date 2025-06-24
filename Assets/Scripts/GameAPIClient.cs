using UnityEngine;
using System;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class GameAPIClient : MonoBehaviour
{
    private static GameAPIClient instance;
    public static GameAPIClient Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameAPIClient");
                instance = go.AddComponent<GameAPIClient>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private readonly string baseUrl = "http://localhost:8999/api/game";

    [Serializable]
    public class GameData
    {
        public int playerFinalMoney;
        public int roundsToWin;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<string> PostDataAsync(string endpoint, object data)
    {
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/{endpoint}", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                throw new Exception(request.error);
            }
        }
    }
}