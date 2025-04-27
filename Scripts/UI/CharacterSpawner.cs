using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject characterPanelPrefab;
    [SerializeField] private ReplacementSpawner replacementSpawner;

    [SerializeField] private GameObject selfPanel;
    [SerializeField] private GameObject activatePanel;
    [SerializeField] private TMP_Text errorText;
    private List<CharacterSelectPanel> spawnedPrefabs;

    private void OnEnable()
    {
        StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (spawnedPrefabs == null)
        {
            return;
        }

        foreach (var obj in spawnedPrefabs)
        {
            Destroy(obj.gameObject);
        }

        spawnedPrefabs = null;
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitUntil(() => !CharacterLibrary.IsLoading());

        List<Variant> baseCivilians = CharacterLibrary.Instance.GetDefaults();
        foreach (var civilian in baseCivilians)
        {
            errorText.gameObject.SetActive(false);
            OnFoundCharacter(civilian);
        }
    }

    private void OnFoundCharacter(Variant civilian)
    {
        spawnedPrefabs ??= new List<CharacterSelectPanel>();
        var categoryObj = Instantiate(characterPanelPrefab, transform);
        CharacterSelectPanel panel = categoryObj.GetComponent<CharacterSelectPanel>();
        panel.SetCharacter(civilian, replacementSpawner);
        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            panel.GetComponent<Button>().Select();
        }

        panel.GetComponent<Button>().onClick.AddListener(() => {
            selfPanel.SetActive(false);
            activatePanel.SetActive(true);
        });
        spawnedPrefabs.Add(panel);
    }
}
