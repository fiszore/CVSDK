using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterSelectPanel : MonoBehaviour
{
    [SerializeField] private Image categoryImage;
    [SerializeField] private TMP_Text categoryText;
    Variant variant;
    private ReplacementSpawner replacementSpawner;

    public void SetCharacter(Variant variant, ReplacementSpawner replacementSpawner)
    {
        this.variant = variant;
        this.replacementSpawner = replacementSpawner;
        categoryImage.sprite = variant.CharacterIcon;
        categoryText.text = variant.Name;
    }

    public void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        replacementSpawner.SetCharacter(variant.CivilianReference.AssetGUID);
    }
}
