using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.Events;

public class ReplacementOption : MonoBehaviour
{
    public LocalizedString localizedTitle;
    public LocalizedString localizedEnableAll;
    public LocalizedString localizedDisableAll;
    [SerializeField] private ReplacementSpawner replacementMenu;
    [SerializeField] private GameObject dropdown;
    [SerializeField] private GameObject buttonPrefab; 

    private string selectionGUID;
    private VariantSelectMethod selectMethod;
    private GameObject dropdownObject;
    bool initialized = false;

    public void SetCharacter(string selectionGUID)
    {
        this.selectionGUID = selectionGUID;
        selectMethod = CharacterLibrary.Instance.GetSelectionMethod(selectionGUID);
        if (!initialized)
        {
            initialized = true;
            CreateDropDowns();
            CreateButtons();
        }
        else
            UpdateDropDowns();
    }

    public void CreateDropDowns()
    {
        dropdownObject = GameObject.Instantiate(dropdown, Vector3.zero, Quaternion.identity);
        dropdownObject.transform.SetParent(this.transform);
        dropdownObject.transform.localScale = Vector3.one;
        foreach (TMP_Text t in dropdownObject.GetComponentsInChildren<TMP_Text>())
        {
            if (t.name == "Label")
            {
                //t.text = o.type.ToString();
                t.text = localizedTitle.GetLocalizedString();
                t.GetComponent<LocalizeStringEvent>().StringReference = localizedTitle;
            }
        }
        List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
        foreach (string str in Enum.GetNames(typeof(VariantSelectMethod)))
        {
            data.Add(new TMP_Dropdown.OptionData(str));
        }
        TMP_Dropdown drop = dropdownObject.GetComponentInChildren<TMP_Dropdown>();
        drop.options = data;
        drop.onValueChanged.AddListener(SetValue);
        UpdateDropDowns();
    }

    public void CreateButtons()
    {
        GameObject enableButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
        enableButton.transform.SetParent(transform);
        enableButton.transform.localScale = Vector3.one;

        TMP_Text enableText = enableButton.GetComponentInChildren<TMP_Text>();
        enableText.text = localizedEnableAll.GetLocalizedString();
        enableText.GetComponent<LocalizeStringEvent>().StringReference = localizedEnableAll;

        enableButton.GetComponent<Button>().onClick.AddListener(() => {
            ToggleAll(true);
        });

        GameObject disableButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
        disableButton.transform.SetParent(transform);
        disableButton.transform.localScale = Vector3.one;

        TMP_Text disableText = disableButton.GetComponentInChildren<TMP_Text>();
        disableText.text = localizedDisableAll.GetLocalizedString();
        disableText.GetComponent<LocalizeStringEvent>().StringReference = localizedDisableAll;

        disableButton.GetComponent<Button>().onClick.AddListener(() => {
            ToggleAll(false);
        });
    }

    private void UpdateDropDowns()
    {
        TMP_Dropdown drop = dropdownObject.GetComponentInChildren<TMP_Dropdown>();
        int value = (int)selectMethod;
        drop.value = Mathf.RoundToInt(value);
        drop.SetValueWithoutNotify(value);
    }

    public void SetValue(int value)
    {
        TMP_Dropdown drop = dropdownObject.GetComponentInChildren<TMP_Dropdown>();
        drop.SetValueWithoutNotify(value);
        CharacterLibrary.Instance.SetSelectionMethod(selectionGUID, (VariantSelectMethod)value);
    }

    public void ToggleAll(bool on)
    {
        foreach (var variant in CharacterLibrary.Instance.GetAllVariants(selectionGUID))
            variant.ToggleVariant(on);

        replacementMenu.RefreshPanels();
    }
}
