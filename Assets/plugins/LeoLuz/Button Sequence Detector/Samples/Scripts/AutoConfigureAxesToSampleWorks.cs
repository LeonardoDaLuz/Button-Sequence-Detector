using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class AutoConfigureAxesToSampleWorks : MonoBehaviour {

#if UNITY_EDITOR
    // Use this for initialization
    void Awake () {
        //Debug.Log("Checking Axes..");
        AutoconfigureAxisToSampleWorks();
    }
	
    static void AutoconfigureAxisToSampleWorks()
    {
        var loadedScene = EditorSceneManager.GetActiveScene().name;

        //if (loadedScene != "FighterDemoWithVirtualJoystick" && loadedScene != "FighterDemo")
        //    return;

        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

        if (inputManager == null)
            return;

        SerializedObject obj = new SerializedObject(inputManager);

        SerializedProperty axisArray = obj.FindProperty("m_Axes");

        if (axisArray.arraySize == 0)
            UnityEngine.Debug.Log("No Axes");

        //Find buttons and configure it
        var punchProp = findFProperty("Fire3", axisArray);
        var horizontalProp = findProperty("Horizontal", axisArray);
        var VerticalProp = findProperty("Vertical", axisArray);

        if (punchProp == null)
        {
            axisArray.arraySize = axisArray.arraySize + 3;
            obj.ApplyModifiedProperties();
            obj.Update();

            punchProp = axisArray.GetArrayElementAtIndex(axisArray.arraySize - 3);
            punchProp.FindPropertyRelative("m_Name").stringValue = "Fire3";
            punchProp.FindPropertyRelative("positiveButton").stringValue = "joystick 1 button 0";
            obj.ApplyModifiedProperties();
            punchProp = axisArray.GetArrayElementAtIndex(axisArray.arraySize - 2);
            punchProp.FindPropertyRelative("m_Name").stringValue = "Fire3";
            punchProp.FindPropertyRelative("positiveButton").stringValue = "f";
            obj.ApplyModifiedProperties();
            punchProp = axisArray.GetArrayElementAtIndex(axisArray.arraySize - 1);
            punchProp.FindPropertyRelative("m_Name").stringValue = "Fire3 p2";
            punchProp.FindPropertyRelative("positiveButton").stringValue = "joystick 2 button 0";
            obj.ApplyModifiedProperties();
            obj.Update();
            Debug.Log("Fire3 axis was added to sample works");
        }

        //find Horizontal and configure it
        if (horizontalProp == null)
        {
            axisArray.arraySize = axisArray.arraySize + 1;
            obj.ApplyModifiedProperties();
            obj.Update();
            horizontalProp = axisArray.GetArrayElementAtIndex(axisArray.arraySize - 1);
            horizontalProp.FindPropertyRelative("m_Name").stringValue = "Horizontal";
            horizontalProp.FindPropertyRelative("positiveButton").stringValue = "right";
            horizontalProp.FindPropertyRelative("negativeButton").stringValue = "left";
            obj.ApplyModifiedProperties();
            obj.Update();

            Debug.Log("Horizontal axis was added to sample works");
        }

        if (VerticalProp == null)
        {
            axisArray.arraySize = axisArray.arraySize + 1;
            obj.ApplyModifiedProperties();
            obj.Update();
            VerticalProp = axisArray.GetArrayElementAtIndex(axisArray.arraySize - 1);
            VerticalProp.FindPropertyRelative("m_Name").stringValue = "Vertical";
            VerticalProp.FindPropertyRelative("positiveButton").stringValue = "up";
            VerticalProp.FindPropertyRelative("negativeButton").stringValue = "down";
            obj.ApplyModifiedProperties();
            obj.Update();

            Debug.Log("Vertical axis was added to sample works");
        }
    }
    private static SerializedProperty findProperty(string element, SerializedProperty axisArray)
    {
        //Find Punch
        for (int i = 0; i < axisArray.arraySize; ++i)
        {
            var axis = axisArray.GetArrayElementAtIndex(i);
            var _elementName = axis.FindPropertyRelative("m_Name").stringValue;
            if (_elementName == element)
                return axis;
        }
        return null;
    }

    private static SerializedProperty findFProperty(string element, SerializedProperty axisArray)
    {
        //Find Punch
        for (int i = 0; i < axisArray.arraySize; ++i)
        {
            var axis = axisArray.GetArrayElementAtIndex(i);
            var _elementName = axis.FindPropertyRelative("m_Name").stringValue;
            if (_elementName == element && axis.FindPropertyRelative("positiveButton").stringValue == "f")
            {
                return axis;
            }
        }
        return null;
    }
#endif
}
