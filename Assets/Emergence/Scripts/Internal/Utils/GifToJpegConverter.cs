using System;
using System.Net;
using System.IO;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Internal.Utils;
using UnityEngine;

public static class GifToJpegConverter
{
    private static string ApiEndpoint = $"{StaticConfig.APIBase}gifTojpeg";

    public static async UniTask<Texture2D> ConvertGifToJpegFromUrl(byte[] gifData)
    {
        Texture2D texture = null;

        try
        {
            // Create a boundary string for the multipart/form-data request
            string boundary = "EmergenceBoundary";

            // Create the request and set the method, content type, and other headers
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiEndpoint);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Accept = "*/*";  // Set the Accept header here
            request.Expect = "";

            // Create the multipart content
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Write the beginning boundary
                byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                memoryStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                // Write the header for the file part
                string header = "Content-Disposition: form-data; name=\"file\"; filename=\"file.gif\"\r\nContent-Type: image/gif\r\n\r\n";
                byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                memoryStream.Write(headerBytes, 0, headerBytes.Length);

                // Write the file bytes
                memoryStream.Write(gifData, 0, gifData.Length);

                // Write the ending boundary
                boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                memoryStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                // Convert the MemoryStream to a byte array
                byte[] requestData = memoryStream.ToArray();

                // Set the ContentLength and write the data to the request stream
                request.ContentLength = requestData.Length;

                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }
            }

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
            Debug.LogError($"WebException: {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"IOException: {e.Message}");
        }

        return texture;
    }
    
    public static async UniTask<Texture2D> ConvertGifToJpegFromUrl(string gifUrl)
    {
        Texture2D texture = null;

        try
        {
            // Download the GIF to a byte array
            byte[] gifData;
            using (WebClient webClient = new WebClient())
            {
                gifData = await webClient.DownloadDataTaskAsync(gifUrl);
            }

            // Create a boundary string for the multipart/form-data request
            string boundary = "EmergenceBoundary";

            // Create the request and set the method, content type, and other headers
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiEndpoint);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Accept = "*/*";  // Set the Accept header here
            request.Expect = "";

            // Create the multipart content
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Write the beginning boundary
                byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                memoryStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                // Write the header for the file part
                string header = "Content-Disposition: form-data; name=\"file\"; filename=\"file.gif\"\r\nContent-Type: image/gif\r\n\r\n";
                byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
                memoryStream.Write(headerBytes, 0, headerBytes.Length);

                // Write the file bytes
                memoryStream.Write(gifData, 0, gifData.Length);

                // Write the ending boundary
                boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                memoryStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                // Convert the MemoryStream to a byte array
                byte[] requestData = memoryStream.ToArray();

                // Set the ContentLength and write the data to the request stream
                request.ContentLength = requestData.Length;

                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    stream.Write(requestData, 0, requestData.Length);
                }
            }

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
            Debug.LogError($"WebException: {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"IOException: {e.Message}");
        }

        return texture;
    }
}
