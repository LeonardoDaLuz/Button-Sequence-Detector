using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LeoLuz.Utilities {
//[CreateAssetMenu(fileName = "NewDynamicInspectorTheme", menuName = "Dynamic Inspector Theme", order = 1)]
public class ComboMasterInterfaceConfig : ScriptableObject
{
    public DepthTheme[] CustomStyles;
    [System.Serializable]
    public class DepthTheme {
        public string Name;

        [Header("Icons")]
        public Texture2D MinimizeIcon;
        public Texture2D MaximizeIcon;

        [Header("Styles")]
        public GUIStyle WindowClosedHeaderStyle;
        public GUIStyle WindowOpennedHeaderStyle;
        public GUIStyle HeaderLabelStyle;
        public GUIStyle WindowInnerStyle;

        public DepthTheme Clone() {
            DepthTheme clone = new DepthTheme();
            clone = new DepthTheme();
            clone.WindowClosedHeaderStyle = new GUIStyle(WindowClosedHeaderStyle);
            clone.WindowOpennedHeaderStyle = new GUIStyle(WindowOpennedHeaderStyle);
            clone.WindowInnerStyle = new GUIStyle(WindowInnerStyle);
            return clone;
        }

        public static DepthTheme[] CloneAll(DepthTheme[] all)
        {
            DepthTheme[] clone = new DepthTheme[all.Length];
            for (int i = 0; i < all.Length; i++) {
                clone[i] = all[i].Clone();
            }
            return clone;
        }
    }

    public DepthTheme GetCustomStyle(string name)
    {
        for (int i = 0; i < CustomStyles.Length; i++)
        {
            if (CustomStyles[i].Name == name)
                return CustomStyles[i];
        }
        return CustomStyles.Length > 0 ? CustomStyles[0] : null;
    }
}

}