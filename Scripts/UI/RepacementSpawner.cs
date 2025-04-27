using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ReplacementSpawner : MonoBehaviour
{
    [SerializeField] private GameObject characterPanelPrefab;
    [SerializeField] private GameObject junkPanel;
    [SerializeField] private ReplacementOption replacementOptions;
    private List<ReplacementPanel> spawnedPrefabs;
    private string selectedCharacter;

    public void SetCharacter(string GUID)
    {
        selectedCharacter = GUID;
    }

    private void OnEnable()
    {
        StartCoroutine(SpawnRoutine());
        junkPanel.SetActive(false);
        replacementOptions.gameObject.SetActive(true);
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
        junkPanel.SetActive(true);
        replacementOptions.gameObject.SetActive(false);
    }

    //TODO - Loading needs to be ASYNC or else it DOESNT LOAD IN TIME surprise surprise

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitUntil(() => !Modding.IsLoading());

        List<Variant> baseCivilians = CharacterLibrary.Instance.GetAllVariants(selectedCharacter);
        foreach (var civilian in baseCivilians)
        {
            if(civilian != null)
                OnFoundCharacter(civilian);
        }
        replacementOptions.SetCharacter(selectedCharacter);
    }

    private void OnFoundCharacter(Variant civilian)
    {
        spawnedPrefabs ??= new List<ReplacementPanel>();
        var categoryObj = Instantiate(characterPanelPrefab, transform);
        ReplacementPanel panel = categoryObj.GetComponent<ReplacementPanel>();
        panel.SetCharacter(civilian);
        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            panel.GetComponent<Button>().Select();
        }
        spawnedPrefabs.Add(panel);
    }

    public void RefreshPanels()
    {
        foreach(var panel in spawnedPrefabs)
            panel.Refresh();
    }
}
