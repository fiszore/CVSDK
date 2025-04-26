using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Events;

public class ReplacementOption : MonoBehaviour
{
    public LocalizedString localizedName;
    [SerializeField] private GameObject dropdown;

    private string selectionGUID;
    private VariantSelectMethod selectMethod;
    private GameObject dropdownObject;
    bool initialized = false;

    public void SetCharacter(string selectionGUID)
    {
        this.selectionGUID = selectionGUID;
        selectMethod = CharacterLibrary.Instance.GetSelectionMethod(selectionGUID);
        if (!initialized)
            CreateDropDowns();
        else
            UpdateDropDowns();
    }

    public void CreateDropDowns()
    {
        initialized = true;

        dropdownObject = GameObject.Instantiate(dropdown, Vector3.zero, Quaternion.identity);
        dropdownObject.transform.SetParent(this.transform);
        dropdownObject.transform.localScale = Vector3.one;
        foreach (TMP_Text t in dropdownObject.GetComponentsInChildren<TMP_Text>())
        {
            if (t.name == "Label")
            {
                //t.text = o.type.ToString();
                t.text = localizedName.GetLocalizedString();
                t.GetComponent<LocalizeStringEvent>().StringReference = localizedName;
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
}
