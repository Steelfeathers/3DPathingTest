using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnknownWorldsTest
{
    public static class Utils 
    {
        public static void QuitApplication()
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }
        
        public static T FromJson<T>(this string str)
        {
            T retVal;
            try
            {
                retVal = JsonUtility.FromJson<T>(str); 
                JsonUtility.ToJson(str);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default(T);
            }

            return retVal;
        }

        public static bool SaveToJson(object data, string filePath)
        {
            try
            {
                string str = JsonUtility.ToJson(data);
                System.IO.File.WriteAllText(filePath, str);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }
        
        public static string LoadTextAsset(string filePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(filePath);
            return asset == null ? null : asset.text;
        }
        
    }
}
