using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueContainer : MonoBehaviour
{
    private List<DialoguePrefab> activeDialogue = new List<DialoguePrefab>();
    float tempFlatOffset = 0.45f;

    public void AddPrefab(DialoguePrefab prefab)
    {
        List<DialoguePrefab> removalQueue = new List<DialoguePrefab>();

        foreach(DialoguePrefab existing in activeDialogue)
        {
            if(existing != null)
                existing.AdjustOffset(new Vector3(0f, tempFlatOffset, 0f));
            else
                removalQueue.Add(existing);
        }
        activeDialogue.Add(prefab);

        foreach(var toRemove in removalQueue)
            activeDialogue.Remove(toRemove);
    }
}
