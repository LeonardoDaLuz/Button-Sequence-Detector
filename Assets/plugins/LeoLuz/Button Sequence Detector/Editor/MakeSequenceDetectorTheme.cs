using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LeoLuz.Utilities;

namespace LeoLuz.ButtonSequenceDetector
{
    public class MakeSequenceDetectorTheme : MonoBehaviour
    {

        //	[MenuItem("Assets/Create/Beaty Inspector Theme")]
        public static void CreateMyAsset()
        {
            Object obj = Resources.Load("DynamicInspectorTheme");
            bool makeRandomName = false;
            if (obj != null)
            {
                makeRandomName = true;
            }
            ComboMasterInterfaceConfig asset = ScriptableObject.CreateInstance<ComboMasterInterfaceConfig>();

            string folder = FileUtility.GetSelectionFolder();
            if (FileUtility.CheckIfItIsSelectionFolder(folder))
            {
                AssetDatabase.CreateAsset(asset, folder + "/NewMakeSequenceDetectorTheme" + (makeRandomName ? Random.Range(0, 9999).ToString() : "") + ".asset");
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "Please, create this file in resources folder", "Ok");
                Debug.Log("Please, put this file in a resources folder");
            }
        }
    }
}