using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace jp.go.aist3ddbclient
{
    public class ServiceRepository
    {
        private readonly string apiUrl = "https://gsrt.digiarc.aist.go.jp/3ddb_demo/api/v1/services";

        public delegate void OnDataReceived(ServicesListModel servicesList);

        public IEnumerator GetServices(OnDataReceived onDataReceived)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
            {

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResult = System.Text.Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                    ServicesListModel servicesList = JsonUtility.FromJson<ServicesListModel>(jsonResult);
                    onDataReceived?.Invoke(servicesList);
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error} : result: {webRequest.result} : code: {webRequest}");
                }
            }
        }
    }
}