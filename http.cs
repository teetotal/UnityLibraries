using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class Http {
    public delegate void Callback(string uri, string response);
    private static readonly Lazy<Http> hInstance =
        new Lazy<Http>(() => new Http());
 
    public static Http Instance
    {
        get {
            return hInstance.Value;
        } 
    }

    protected Http()
    {
    }
    
    public IEnumerator GetRequest(string uri, Callback cb)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                if(cb != null)
                    cb(pages[page].Split('?')[0], webRequest.downloadHandler.text);
            }
        }
    }
    public IEnumerator PostRequest(string uri, string data, Callback cb)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(uri, bytes))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.method = UnityWebRequest.kHttpVerbPOST;
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                if(cb != null)
                    cb(pages[page].Split('?')[0], webRequest.downloadHandler.text);
            }
        }
    }
    
}