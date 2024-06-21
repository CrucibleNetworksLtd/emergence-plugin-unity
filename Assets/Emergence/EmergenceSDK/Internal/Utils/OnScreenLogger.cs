using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EmergenceSDK.Internal.Utils
{
    public class OnScreenLogger : SingletonComponent<OnScreenLogger>
    {
        uint qsize = 15;  // number of messages to keep
        Queue<(string Text, GUIStyle Style)> myLogQueue = new Queue<(string Text, GUIStyle Style)>();
        GUIStyle logStyle;
        GUIStyle callingClassStyle;

        void Start()
        {
            // Initialise log string style
            logStyle = new GUIStyle();
            logStyle.fontSize = 24;
            logStyle.normal.textColor = Color.black;
            logStyle.border = new RectOffset(1, 1, 1, 1);
            logStyle.normal.background = MakeTex(2, 2, Color.white);

            // Initialise calling class style
            callingClassStyle = new GUIStyle();
            callingClassStyle.fontSize = 14;
            callingClassStyle.normal.textColor = Color.gray;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public void HandleLog(string logString, string callingClass, EmergenceLogger.LogLevel type) 
        {
            myLogQueue.Enqueue((Text: "[" + type + "] : " + logString, Style: logStyle));
            myLogQueue.Enqueue((Text: callingClass, Style: callingClassStyle));
            RemoveLogAfterDelay().Forget();

            while (myLogQueue.Count > qsize * 2) // Each log entry now takes 2 slots in the queue
                myLogQueue.Dequeue();
        }

        private async UniTask RemoveLogAfterDelay()
        {
            await UniTask.Delay(10000); // Delay for 10 seconds
            if (myLogQueue.Count >= 2)
            {
                myLogQueue.Dequeue(); // Remove log string
                myLogQueue.Dequeue(); // Remove calling class
            }
        }

        void OnGUI() 
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
            foreach (var logEntry in myLogQueue)
            {
                GUILayout.Label(logEntry.Text, logEntry.Style);
            }
            GUILayout.EndArea();
        }
    }
}
