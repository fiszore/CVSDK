using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Data/Dialogue", order = 8)]
public class Dialogue : ScriptableObject {
    [SerializeField] private List<DialogueLine> lines;
    private List<DialoguePrefab> dialoguePrefabs;
    private bool playing = false;
    public bool GetPlaying() => playing;

    public void ForceEnd() {
        playing = false;
        foreach (var dialoguePrefab in dialoguePrefabs) {
            if (dialoguePrefab != null) {
                Destroy(dialoguePrefab.gameObject);
            }
        }
        dialoguePrefabs.Clear();
    }

    public IEnumerator Begin(IList<DialogueCharacter> characters) {
        try {
            playing = true;
            dialoguePrefabs = new();
            WaitForSeconds interval = new WaitForSeconds(0.1f);
            foreach (var line in lines) {
                if ((int)line.actor > characters.Count) {
                    continue;
                }
                var speaker = characters[(int)line.actor];
                var obj = Instantiate(speaker.GetDialogueTheme().GetDialoguePrefab().gameObject);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                var prefab = obj.GetComponent<DialoguePrefab>();

                DialogueContainer container = speaker.GetTransform().GetComponent<DialogueContainer>();
                if (container == null)
                {
                    container = speaker.GetTransform().gameObject.AddComponent<DialogueContainer>();
                }

                container.AddPrefab(prefab);
                prefab.AttachTo(speaker.GetTransform());
                dialoguePrefabs.Add(prefab);
                var subject = (int)line.subject < characters.Count ? characters[(int)line.subject] : speaker;
                var speakerOverrides = speaker.GetDialogueOverrides();
                var subjectOverrides = subject.GetDialogueOverrides();

                bool isSmart = false;
                {
                    var stringTable = LocalizationSettings.StringDatabase.GetTable(line.line.TableReference) as StringTable;
                    var entry = stringTable?.GetEntryFromReference(line.line.TableEntryReference) as StringTableEntry;
                    isSmart = entry != null && entry.IsSmart;
                }

                // Only pass arguments if the entry is Smart
                if (isSmart)
                {
                    var vars = new
                    {
                        speakerName = speakerOverrides.CharacterName,
                        speakerIdentifier = speakerOverrides.Identifier,
                        subjectName = subjectOverrides.CharacterName,
                        subjectIdentifier = subjectOverrides.Identifier,
                    };
                    line.line.Arguments = new object[] { vars };
                }

                var handle = line.line.GetLocalizedStringAsync();
                yield return handle;
                string lineString = handle.Result;
                prefab.GetLocalizeStringEvent().StringReference = line.line;
                prefab.SetText(lineString);
                prefab.SetMaxVisibleCharacters(0);

                float startTime = Time.time;
                float duration = lineString.Length * 0.04f;
                while (Time.time < startTime + duration) {
                    float t = (Time.time - startTime) / duration;
                    int visibleCharacters = Mathf.CeilToInt(t * lineString.Length);
                    prefab.SetMaxVisibleCharacters(visibleCharacters);
                    AudioPack.PlayClipAtPoint(speaker.GetDialogueTheme().GetTalkPack(), prefab.GetAttachPosition());
                    yield return interval;
                }
                prefab.SetMaxVisibleCharacters(lineString.Length+1);
                yield return new WaitForSeconds(1.4f);
                prefab.SetMaxVisibleCharacters(0);
                Destroy(prefab);
                dialoguePrefabs.Remove(prefab);
            }
        } finally {
            playing = false;
            foreach (var dialoguePrefab in dialoguePrefabs) {
                Destroy(dialoguePrefab.gameObject);
            }

            dialoguePrefabs.Clear();
        }
    }
}
