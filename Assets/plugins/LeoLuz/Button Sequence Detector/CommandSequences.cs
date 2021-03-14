using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeoLuz.PropertyAttributes;
using LeoLuz;

#if UNITY_EDITOR
using UnityEditor;
#endif


public partial class CommandSequences : MonoBehaviour
{
    public Config config;
    public InputSequence[] InputSequences;
    public bool FacingRight = true;
    public bool FacingUp = true;
    public SpriteRenderer spriteRenderer;
    InputSequence.AbstractedDirectional AbstractedDirectionalInput;
     
    [System.Serializable]
    public class Config
    {
        [InputAxesListDropdownAttribute]
        public string DigitalHorizontal = "Horizontal";
        [InputAxesListDropdownAttribute]
        public string DigitalVertical = "Vertical";
        public enum InversionDetection { Manual, ByScale, BySpriteFlip }
        public InversionDetection inversionDetection;

        [Range(1, 12)]
        public int Player = 1;
        public string PlayerSlotPrefix;
        public bool Debug = true;
    }

    private static List<string> PerformedSequences;
    /// <summary>
    /// Check if certain sequence is completed on Player 1
    /// </summary>
    /// <param name="name">Name of Sequence </param>
    /// <returns></returns>
    public static bool SequenceIsCompleted(string name)
    {
        return PerformedSequences.Contains(name);
    }
    /// <summary>
    /// Check if certain sequence is completed on specific player
    /// </summary>
    /// <param name="name">Name of Sequence </param>
    /// <param name="playerNumber">Player Number</param>
    /// <returns></returns>
    public static bool SequenceIsCompleted(string name, int playerNumber)
    {
        if (playerNumber == 1)
        {
            return PerformedSequences.Contains(name);
        }
        else
        {
            return PerformedSequences.Contains(name + " p" + playerNumber);
        }
    }

    void Awake()
    {
        //Autofit
        if (PerformedSequences == null)
            PerformedSequences = new List<string>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!Application.isPlaying)
        {
            //Auto fit settings to avoid bugs and mistakes.
            for (int i = 0; i < InputSequences.Length; i++)
            {
                InputSequences[i].AutofitConnections();
            }
        }
        AbstractedDirectionalInput = new InputSequence.AbstractedDirectional();
        config.PlayerSlotPrefix = config.Player == 1 ? "" : " p" + config.Player;
        //Test Axes configuration for player 2
        if (config.Player > 1)
        {
            try
            {
                Input.GetButtonDown(config.DigitalVertical + config.PlayerSlotPrefix);
            }
            catch
            {
                Debug.LogError("<b><color=blue>Calm down, my friend! input axes to player "+config.Player+" is missing...</color></b> To use the multiplayer feature you must configure an <b>alternate Axis</b> corresponding to <b>each command</b> of the first player, in Edit>ProjectSettings>Input, placing suffixes ´p(player number)´, example: ´ p1´, ´ p2´, ´p3´, ´p4´. \nIf you have 2 players, you must set up a second input for each command but with the prefix <b>´ p2´</b> at the end. For example: Horizontal p2, Vertical p2, Fire1 p2, Fire2 p2, Jump p2. \nIf you have 3 players, you must set up a third input for each command of the first player. For example: Horizontal p3, Vertical p3, Fire1 p3, Jump p3. For the first player you should leave the commands without suffixes. \nThus the Command Sequence Detector will abstract the change of players. \nYou only need to create command sequences for the first player.\n\nAn example of an axes layout for 4 players:<b><color=blue>\nHorizontal\nVertical\nPunch\nKick\nHorizontal p2\nVertical p2\nPunch p2\nKick p2\nHorizontal p3\nVertical p3\nPunch p3\nKick p3\nHorizontal p4\nVertical p4\nPunch p4\nKick p4</color></b>\n\n");
            }
        }
        font = (Font)Resources.Load("DejaVuSans");
    }

    void Update()
    {
        /*AbstractedDirectional was created to abstract common digital directionalities in fighting game commands. 
        Due to the limitations of unity this is not possible by default. This class needs to be refreshed with each
        frame to detect Button Down, Button Up and Button Hold. This class is able to detect diagonal down arrow for example.
        What is not possible through a simple get axis.*/
        AbstractedDirectionalInput.Refresh(this);

        CheckDirection();
        CheckSequences();

        if (config.Debug)
            CalculateGUILog();

    }
    void LateUpdate()
    {
        PerformedSequences.Clear();
    }
    void CheckDirection()
    {
        switch (config.inversionDetection)
        {
            case Config.InversionDetection.ByScale:
                if (transform.lossyScale.x > 0f)
                    FacingRight = true;
                else
                    FacingRight = false;

                if (transform.lossyScale.y > 0f)
                    FacingUp = true;
                else
                    FacingUp = false;

                break;
            case Config.InversionDetection.BySpriteFlip:
                if (spriteRenderer != null)
                {
                    FacingRight = !spriteRenderer.flipX;
                    FacingUp = !spriteRenderer.flipY;
                }
                break;
            default:
                break;
        }
    }
    private void CheckSequences()
    {
        for (int i = 0; i < InputSequences.Length; i++)
        {
            if (InputSequences[i].CommandSequenceToStartWerePerformed(this))
            {
                if (config.Player == 1)
                    PerformedSequences.Add(InputSequences[i].Name);
                else
                    PerformedSequences.Add(InputSequences[i].Name + " p" + config.Player);
                if (config.Debug)
                    print("<color=blue>Sequence <b>" + InputSequences[i].Name + "</b> was performed</color>");
            }
        }
    }


    string GuiStr = "";
    private int CompletedSequenceOnGui = -1;
    void CalculateGUILog()
    {
        GuiStr = "";
        if (CompletedSequenceOnGui != -1)
        {
            GuiStr += "\n<color=#ffdd00><b>" + InputSequences[CompletedSequenceOnGui].Name + "</b>\n";
            for (int ib = 0; ib < InputSequences[CompletedSequenceOnGui].commandList.Length; ib++)
            {
                GuiStr += InputSequences[CompletedSequenceOnGui].commandList[ib].RealAxisButtonName + (ib < InputSequences[CompletedSequenceOnGui].commandList.Length - 1 ? (InputSequences[CompletedSequenceOnGui].commandList[ib]._Operator == InputSequence.commandStep.Operator.Next ? " + " : InputSequences[CompletedSequenceOnGui].commandList[ib]._Operator == InputSequence.commandStep.Operator.And ? " and " : " or ") : "");

            }
            GuiStr += "</color>";
        }


        int mostLongSequeceCompleted = -1;
        for (int i = 0; i < InputSequences.Length; i++)
        {
            if (CommandSequences.SequenceIsCompleted(InputSequences[i].Name, config.Player))
            {
                if (mostLongSequeceCompleted == -1 || InputSequences[i].commandList.Length >= InputSequences[mostLongSequeceCompleted].commandList.Length)
                {
                    mostLongSequeceCompleted = i;
                }
            }

            if (InputSequences[i].CurrentSequenceIndex > 0)
            {
                GuiStr += "\n" + InputSequences[i].Name + "\n";
                for (int ib = 0; ib < InputSequences[i].commandList.Length; ib++)
                {
                    if (InputSequences[i].CurrentSequenceIndex > ib)
                        GuiStr += InputSequences[i].commandList[ib].RealAxisButtonName + (InputSequences[i].commandList[ib]._Operator== InputSequence.commandStep.Operator.Next? " + ": InputSequences[i].commandList[ib]._Operator == InputSequence.commandStep.Operator.And?" and ": " or ");
                    else
                        GuiStr += "<color=grey>" + InputSequences[i].commandList[ib].RealAxisButtonName + (ib < InputSequences[i].commandList.Length - 1 ? (InputSequences[i].commandList[ib]._Operator == InputSequence.commandStep.Operator.Next ? " + " : InputSequences[i].commandList[ib]._Operator == InputSequence.commandStep.Operator.And ? " and " : " or ") : "") + "</color>";
                }
            }
        }

        if (mostLongSequeceCompleted != -1)
        {
            CompletedSequenceOnGui = mostLongSequeceCompleted;
            StopAllCoroutines();
            StartCoroutine(CompletedSequencesOnGUIExpireTime());
        }
    }
    Font font;
    void OnGUI()
    {
        GUIStyle style = GUIStyle.none;
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.font = font;
//#if UNITY_WEBGL && !UNITY_EDITOR
//        GuiStr = new System.Text.StringBuilder(GuiStr)
//            .Replace("↗", "&#8599;")
//            .Replace("↘", "&#8600;")
//            .Replace("→", "&rarr;")
//            .Replace("↖", "&#8598;")
//            .Replace("↙", "&#8601")
//            .Replace("←", "&larr;")
//            .Replace("↑", "&uarr;")
//            .Replace("↓", "&darr;")
//            .ToString();
//#endif
        GUI.Box(new Rect(Screen.width * 0.05f, Screen.height * 0.05f, Screen.width * 0.9f, Screen.height * 0.9f), GuiStr, style);
    }

    IEnumerator CompletedSequencesOnGUIExpireTime()
    {
        yield return new WaitForSeconds(1f);
        CompletedSequenceOnGui = -1;
    }

}


