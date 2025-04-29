using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class CharacterSpawnInfo {
    [SerializeField] private CivilianReference civilianPrefab;
    [SerializeField,SerializeReference,SubclassSelector] private InputGenerator inputSource;
    [SerializeField] private List<CharacterGroup> overrideGroups;
    [SerializeField] private List<CharacterGroup> overrideUseByGroups;
    [SerializeField] private OverrideFlags overrideFlags;
    private AsyncOperationHandle<Civilian> handle;
    
    public AsyncOperationHandle<Civilian> GetCharacter() {
        return GetCharacter(Vector3.zero, Quaternion.identity);
    }
    public AsyncOperationHandle<Civilian> GetCharacter(Vector3 position, Quaternion rotation) {
        if (handle.IsValid()) {
            return handle;
        }
        /*
        CivilianReference realReference = civilianPrefab;
        foreach (var mod in Modding.GetMods()) {
            foreach (var replacement in mod.GetDescription().GetReplacementCharacters()) {
                if (replacement.existingGUID == realReference.AssetGUID) {
                    realReference = new CivilianReference(replacement.replacementGUID);
                }
            } 
        }*/
        CivilianReference realReference = CharacterLibrary.Instance.GetCivilianReference(civilianPrefab.AssetGUID);
        if (realReference == null)
            realReference = civilianPrefab;
        handle = realReference.InstantiateAsync(position, rotation);
        handle.Completed += OnLoadComplete;
        return handle;
    }

    private void OnLoadComplete(AsyncOperationHandle<Civilian> obj) {
        if (obj.Result is not Civilian characterBase) {
            throw new UnityException("Loaded a non-civilian as a character! Characters must have it as a behavior!");
        }

        if (characterBase.gameObject.TryGetComponent(out StartingActionOverride overrides))
        {
            if((overrideFlags.Flags & (1 << 0)) != 0)
                overrides.ApplyActionOverride(inputSource);

            if ((overrideFlags.Flags & (1 << 1)) != 0)
                overrides.ApplyGroupOverrides(characterBase, overrideGroups, overrideUseByGroups);
        }

        characterBase.SetInputGenerator(inputSource);
        characterBase.SetGroups(overrideGroups);
        characterBase.SetUseByGroups(overrideUseByGroups);
    }

}
