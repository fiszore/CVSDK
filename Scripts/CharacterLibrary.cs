using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;

public class CharacterLibrary : MonoBehaviour
{
    public static CharacterLibrary Instance;

    private AsyncOperationHandle<IList<GameObject>>? baseCharacterHandle;

    Dictionary<string, CharacterData> variants = new Dictionary<string, CharacterData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartCoroutine(Initialize());
        }
    }

    private IEnumerator Initialize()
    {
        List<string> civKeys = new List<string> { "ChurnVectorCharacter" };
        baseCharacterHandle ??= Addressables.LoadAssetsAsync<GameObject>(civKeys, (civilian) => {}, Addressables.MergeMode.Union, false);

        yield return new WaitUntil(() => baseCharacterHandle.Value.IsDone);
        foreach (var civ in baseCharacterHandle.Value.Result)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(civ));
            if (!variants.ContainsKey(guid))
                variants.Add(guid, new CharacterData(new CivilianReference(guid)));
        }

        yield return new WaitUntil(() => !Modding.IsLoading());
        foreach (var mod in Modding.GetMods())
        {
            foreach (var replacement in mod.GetDescription().GetReplacementCharacters())
            {
                if (variants.ContainsKey(replacement.existingGUID))
                {
                    variants[replacement.existingGUID].AddVariant(new CivilianReference(replacement.replacementGUID));
                }
            }
        }

        foreach (var data in variants.Values)
        {
            yield return StartCoroutine(data.LoadAllVariants());
        }
    }

    public CivilianReference GetCivilianReference(string existingCharacterGUID)
    {
        if (!variants.ContainsKey(existingCharacterGUID))
            return null;

        return variants[existingCharacterGUID].GetCharacter();
    }

    public List<Variant> GetAllVariants(string baseCivilianGUID)
    {
        if (!variants.ContainsKey(baseCivilianGUID))
            return null;

        return variants[baseCivilianGUID].GetVariants();
    }

    public List<CivilianReference> GetDefaultCharacters()
    {
        return variants.Values.Select(variant => variant.Default).ToList();
    }

    public List<Variant> GetDefaults()
    {
        return variants.Values.Select(variant => variant.DefaultData).ToList();
    }

    public VariantSelectMethod GetSelectionMethod(string GUID)
    {
        if (!variants.ContainsKey(GUID))
            return VariantSelectMethod.Random;

        return variants[GUID].SelectionMethod;
    }

    public void SetSelectionMethod(string GUID, VariantSelectMethod selectionMethod)
    {
        if (!variants.ContainsKey(GUID))
            return;

        variants[GUID].SetSelectionMethod(selectionMethod);
    }

    public class CharacterData
    {
        private string defaultCharacter;
        private Dictionary<string, Variant> variants = new Dictionary<string, Variant>();

        private VariantSelectMethod selectionMethod;

        public CharacterData(CivilianReference defautCharacter)
        {
            this.defaultCharacter = defautCharacter.AssetGUID;
            variants = new Dictionary<string, Variant>();
            AddVariant(defautCharacter);
            selectionMethod = VariantSelectMethod.Random;
        }

        public void AddVariant(CivilianReference variant)
        {
            if (!variants.ContainsKey(variant.AssetGUID))
                variants.Add(variant.AssetGUID, new Variant(variant));
        }

        public CivilianReference Default => variants[defaultCharacter].CivilianReference;
        public Variant DefaultData => variants[defaultCharacter];

        public VariantSelectMethod SelectionMethod => selectionMethod;
        public void SetSelectionMethod(VariantSelectMethod method)
        {
            selectionMethod = method;
        }

        int altIndex = 0;

        public CivilianReference GetCharacter()
        {
            if (variants.Count < 1)
                return Default;

            var enabledValues = variants.Values.Where(v => v.Enabled).ToList();

            if (enabledValues.Count == 0)
                return Default;

            switch (selectionMethod)
            {
                case VariantSelectMethod.Random:
                    return enabledValues[UnityEngine.Random.Range(0, enabledValues.Count)].CivilianReference;

                case VariantSelectMethod.Alternating:
                    var result = enabledValues[altIndex].CivilianReference;
                    altIndex = (altIndex + 1) % enabledValues.Count;
                    return result;

                default:
                    return Default;
            }
        }

        public IEnumerator LoadAllVariants()
        {
            foreach (var variant in variants.Values)
            {
                yield return Instance.StartCoroutine(variant.LoadCharacterData());
            }
        }

        public List<CivilianReference> GetCharacters()
        {
            return variants.Values.Select(civ => civ.CivilianReference).ToList();
        }

        public List<Variant> GetVariants()
        {
            return variants.Values.ToList();
        }
    }
}

public enum VariantSelectMethod
{
    Random,
    Alternating,
}

public class Variant
{
    private CivilianReference reference;
    private string name;
    private Sprite characterIcon;
    private bool enabled;

    public Variant(CivilianReference reference, bool active = true)
    {
        this.reference = reference;
        this.enabled = active;
    }

    public CivilianReference CivilianReference => reference;
    public bool Enabled => enabled;

    public string Name => name;
    public Sprite CharacterIcon => characterIcon;

    public void ToggleVariant()
    {
        enabled = !enabled;
    }

    public void ToggleVariant(bool enabled)
    {
        this.enabled = enabled;
    }

    private AsyncOperationHandle<GameObject>? characterHandle;

    public IEnumerator LoadCharacterData()
    {
        if (characterHandle.HasValue && characterHandle.Value.IsDone)
            yield break;

        characterHandle ??= reference.LoadAssetAsync<GameObject>();
        yield return new WaitUntil(() => characterHandle.Value.IsDone);

        var characterObject = characterHandle.Value.Result;
        name = characterObject.name;
        characterIcon = ((IChurnable)characterObject.GetComponent<Civilian>()).GetHeadSprite();
    }
}
