using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using LeoLuz.Extensions;
using LeoLuz.Utilities;

namespace LeoLuz.ButtonSequenceDetector
{
    [CustomEditor(typeof(CommandSequences))]
    public class CommandSequencesEditor : Editor
    {
        SerializedObject serializedObj;
        SerializedProperty configProp;
        SerializedProperty InputSequencesProp;
        SerializedProperty[] InputSequenceElements;
        ComboMasterInterfaceConfig InterfaceThemeConfig;
        public string ThemeFileName;
        public int CurrentInspectedIndex;
        public int _DamageTypesCurrentInspectedIndex = -1;
        public bool started;
        CommandSequences comboMaster;
        public bool scriptOrderApplied;
        bool useDarkTheme;

        //Navigation variables
        public override void OnInspectorGUI()
        {
            comboMaster = (CommandSequences)target;
            Draw();
        }
        public void Draw()
        {
            serializedObj.Update();

            if (InterfaceThemeConfig == null)
                InterfaceThemeConfig = FileUtility.LoadFile("ComboMasterTheme") as ComboMasterInterfaceConfig;

            if (serializedObj == null)
                ReloadSerializedProperties();

            InspectorBeautifierExtensions.ThemeFileName = "ComboMasterTheme";

            if (comboMaster.InputSequences == null || comboMaster.InputSequences.Length != InputSequenceElements.Length || comboMaster.InputSequences.Length != InputSequencesProp.arraySize)
            {
                ReloadSerializedProperties();
            }
            if (Math.Round(EditorStyles.label.normal.textColor.r, 2) == 0.7)
            {
                useDarkTheme = true;
            }
            else
            {
                useDarkTheme = false;
            }


            DGUI.Space(0f);
            if (configProp.BeginWrapperWindow(32f, useDarkTheme ? "Dark" : "Grey", "GearIcon"))
            {
                configProp.FindPropertyRelative("DigitalHorizontal").Draw();
                configProp.FindPropertyRelative("DigitalVertical").Draw();
                configProp.FindPropertyRelative("inversionDetection").Draw();
                if (comboMaster.config.inversionDetection == CommandSequences.Config.InversionDetection.Manual)
                {

                    serializedObj.FindProperty("FacingRight").Draw();
                    serializedObj.FindProperty("FacingUp").Draw();
                }
                configProp.FindPropertyRelative("Player").Draw();
                configProp.FindPropertyRelative("Debug").Draw();
            }
            configProp.EndWrapperWindow();
            //Economic interface mode
            ComboMasterInterfaceConfig.DepthTheme theme = InterfaceThemeConfig.GetCustomStyle(useDarkTheme ? "Dark" : "Grey");
            Texture2D SequenceIcon = (Texture2D)FileUtility.LoadTexture("SequenceIcon");
            GUIStyle BoldLabel = GUIStyle.none;
            BoldLabel.fontStyle = FontStyle.Bold;
            BoldLabel.fontSize = 12;
            DGUI.Space(0f);
            for (int i = 0; comboMaster.InputSequences != null && i < comboMaster.InputSequences.Length; i++)
            {
                GUILayout.BeginVertical(InputSequenceElements[i].isExpanded ? theme.WindowOpennedHeaderStyle : theme.WindowClosedHeaderStyle);

                Rect BlockRect = EditorGUILayout.GetControlRect(false, 32f);
                float fullwidth = BlockRect.width;
                BlockRect.x -= 12f;
                BlockRect.width += 12f;
                //Header block style:
                #region Header visible block
                GUI.Box(BlockRect, SequenceIcon, theme.HeaderLabelStyle);
                //label.x -= LabelXOffset;
                #endregion

                #region clicable button
                BlockRect.width = 32f;
                if (GUI.Button(BlockRect, "", GUIStyle.none))
                {
                    InputSequenceElements[i].isExpanded = !InputSequenceElements[i].isExpanded;
                }
                #endregion

                #region Sandwich
                if (InputSequenceElements[i].isExpanded)
                {
                    //BlockRect.width = fullwidth;

                    //GUIStyle sandwichStyle = new GUIStyle();
                    //sandwichStyle.alignment = TextAnchor.MiddleRight;
                    //GUI.Box(BlockRect, new GUIContent(theme.MinimizeIcon), sandwichStyle);
                    DrawMoveOrderingOptions(i, comboMaster, comboMaster.InputSequences[i], -13f, 13f);
                }
                else
                {
                    DrawMoveOrderingOptions(i, comboMaster, comboMaster.InputSequences[i], -13f, 13f);
                }
                #endregion

                #region Editable Label
                BlockRect.width = fullwidth - 36f;
                BlockRect.xMin += 34f;
                //  BlockRect.yMin += 10f;
                if (i >= comboMaster.InputSequences.Length)
                    return;
                if (comboMaster.InputSequences[i].Name == "")
                    comboMaster.InputSequences[i].Name = "New Sequence";
                comboMaster.InputSequences[i].Name = EditorGUI.TextField(BlockRect, comboMaster.InputSequences[i].Name, theme.HeaderLabelStyle);
                #endregion

                #region Sequence Body
                if (InputSequenceElements[i].isExpanded)
                {
                    EditorGUILayout.BeginVertical(theme.WindowInnerStyle);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10f);
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(10f);
                    comboMaster.InputSequences[i].AutofitConnections();
                    DrawCommandSteps(InputSequenceElements[i].FindPropertyRelative("commandList"), ref comboMaster.InputSequences[i].commandList);
                }

                if (InputSequenceElements[i].isExpanded)
                {
                    GUILayout.Space(10f);
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10f);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
                DGUI.Space(0f);
                #endregion

            }
            InputSequencesProp.ArrayIncreaseButton();
            DGUI.Space(0f);
            if (GUI.changed)
            {
                serializedObj.ApplyModifiedProperties();
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }




        }
        void DrawMoveElement(SerializedProperty currentMoveProp, CommandSequences.InputSequence currentMove, CommandSequences comboMaster)
        {
            //FIRST 4 PROPERTYS
            currentMoveProp.DrawChildrenLight("", "StartupType");


        }
        float lastSave;
        void DrawMoveOrderingOptions(int i, CommandSequences comboMaster, CommandSequences.InputSequence currentMove, float XOffset = 0f, float YOffset = 0f)
        {
            //ORDERING
            Rect optionsRect = EditorGUILayout.GetControlRect(false, 1f);
            optionsRect.x += optionsRect.width - 35f + XOffset;
            optionsRect.y -= 42f - YOffset;
            optionsRect.height = 12f;
            optionsRect.width = 22f;
            if (i == 0f)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            if (GUI.Button(optionsRect, "▲"))
            {
                comboMaster.InputSequences = LeoLuz.Utilities.ArrayUtility.Remove(comboMaster.InputSequences, currentMove);
                comboMaster.InputSequences = LeoLuz.Utilities.ArrayUtility.Insert(comboMaster.InputSequences, currentMove, i - 1);
                //comboMaster.Autofit();
                ReloadSerializedProperties();
            }
            optionsRect.y += 12f;
            if (i > comboMaster.InputSequences.Length - 2)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            if (GUI.Button(optionsRect, "▼"))
            {
                comboMaster.InputSequences = LeoLuz.Utilities.ArrayUtility.Remove(comboMaster.InputSequences, currentMove);
                comboMaster.InputSequences = LeoLuz.Utilities.ArrayUtility.Insert(comboMaster.InputSequences, currentMove, i + 1);
                //comboMaster.Autofit();
                ReloadSerializedProperties();
            }
            GUI.enabled = true;
            optionsRect.x += 22f;
            optionsRect.y -= 12f;
            optionsRect.height = 24f;
            optionsRect.width = 22f;
            if (GUI.Button(optionsRect, "X"))
            {
                comboMaster.InputSequences = LeoLuz.Utilities.ArrayUtility.Remove(comboMaster.InputSequences, currentMove);
                //comboMaster.Autofit();
                ReloadSerializedProperties();

            }
        }
        public void DrawMoveOrderingOptions<T>(int i, CommandSequences comboMaster, ref T[] MovesAndTechniques, T currentMove, float XOffset = 0f, float YOffset = 0f)
        {
            //ORDERING
            Rect optionsRect = EditorGUILayout.GetControlRect(false, 1f);
            optionsRect.x += optionsRect.width - 35f + XOffset;
            optionsRect.y -= 42f - YOffset;
            optionsRect.height = 12f;
            optionsRect.width = 22f;
            if (i == 0f)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            if (GUI.Button(optionsRect, "▲"))
            {
                MovesAndTechniques = LeoLuz.Utilities.ArrayUtility.Remove(MovesAndTechniques, currentMove);
                MovesAndTechniques = LeoLuz.Utilities.ArrayUtility.Insert(MovesAndTechniques, currentMove, i - 1);
                // comboMaster.Autofit();
                ReloadSerializedProperties();
            } 
            optionsRect.y += 12f;
            if (i > MovesAndTechniques.Length - 2)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            if (GUI.Button(optionsRect, "▼"))
            {
                LeoLuz.Utilities.ArrayUtility.Remove(ref MovesAndTechniques, currentMove);
                MovesAndTechniques = LeoLuz.Utilities.ArrayUtility.Insert(MovesAndTechniques, currentMove, i + 1);
                // comboMaster.Autofit();
                ReloadSerializedProperties();
            }
            GUI.enabled = true;
            optionsRect.x += 22f;
            optionsRect.y -= 12f;
            optionsRect.height = 24f;
            optionsRect.width = 22f;
            if (GUI.Button(optionsRect, "X"))
            {
                LeoLuz.Utilities.ArrayUtility.Remove(ref MovesAndTechniques, currentMove);
                // comboMaster.Autofit();
                ReloadSerializedProperties();

            }
        }
        void OnEnable()
        {
            if (serializedObj == null)
                ReloadSerializedProperties();
        }
        void ReloadSerializedProperties()
        {
            if (target == null)
                return;

            if (!scriptOrderApplied)
            {
                // First you get the MonoScript of your MonoBehaviour
                MonoScript monoScript = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
                int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
                if (currentExecutionOrder >= 0)
                {
                    MonoImporter.SetExecutionOrder(monoScript, -9998);
                }
                scriptOrderApplied = true;
            }
            //UnityEngine.Debug.Log("ReloadSerializedProperties");

            serializedObj = new SerializedObject(target);
            //serializedObj.Update();
            configProp = serializedObj.FindProperty("config");
            InputSequencesProp = serializedObj.FindProperty("InputSequences");
            InputSequenceElements = new SerializedProperty[InputSequencesProp.arraySize];
            for (int i = 0; i < InputSequencesProp.arraySize; i++)
            {
                InputSequenceElements[i] = InputSequencesProp.GetArrayElementAtIndex(i);
            }
        }

        public void DrawCommandSteps(SerializedProperty prop, ref CommandSequences.InputSequence.commandStep[] _commandSteps, bool withoutNext = false)
        {
            if (_commandSteps == null || _commandSteps.Length == 0)
            {
                DGUI.BeginHorizontalCenter();
                Rect newStep = GUILayoutUtility.GetRect(32f, 32f);
                newStep.x -= 16f;
                if (GUI.Button(newStep, "Add"))
                {
                    prop.arraySize += 1;
                }
                DGUI.EndHorizontalCenter();
            }
            else
            {
                #region Getting axes and buttons name
                object[] axesPackage = LeoLuz.Utilities.InputUtility.GetAxes();

                var Axislist = (List<InputUtility.Axis>)axesPackage[1];
                var AxisLabelList = (List<string>)axesPackage[0];

                Regex r = new Regex(@"^.+ p[0123456789]|1[012]$");

                for (int i = 0; i < AxisLabelList.Count; i++)
                {
                    if (r.IsMatch(AxisLabelList[i]))
                    {
                        AxisLabelList.RemoveAt(i);
                        Axislist.RemoveAt(i);
                        i--;
                    }
                }

                AxisLabelList.Insert(0, "↗");
                AxisLabelList.Insert(0, "↑");
                AxisLabelList.Insert(0, "↖");
                AxisLabelList.Insert(0, "←");
                AxisLabelList.Insert(0, "↙");
                AxisLabelList.Insert(0, "↓");
                AxisLabelList.Insert(0, "↘");
                AxisLabelList.Insert(0, "→");


                #endregion
                //DebugUtility.PrintDatabase(Axislist);
                for (int i = 0; i < _commandSteps.Length; i++)
                {
                    #region Begin horizontal center
                    if (i == 0 || (_commandSteps[i - 1]._Operator == CommandSequences.InputSequence.commandStep.Operator.Next))
                    {
                        GUILayout.Space(5f);
                        DGUI.BeginHorizontalCenter();
                    }
                    #endregion
                    #region Get Choice Index

                    var choiceIndex = AxisLabelList.IndexOf(_commandSteps[i].RealAxisButtonName);

                    #endregion
                    #region Allocate element
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginVertical();
                    Rect slot = GUILayoutUtility.GetRect(90f, choiceIndex < 8 ? 68f : _commandSteps[i].inputEvent == CommandSequences.InputSequence.commandStep.InputEvent.Axis ? 99f : Axislist[choiceIndex - 8].isAxis ? 83f : 52f);

                    #region Window Wrapper
                    Rect window = new Rect(slot);
                    window.x -= 4f;
                    window.width += 8f;
                    GUI.Box(window, " ");
                    window.x += window.width - 16f;
                    window.width = 16f;
                    window.height = 16f;
                    if (GUI.Button(window, "X", GUI.skin.label))
                    {
                        if (i > 0 && _commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.Next)
                        {
                            _commandSteps[i - 1]._Operator = CommandSequences.InputSequence.commandStep.Operator.Next;
                        }
                        // _commandSteps = ArrayUtility.Remove(_commandSteps, _commandSteps[i]);
                        prop.DeleteArrayElementAtIndex(i);
                    }
                    #endregion
                    slot.y += 16f;

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5f);
                    #endregion
                    #region Draw Elements

                    slot.height = 16f;
                    choiceIndex = EditorGUI.Popup(slot, choiceIndex, AxisLabelList.ToArray());
                    slot.y += 16f;
                    if (choiceIndex < 0)
                        choiceIndex = 0;

                    if (i > _commandSteps.Length - 1)
                    {
                        DGUI.EndHorizontalCenter();
                        continue;
                    }

                    _commandSteps[i].RealAxisButtonName = AxisLabelList[choiceIndex];
                    if (choiceIndex < 8)
                    {
                        _commandSteps[i].IsAbstractedDiretional = true;
                    }
                    else
                    {
                        _commandSteps[i].IsAbstractedDiretional = false;
                    }

                    string[] minus = new string[1] { "Axis" };

                    if (choiceIndex < 8 || !Axislist[choiceIndex - 8].isAxis)
                    {
                        _commandSteps[i].inputEvent = (CommandSequences.InputSequence.commandStep.InputEvent)DGUI.EnumPopup(slot, _commandSteps[i].inputEvent, minus);
                    }
                    else
                    {
                        _commandSteps[i].inputEvent = (CommandSequences.InputSequence.commandStep.InputEvent)EditorGUI.EnumPopup(slot, _commandSteps[i].inputEvent);
                    }
                    if (choiceIndex > 7)
                    {
                        if (_commandSteps[i].inputEvent == CommandSequences.InputSequence.commandStep.InputEvent.Axis)
                        {
                            slot.y += 16f;
                            _commandSteps[i].Statement = (CommandSequences.InputSequence.commandStep.GreaterEqualLower)EditorGUI.EnumPopup(slot, _commandSteps[i].Statement);
                            slot.y += 16f;
                            _commandSteps[i].value = EditorGUI.FloatField(slot, _commandSteps[i].value);
                        }
                        else
                        {
                            if (Axislist[choiceIndex - 8].isAxis)
                            {
                                _commandSteps[i].IsAxis = true;
                                slot.y += 16f;
                                _commandSteps[i].axisDirection = (CommandSequences.InputSequence.commandStep.AxisDirection)EditorGUI.EnumPopup(slot, _commandSteps[i].axisDirection);
                                slot.y += 16f;
                                _commandSteps[i].axisInversion = (CommandSequences.InputSequence.commandStep.AxisInversion)EditorGUI.EnumPopup(slot, _commandSteps[i].axisInversion);
                            }
                            else
                            {
                                _commandSteps[i].IsAxis = false;
                                _commandSteps[i].axisInversion = CommandSequences.InputSequence.commandStep.AxisInversion.NoInvert;
                            }
                        }
                    }
                    else
                    {
                        slot.y += 16f;
                        _commandSteps[i].axisInversion = (CommandSequences.InputSequence.commandStep.AxisInversion)EditorGUI.EnumPopup(slot, _commandSteps[i].axisInversion);
                    }
                    slot.y += 16f;
                    slot.x += slot.width - 16f;
                    slot.width = 16f;
                    #endregion
                    #region Div Or or And
                    if (_commandSteps[i]._Operator != CommandSequences.InputSequence.commandStep.Operator.Next && i < _commandSteps.Length - 1)
                    {
                        Rect div = GUILayoutUtility.GetRect(42f, 16f);
                        div.y += 30f;
                        //GUI.Box(div,"And");
                        string[] options = { "Or", "And", "+" };
                        int selected = _commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.Or ? 0 : _commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.And ? 1 : 2;
                        selected = EditorGUI.Popup(div, selected, options);
                        _commandSteps[i]._Operator = (CommandSequences.InputSequence.commandStep.Operator)System.Enum.GetValues(typeof(CommandSequences.InputSequence.commandStep.Operator)).GetValue(selected);
                    }
                    #endregion
                    #region ADD and Deadline
                    if (_commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.Next || i == _commandSteps.Length - 1)
                    {
                        Rect div = GUILayoutUtility.GetRect(60f, 64f);
                        div.width = 32f;
                        //div.y += 10f;
                        div.height = 16f;
                        if (GUI.Button(div, "Add"))
                        {
                            _commandSteps[i]._Operator = CommandSequences.InputSequence.commandStep.Operator.And;
                            _commandSteps = LeoLuz.Utilities.ArrayUtility.Insert(_commandSteps, new CommandSequences.InputSequence.commandStep(), i + 1);
                            _commandSteps[i + 1]._Operator = CommandSequences.InputSequence.commandStep.Operator.Next;
                        }
                        div.y += 16f;
                        div.height = 14f;
                        div.width = 68f;
                        GUI.skin.label.fontSize = 9;
                        GUI.skin.textField.fontSize = 9;
                        if (_commandSteps[i].first > 0)
                        {
                            GUI.Label(div, "Deadline", GUI.skin.label);
                            div.y += 12f;
                            _commandSteps[i].deadline = Mathf.Clamp(EditorGUI.FloatField(div, _commandSteps[i].deadline, GUI.skin.textField), 0.017f, Mathf.Infinity);
                            div.y += 14f;
                            GUI.Label(div, "Anticipation", GUI.skin.label);
                            div.y += 12f;
                            _commandSteps[i].Antecipation = EditorGUI.FloatField(div, _commandSteps[i].Antecipation, GUI.skin.textField);
                            if (_commandSteps[i].Antecipation > 0f)
                                _commandSteps[i].Antecipation = -_commandSteps[i].Antecipation;


                        }
                        GUI.skin.label.fontSize = 11;
                        GUI.skin.textField.fontSize = 11;
                    }

                    #endregion
                    #region End Horizontal Center and  div Next
                    if (i == _commandSteps.Length - 1 || (_commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.Next))
                    {

                        DGUI.EndHorizontalCenter();
                        GUILayout.Space(5f);

                        #region div Next
                        if (i < _commandSteps.Length - 1)
                        {
                            DGUI.BeginHorizontalCenter();
                            Rect div = GUILayoutUtility.GetRect(42f, 32f);
                            div.x -= 29f;
                            div.y += 8f;
                            string[] options = { "And", "Or", "+" };
                            int selected = _commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.Or ? 0 : _commandSteps[i]._Operator == CommandSequences.InputSequence.commandStep.Operator.And ? 1 : 2;
                            selected = EditorGUI.Popup(div, selected, options);
                            _commandSteps[i]._Operator = (CommandSequences.InputSequence.commandStep.Operator)System.Enum.GetValues(typeof(CommandSequences.InputSequence.commandStep.Operator)).GetValue(selected);
                            DGUI.EndHorizontalCenter();
                        }
                        #endregion
                    }
                    #endregion
                }
                DGUI.BeginHorizontalCenter();
                Rect newStep = GUILayoutUtility.GetRect(32f, 32f);
                newStep.x -= 16f;
                if (!withoutNext && GUI.Button(newStep, "Add"))
                {
                    if (_commandSteps.Length > 0)
                    {
                        _commandSteps[_commandSteps.Length - 1]._Operator = CommandSequences.InputSequence.commandStep.Operator.Next;
                    }
                    LeoLuz.Utilities.ArrayUtility.Add(ref _commandSteps, new CommandSequences.InputSequence.commandStep());
                }
                DGUI.EndHorizontalCenter();
                GUILayout.Space(10f);

            }
        }
    }
}




