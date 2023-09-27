using System;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

public static class GifToJpegConverter
{
    private static string ApiEndpointFromUrl = StaticConfig.APIBase + "gifTojpegFromUrl";

    public static async UniTask<Texture2D> ConvertGifToJpegFromUrl(string gifUrl)
    {
        Texture2D texture = null;
        try
        {
            // Create the complete endpoint with the URL parameter
            string completeEndpoint = $"{ApiEndpointFromUrl}?url={Uri.EscapeDataString(gifUrl)}";

            // Create the web request and set the method and headers
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(completeEndpoint);
            request.Method = "POST";
            request.ContentLength = 0;

            // Get the response
            WebResponse response = await request.GetResponseAsync();

            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await responseStream.CopyToAsync(memoryStream);
                byte[] jpegBytes = memoryStream.ToArray();

                texture = new Texture2D(2, 2);
                texture.LoadImage(jpegBytes);
            }
        }
        catch (WebException e)
        {
            if (e.Response is HttpWebResponse errorResponse)
            {
                using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                {
                    string serverMessage = reader.ReadToEnd();
                    Debug.LogError($"Server response: {serverMessage}");
                }
            }
            Debug.LogError($"WebException: {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"IOException: {e.Message}");
        }

        return texture;
    }
}