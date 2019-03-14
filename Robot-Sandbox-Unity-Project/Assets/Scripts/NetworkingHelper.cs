using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

// Asynchronous programming in .NET development
using System.Net.Http;
using System.Threading.Tasks;

public class NetworkingHelper : MonoBehaviour {
    // instantiate HttpClient for life time
    private static readonly HttpClient client = new HttpClient();

    // ---------------------------------------------------------------------
    // Communication with Server
    // ---------------------------------------------------------------------

    // ----- Using Unity's Networking Module -----

    // grants access to post requests on specified ip, the server is a simple http server running on python3.6
    public IEnumerator UploadHTTP(String message) {
        String address = "http://192.168.0.154:3000";
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection(message));

        UnityWebRequest www = UnityWebRequest.Post(address, formData);

        yield return www.Send();

        // Debug for making sure the method has been called
        //if (www.isHttpError) {
        //    Debug.Log("POST Error");
        //} else {
        //    Debug.Log("finished req");
        //}
    }

    // ----- Using System's .Net HTTP Module -----
    // reference: https://stackoverflow.com/questions/4015324/how-to-make-http-post-web-request

    // 6 strings corresponding to 6 data output each iteration
    public async void MainAsync(
        string string1,
        string string2,
        string string3,
        string string4,
        string string5,
        string string6) {

        // ----- Non-JSON format Post Request -----

        // using Dictionary for standard non-json http request content
        var values = new Dictionary<string, string> {
            {"1", string1},
            {"2", string2},
            {"3", string3},
            {"4", string4},
            {"5", string5},
            {"6", string6}
        };
        // encode the content into the standard format for http request
        var content = new FormUrlEncodedContent(values);

        // using Key-Value Pair for standard non-json http request content
        var content2 = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("1", string1),
            new KeyValuePair<string, string>("2", string2),
            new KeyValuePair<string, string>("3", string3),
            new KeyValuePair<string, string>("4", string4),
            new KeyValuePair<string, string>("5", string5),
            new KeyValuePair<string, string>("6", string6)
        });

        // create the post request
        var result = await client.PostAsync("http://192.168.0.154:3000", content);
        // using `await` to force the current thread to wait until the asynchronous operation has completed.
        string resultContent = await result.Content.ReadAsStringAsync();
        Console.WriteLine(resultContent);
    }
}