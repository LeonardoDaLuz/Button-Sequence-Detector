using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeoLuz.Extensions;
using LeoLuz.Utilities;

public partial class CommandSequences
{
    [System.Serializable]
    public class InputSequence
    {
        public string Name = "New Sequence";
        public commandStep[] commandList;
        public int[] CommandStepsToAntecipate;
        public int CurrentSequenceIndex;
        private float TimeOfTheLastSequencePerformed = -9999f;


        [System.Serializable]
        public class commandStep
        {
            public enum Operator { Or, And, Next }
            public enum InputEvent { ButtonDown, ButtonHold, ButtonUp, ButtonReleased, Axis }
            public enum GreaterEqualLower { greater, Equal, Lower, }
            public enum AxisDirection { Positive, Negative, PositiveOrNegative }
            public enum AxisInversion { NoInvert, InvertRelativeToHorizontalFacing, InvertRelativeToVerticalFacing }
            public int next;
            public int first;
            public int Or;
            public int And;
            public int id;

            public bool IsAxis; /*The variable 'HasNegative' indicates that the chosen axis has input for negative values (for example the "Horizontal" and 
                                                               * "Vertical" axis.) Axis without input for negative values are like joystick buttons (for example Fire1, Fire2, Jump
                                                               *  axis) that in fact do not This value is externally assigned by a property drawer called InputAxisListDropDrawer*/
                                // [HideInInspector]
            public bool IsAbstractedDiretional; /* The variable 'IsAbstractedDiretional' indicates that this command uses abstracted directional class. The
                                                                         *  abstracted directional class were made in such a way as to simulate the diagonal digital imputs used 
                                                                         * in the list of commands of the fighting games moves (↗↘ → ↖↙ ← ↑ ↓). Because the axis does 
                                                                         * not support the Down, Up, and Hold events of diagonal directional, its creation was necessary.*/
            public string RealAxisButtonName = "Fire1"; //Definition of axis: any button ou axis
            public string NegativeRealAxisButtonName = "Fire1"; //Definition of axis: any button ou axis
            public Operator _Operator;
            public InputEvent inputEvent = InputEvent.ButtonDown;
            public GreaterEqualLower Statement;
            public float value;
            public AxisDirection axisDirection = AxisDirection.Positive;
            public AxisInversion axisInversion = AxisInversion.NoInvert;
            public float deadline = 0.2f;
            public float Antecipation = 0.2f;
            public float LastVerifiedAnticipation = -999f;

            public static bool debug;
            #region Methods
            public bool commandChainWerePerformedCMInput(CommandSequences root, commandStep[] commandSteps)
            {
                if (Antecipation < 0f && Time.time < LastVerifiedAnticipation - Antecipation && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                {
                    if (debug)
                        Debug.Log("<color=green>ANTECIPATED " + RealAxisButtonName + " " + inputEvent + "</color>");

                    LastVerifiedAnticipation = -9999f;
                    return true;
                }

                if (IsAbstractedDiretional)
                {
                    string abstractedAxisButton = axisInversion == AxisInversion.NoInvert ? this.RealAxisButtonName : axisInversion == AxisInversion.InvertRelativeToHorizontalFacing ? root.FacingRight ? this.RealAxisButtonName : NegativeRealAxisButtonName : root.FacingUp ? this.RealAxisButtonName : NegativeRealAxisButtonName;
                    // Debug.Log("AxisNameVerified: " + AxisName);
                    switch (inputEvent)
                    {
                        case InputEvent.ButtonDown:
                            if (root.AbstractedDirectionalInput.GetDirectionalDown(abstractedAxisButton) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                return true;
                            break;
                        case InputEvent.ButtonHold:
                            if (root.AbstractedDirectionalInput.GetDirectionalHold(abstractedAxisButton) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                return true;
                            break;
                        case InputEvent.ButtonUp:
                            if (root.AbstractedDirectionalInput.GetDirectionalUp(abstractedAxisButton) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                return true;
                            break;
                        case InputEvent.ButtonReleased:
                            if (root.AbstractedDirectionalInput.GetDirectionalReleased(abstractedAxisButton) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                return true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (!IsAxis)
                    {
                        switch (inputEvent)
                        {
                            case InputEvent.ButtonDown:
                                if (Input.GetButtonDown(RealAxisButtonName + root.config.PlayerSlotPrefix) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    return true;
                                break;
                            case InputEvent.ButtonHold:
                                if (Input.GetButton(RealAxisButtonName + root.config.PlayerSlotPrefix) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    return true;
                                break;
                            case InputEvent.ButtonUp:
                                if (Input.GetButtonUp(RealAxisButtonName + root.config.PlayerSlotPrefix) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    return true;
                                break;
                            case InputEvent.ButtonReleased:
                                if (!(Input.GetButtonUp(RealAxisButtonName + root.config.PlayerSlotPrefix) || Input.GetButton(RealAxisButtonName + root.config.PlayerSlotPrefix) || Input.GetButtonDown(RealAxisButtonName + root.config.PlayerSlotPrefix)) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    return true;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (inputEvent)
                        {
                            case InputEvent.Axis:

                                float Axis = axisInversion == AxisInversion.NoInvert ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : axisInversion == AxisInversion.InvertRelativeToHorizontalFacing ? root.FacingRight ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : -Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : root.FacingUp ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : -Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix);

                                switch (Statement)
                                {
                                    case GreaterEqualLower.greater:
                                        if (value < Axis && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                            return true;
                                        break;
                                    case GreaterEqualLower.Equal:
                                        if (value == Axis && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                            return true;
                                        break;
                                    default:
                                        if (value > Axis && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                            return true;
                                        break;
                                }
                                break;
                            case InputEvent.ButtonDown:
                                if (Input.GetButtonDown(RealAxisButtonName + root.config.PlayerSlotPrefix) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                {
                                    if (CheckInversions(root))
                                        return true;
                                }
                                break;
                            case InputEvent.ButtonHold:
                                if (Input.GetButton(RealAxisButtonName + root.config.PlayerSlotPrefix) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    if (CheckInversions(root))
                                        return true;
                                break;
                            case InputEvent.ButtonUp:
                                if (Input.GetButtonUp(RealAxisButtonName + root.config.PlayerSlotPrefix) && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    if (CheckInversions(root))
                                        return true;
                                break;
                            case InputEvent.ButtonReleased:
                                float ThisAxis = Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix);
                                if (ThisAxis < 0.5f && ThisAxis > -0.5f && (And == -1 || commandSteps[And].commandChainWerePerformedCMInput(root, commandSteps)))
                                    if (CheckInversions(root))
                                        return true;
                                break;
                            default:
                                break;

                        }
                    }
                }

                if (Or == -1)
                    return false;
                else if (commandSteps[Or].commandChainWerePerformedCMInput(root, commandSteps))
                    return true;

                return false;
            }

            public bool commandWerePerformedCMInput(CommandSequences root)
            {
                if (IsAbstractedDiretional)
                {
                    string abstractedAxisButton = axisInversion == AxisInversion.NoInvert ? this.RealAxisButtonName : axisInversion == AxisInversion.InvertRelativeToHorizontalFacing ? root.FacingRight ? this.RealAxisButtonName : NegativeRealAxisButtonName : root.FacingUp ? this.RealAxisButtonName : NegativeRealAxisButtonName;
                    // Debug.Log("AxisNameVerified: " + AxisName);
                    switch (inputEvent)
                    {
                        case InputEvent.ButtonDown:
                            if (root.AbstractedDirectionalInput.GetDirectionalDown(abstractedAxisButton))
                                return true;
                            break;
                        case InputEvent.ButtonHold:
                            if (root.AbstractedDirectionalInput.GetDirectionalHold(abstractedAxisButton))
                                return true;
                            break;
                        case InputEvent.ButtonUp:
                            if (root.AbstractedDirectionalInput.GetDirectionalUp(abstractedAxisButton))
                                return true;
                            break;
                        case InputEvent.ButtonReleased:
                            if (root.AbstractedDirectionalInput.GetDirectionalReleased(abstractedAxisButton))
                                return true;
                            break;
                        default:
                            break;
                    }
                }
                else
                {


                    if (!IsAxis)
                    {
                        switch (inputEvent)
                        {
                            case InputEvent.ButtonDown:
                                if (Input.GetButtonDown(RealAxisButtonName + root.config.PlayerSlotPrefix))
                                    return true;
                                break;
                            case InputEvent.ButtonHold:
                                if (Input.GetButton(RealAxisButtonName + root.config.PlayerSlotPrefix))
                                    return true;
                                break;
                            case InputEvent.ButtonUp:
                                if (Input.GetButtonUp(RealAxisButtonName + root.config.PlayerSlotPrefix))
                                    return true;
                                break;
                            case InputEvent.ButtonReleased:
                                if (!(Input.GetButtonUp(RealAxisButtonName + root.config.PlayerSlotPrefix) || Input.GetButton(RealAxisButtonName + root.config.PlayerSlotPrefix) || Input.GetButtonDown(RealAxisButtonName + root.config.PlayerSlotPrefix)))
                                    return true;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (inputEvent)
                        {
                            case InputEvent.Axis:

                                float Axis = axisInversion == AxisInversion.NoInvert ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : axisInversion == AxisInversion.InvertRelativeToHorizontalFacing ? root.FacingRight ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : -Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : root.FacingUp ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : -Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix);

                                switch (Statement)
                                {
                                    case GreaterEqualLower.greater:
                                        if (value < Axis)
                                            return true;
                                        break;
                                    case GreaterEqualLower.Equal:
                                        if (value == Axis)
                                            return true;
                                        break;
                                    default:
                                        if (value > Axis)
                                            return true;
                                        break;
                                }
                                break;
                            case InputEvent.ButtonDown:
                                if (Input.GetButtonDown(RealAxisButtonName + root.config.PlayerSlotPrefix))
                                {
                                    if (CheckInversions(root))
                                        return true;
                                }
                                break;
                            case InputEvent.ButtonHold:
                                if (Input.GetButton(RealAxisButtonName + root.config.PlayerSlotPrefix))
                                    if (CheckInversions(root))
                                        return true;
                                break;
                            case InputEvent.ButtonUp:
                                if (Input.GetButtonUp(RealAxisButtonName + root.config.PlayerSlotPrefix))
                                    if (CheckInversions(root))
                                        return true;
                                break;
                            case InputEvent.ButtonReleased:
                                float ThisAxis = Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix);
                                if (ThisAxis < 0.5f && ThisAxis > -0.5f)
                                    if (CheckInversions(root))
                                        return true;
                                break;
                            default:
                                break;

                        }
                    }
                }

                return false;
            }

            public bool CheckInversions(CommandSequences root)
            {

                float Axis = axisInversion == AxisInversion.NoInvert || (axisInversion == AxisInversion.InvertRelativeToHorizontalFacing && root.FacingRight) ||
                    (axisInversion == AxisInversion.InvertRelativeToVerticalFacing && root.FacingUp) ? Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix) : -Input.GetAxis(RealAxisButtonName + root.config.PlayerSlotPrefix);

                if (axisDirection == AxisDirection.PositiveOrNegative)
                    return true;
                else if (axisDirection == AxisDirection.Positive && Axis > 0f)
                    return true;
                else if (axisDirection == AxisDirection.Negative && Axis < 0f)
                    return true;

                return false;
            }
            #endregion

        }
        [System.Serializable]
        public class AbstractedDirectional
        {
            /// <summary>
            /// AbstractedDirectional was created to abstract common digital directionalities in fighting game commands. Due to the limitations of unity this is not possible by default. This class needs to be refreshed with each
            ///frame to detect Button Down, Button Up and Button Hold.This class is able to detect diagonal down arrow for example.
            ///What is not possible through a simple get axis.
            /// </summary>
            public string CurrentDirection;
            public string DirectionOnLastFrame;
            public float DeadZone = 0.5f;
            public int LastFrameUpdated;
            public string Horizontal;
            public string Vertical;

            public void Refresh(CommandSequences parent)
            {
                float hor = Input.GetAxisRaw(parent.config.DigitalHorizontal + parent.config.PlayerSlotPrefix);
                float ver = Input.GetAxisRaw(parent.config.DigitalVertical + parent.config.PlayerSlotPrefix);

                if (hor > DeadZone)
                {
                    if (ver > DeadZone)
                    {
                        SetDirection("↗");
                    }
                    else if (ver < -DeadZone)
                    {
                        SetDirection("↘");
                    }
                    else
                    {
                        SetDirection("→");
                    }
                }
                else if (hor < -DeadZone)
                {
                    if (ver > DeadZone)
                    {
                        SetDirection("↖");
                    }
                    else if (ver < -DeadZone)
                    {
                        SetDirection("↙");
                    }
                    else
                    {
                        SetDirection("←");
                    }
                }
                else
                {
                    if (ver > DeadZone)
                    {
                        SetDirection("↑");
                    }
                    else if (ver < -DeadZone)
                    {
                        SetDirection("↓");
                    }
                    else
                    {
                        SetDirection("");
                    }
                }
                //Debug.Log("Dir: " + Direction);
            }

            void SetDirection(string dir)
            {
                DirectionOnLastFrame = CurrentDirection;
                CurrentDirection = dir;
                LastFrameUpdated = Time.frameCount;
            }

            //Events Hold, Down and Up is obtained though comparison of keys of last and penult frames.
            public bool GetDirectionalHold(string dir)
            {
                if (dir == CurrentDirection)
                {
                    return true;
                }
                return false;
            }
            public bool GetDirectionalDown(string dir)
            {
                if (dir == CurrentDirection && CurrentDirection != DirectionOnLastFrame)
                {
                    return true;
                }
                return false;
            }
            public bool GetDirectionalUp(string dir)
            {
                if (dir == DirectionOnLastFrame && CurrentDirection != DirectionOnLastFrame)
                {
                    return true;
                }
                return false;
            }
            public bool GetDirectionalReleased(string dir)
            {
                if (dir != DirectionOnLastFrame && dir != CurrentDirection)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        ///  inside this method even if it returns false it is registering and annotating the sequences so that when executing the last sequence it returns true, and then the move begins.
        /// </summary>
        public bool CommandSequenceToStartWerePerformed(CommandSequences root)
        {
            if (commandList.Length == 0)
                return true;

            //Jump to first commandstep of this horizontal list
            //If dead line expires, reset index
            if (CurrentSequenceIndex > 0 && TimeOfTheLastSequencePerformed + commandList[CurrentSequenceIndex].deadline < Time.time)
            {
                CurrentSequenceIndex = 0;
            }
            ///Check if command is performed
            if (commandList[CurrentSequenceIndex].commandChainWerePerformedCMInput(root, commandList))
            {
                TimeOfTheLastSequencePerformed = Time.time;

                if (commandList[CurrentSequenceIndex].next == -1)
                {
                    CurrentSequenceIndex = 0;
                    return true;
                }
                else
                {
                    CurrentSequenceIndex = commandList[CurrentSequenceIndex].next;
                    VerifyAntecipations(root);
                    return false;
                }
            }
            if (CurrentSequenceIndex > 0)
                VerifyAntecipations(root);

            return false;

        }

        void VerifyAntecipations(CommandSequences root)
        {
            for (int i = 0; i < CommandStepsToAntecipate.Length; i++)
            {
                if (commandList[CommandStepsToAntecipate[i]].commandWerePerformedCMInput(root))
                {
                    if (commandStep.debug)
                        Debug.Log("<color=blue>" + commandList[CommandStepsToAntecipate[i]].RealAxisButtonName + "</color> antecipation register" + " frame:" + Time.frameCount + " time:" + Time.time.ToString("0.00"));
                    commandList[CommandStepsToAntecipate[i]].LastVerifiedAnticipation = Time.time;
                }
            }
        }

        public void AutofitConnections()
        {
            commandStep[] commands = commandList;

            if (commands == null || commands.Length == 0)
                return;

            if (commands.Length == 1)
            {
                commands[0].And = -1;
                commands[0].Or = -1;
                commands[0].next = -1;
                commands[0].first = -1;
                return;
            }

            //Search AND links
            for (int i = 0; i < commands.Length - 1; i++)
            {
                commands[i].And = -1;
                if (commands[i]._Operator == commandStep.Operator.And)
                {
                    commands[i].And = i + 1;
                }
            }
            //Search OR links
            for (int i = 0; i < commands.Length - 1; i++)
            {
                commands[i].Or = -1;
                //Get next OR
                for (int ib = i; ib < commands.Length - 1; ib++)
                {
                    if (commands[ib]._Operator == commandStep.Operator.Next)
                    {
                        commands[i].Or = -1;
                        break;
                    }
                    if (commands[ib]._Operator == commandStep.Operator.Or)
                    {
                        commands[i].Or = ib + 1;
                        break;
                    }
                }
            }
            //Search "+/Next" links
            for (int i = 0; i < commands.Length - 1; i++)
            {
                commands[i].next = -1;
                //Get next OR
                for (int ib = i; ib < commands.Length - 1; ib++)
                {
                    if (commands[ib]._Operator == commandStep.Operator.Next)
                    {
                        commands[i].next = ib + 1;
                        commands[i].deadline = commands[ib].deadline;
                        break;
                    }
                }
            }
            //Search "First" links
            for (int i = commands.Length - 1; i > -1; i--)
            {

                if (i == 0)
                {
                    commands[0].first = 0;
                }
                else
                {
                    //search first
                    for (int ib = i - 1; ib > -1; ib--)
                    {
                        if (commands[ib]._Operator == commandStep.Operator.Next)
                        {
                            commands[i].first = ib + 1;
                            break;
                        }
                    }
                }
            }
            //cleaning final chain
            commands[commands.Length - 1].Or = -1;
            commands[commands.Length - 1].next = -1;
            commands[commands.Length - 1].And = -1;

            //Normalize deadlines
            float lastDeadLine = 999999f;
            for (int i = commands.Length - 1; i > -1; i--)
            {
                if (commands[i]._Operator == commandStep.Operator.Next || i == commands.Length - 1)
                {
                    lastDeadLine = commands[i].deadline;
                }
                else
                {
                    commands[i].deadline = lastDeadLine;
                }
            }
            //Search and normalize Antecipations links
            float LastAntecipationTime = 999999f;
            CommandStepsToAntecipate = new int[0];
            for (int i = commands.Length - 1; i > -1; i--)
            {
                //commands[i].LastVerifiedAntecipation = -9999f;
                if (commands[i]._Operator == commandStep.Operator.Next || i == commands.Length - 1)
                {
                    LastAntecipationTime = commands[i].Antecipation;
                }
                else
                {
                    commands[i].Antecipation = LastAntecipationTime;
                }
                if (commands[i].Antecipation < 0f)
                {
                    if (!CommandStepsToAntecipate.Contains(i))
                    {
                        
                        ArrayUtility.Add(ref CommandStepsToAntecipate, i);
                    }
                }
            }
            //CALCULATE NEGATIVE ABSTRACTED AXIS NAME
            for (int i = commands.Length - 1; i > -1; i--)
            {
                if (commands[i].IsAbstractedDiretional && commands[i].axisInversion == commandStep.AxisInversion.InvertRelativeToHorizontalFacing)
                {
                    switch (commands[i].RealAxisButtonName)
                    {
                        case "↗":
                            commands[i].NegativeRealAxisButtonName = "↖";
                            break;
                        case "↑":
                            commands[i].NegativeRealAxisButtonName = "↑";
                            break;
                        case "↖":
                            commands[i].NegativeRealAxisButtonName = "↗";
                            break;
                        case "←":
                            commands[i].NegativeRealAxisButtonName = "→";
                            break;
                        case "↙":
                            commands[i].NegativeRealAxisButtonName = "↘";
                            break;
                        case "↓":
                            commands[i].NegativeRealAxisButtonName = "↓";
                            break;
                        case "↘":
                            commands[i].NegativeRealAxisButtonName = "↙";
                            break;
                        case "→":
                            commands[i].NegativeRealAxisButtonName = "←";
                            break;
                        default:
                            break;
                    }
                }
                else if (commands[i].IsAbstractedDiretional && commands[i].axisInversion == commandStep.AxisInversion.InvertRelativeToVerticalFacing)
                {
                    switch (commands[i].RealAxisButtonName)
                    {
                        case "↗":
                            commands[i].NegativeRealAxisButtonName = "↘";
                            break;
                        case "↑":
                            commands[i].NegativeRealAxisButtonName = "↓";
                            break;
                        case "↖":
                            commands[i].NegativeRealAxisButtonName = "↙";
                            break;
                        case "←":
                            commands[i].NegativeRealAxisButtonName = "←";
                            break;
                        case "↙":
                            commands[i].NegativeRealAxisButtonName = "↖";
                            break;
                        case "↓":
                            commands[i].NegativeRealAxisButtonName = "↑";
                            break;
                        case "↘":
                            commands[i].NegativeRealAxisButtonName = "↗";
                            break;
                        case "→":
                            commands[i].NegativeRealAxisButtonName = "→";
                            break;
                        default:
                            break;
                    }
                }
            }

        }


    }
}
