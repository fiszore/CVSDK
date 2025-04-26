using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ReplacementPanel : MonoBehaviour
{
    [SerializeField] private Image categoryImage;
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private Image checkBox;
    private Variant civilian;

    public void SetCharacter(Variant civilian)
    {
        this.civilian = civilian;
        categoryImage.sprite = civilian.CharacterIcon;
        categoryText.text = civilian.Name;
        checkBox.enabled = civilian.Enabled;
    }

    public void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        civilian.ToggleVariant();
        checkBox.enabled = civilian.Enabled;
    }
}
