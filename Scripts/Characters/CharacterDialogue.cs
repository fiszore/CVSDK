using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDialogue : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField] private string characterName = "Employee";
    [SerializeField] private string identifier = "person";

    [Header("Settings")]
    [SerializeField] private bool useDefaults = true;
    [SerializeField] private Chatter chatterLevel = Chatter.Default;

    [Header("Dialogue")]
    [SerializeField] DialogueLibrary.DialogueGroup[] dialogueOverrides;

    public bool UseDefaults => useDefaults;
    public string CharacterName => characterName;
    public string Identifier => identifier;

    public DialogueLibrary.DialogueGroup[] Dialogue => dialogueOverrides;

    public bool ShouldSpeak()
    {
        switch(chatterLevel)
        {
            case Chatter.Minimal:
                return Random.Range(0.1f, 0.4f) < 0.2f;
            case Chatter.Silent:
                return false;
            default:
                return true;
        }
    }

    public enum Chatter
    {
        Default,
        Minimal,
        Silent
    }
}
