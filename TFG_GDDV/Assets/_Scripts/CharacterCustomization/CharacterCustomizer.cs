using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CustomizationOptions
{
    [HideInInspector]
    public string modelType;
    
    public GameObject modelGameObject;

    public List<GameObject> hats;
    public List<GameObject> headphones;
    public List<GameObject> hair;
    public List<GameObject> shirts;
    public List<GameObject> pants;
    public List<GameObject> shoes;
    public List<GameObject> watches;
    public List<GameObject> beards;
    public List<GameObject> eyebrows;
    public List<GameObject> glasses;
    public List<GameObject> mask;
    public List<GameObject> earrings;
    public List<GameObject> rightHands;
    public List<GameObject> leftHands;

    [HideInInspector] public int currentHat = -1;
    [HideInInspector] public int currentHeadphone = -1;
    [HideInInspector] public int currentHair = -1;
    [HideInInspector] public int currentShirt = -1;
    [HideInInspector] public int currentPants = -1;
    [HideInInspector] public int currentShoes = -1;
    [HideInInspector] public int currentWatch = -1;
    [HideInInspector] public int currentBeard = -1;
    [HideInInspector] public int currentEyebrow = -1;
    [HideInInspector] public int currentGlasses = -1;
    [HideInInspector] public int currentMask = -1;
    [HideInInspector] public int currentEarring = -1;
    [HideInInspector] public int currentRightHand = -1;
    [HideInInspector] public int currentLeftHand = -1;
}

public class CharacterCustomizer : MonoBehaviour
{

    public RectTransform customizationUI;
    private bool modelIsSet = false;

    [Header("Container of buttons")]
    public GameObject categoriesContent;
    public GameObject partsContent;

    [Header("Button prefabs")]
    public GameObject categoryButtonPrefab;
    public GameObject partButtonPrefab;

    [Header("Customization Data")]
    public CustomizationOptions maleOptions;
    public CustomizationOptions femaleOptions;
    public CustomizationOptions maleCustomOptions;
    public CustomizationOptions femaleCustomOptions;

    [Header("Default sprite (empty option)")]
    public Sprite emptySprite;

    private CustomizationOptions currentOptions;
    private List<Button> partsButtons;
    private int activePartIndex = -1;

    private void Update()
    {
        if(customizationUI.gameObject.activeSelf && !modelIsSet)
        {
            InitializeCustomizationOptions(maleOptions);
            InitializeCustomizationOptions(femaleOptions);
            InitializeCustomizationOptions(maleCustomOptions);
            InitializeCustomizationOptions(femaleCustomOptions);

            maleOptions.modelType = "male";
            femaleOptions.modelType = "female";
            maleCustomOptions.modelType = "customMale";
            femaleCustomOptions.modelType = "customFemale";

            currentOptions = maleOptions;
            SetModelType("male");
            modelIsSet = true;
        }
    }

    private void InitializeCustomizationOptions(CustomizationOptions options)
    {
        options.currentHat = GetActiveIndex(options.hats);
        options.currentHeadphone = GetActiveIndex(options.headphones);
        options.currentHair = GetActiveIndex(options.hair);
        options.currentShirt = GetActiveIndex(options.shirts);
        options.currentPants = GetActiveIndex(options.pants);
        options.currentShoes = GetActiveIndex(options.shoes);
        options.currentWatch = GetActiveIndex(options.watches);
        options.currentBeard = GetActiveIndex(options.beards);
        options.currentEyebrow = GetActiveIndex(options.eyebrows);
        options.currentGlasses = GetActiveIndex(options.glasses);
        options.currentMask = GetActiveIndex(options.mask);
        options.currentEarring = GetActiveIndex(options.earrings);
        options.currentRightHand = GetActiveIndex(options.rightHands);
        options.currentLeftHand = GetActiveIndex(options.leftHands);
    }

    private int GetActiveIndex(List<GameObject> optionsList)
    {
        if (optionsList.Count != 0)
        {
            foreach (GameObject currentOption in optionsList)
            {

                if (currentOption != null && currentOption.activeSelf) 
                {
                    return optionsList.IndexOf(currentOption);
                }
            }
            return -1; // La lista tiene opciones, pero ninguna está activa  
        }
        else
        {
            return -2; // La lista no tiene opciones, se descarta  
        }
    }


    public void SetModelType(string modelType)
    {
        ClearUI(categoriesContent);
        ClearUI(partsContent);

        switch (modelType)
        {
            case "male":
                SetCurrentoptions(maleOptions);
                break;
            case "female":
                SetCurrentoptions(femaleOptions);
                break;
            case "customMale":
                SetCurrentoptions(maleCustomOptions);
                break;
            case "customFemale":
                SetCurrentoptions(femaleCustomOptions);
                break;
        }

        GenerateCategoryButtons(currentOptions);
    }

    public void SetCurrentoptions(CustomizationOptions newOptions)
    {
        currentOptions.modelGameObject.SetActive(false);
        currentOptions = newOptions;
        currentOptions.modelGameObject.SetActive(true);
    }

    private void ClearUI(GameObject content)
    {
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);
    }
    private void GenerateCategoryButtons(CustomizationOptions options)
    {
        Dictionary<string, (List<GameObject> list, int index)> categoryMap = new()
    {
        { "Sombreros", (options.hats, options.currentHat) },
        { "Cascos", (options.headphones, options.currentHeadphone) },
        { "Pelo", (options.hair, options.currentHair) },
        { "Torsos", (options.shirts, options.currentShirt) },
        { "Piernas", (options.pants, options.currentPants) },
        { "Calzados", (options.shoes, options.currentShoes) },
        { "Relojes", (options.watches, options.currentWatch) },
        { "Barbas", (options.beards, options.currentBeard) },
        { "Cejas", (options.eyebrows, options.currentEyebrow) },
        { "Gafas", (options.glasses, options.currentGlasses) },
        { "Máscaras", (options.mask, options.currentMask) },
        { "Pendientes", (options.earrings, options.currentEarring) },
        { "Mano derecha", (options.rightHands, options.currentRightHand) },
        { "Mano izquierda", (options.leftHands, options.currentLeftHand) }
    };

        foreach (var pair in categoryMap)
        {
            if (pair.Value.index != -2) // Solo si esa categoría está disponible en este modelo
            {
                GameObject btn = Instantiate(categoryButtonPrefab, categoriesContent.transform);
                btn.GetComponentInChildren<TMP_Text>().text = pair.Key;

                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GeneratePartButtons(pair.Value.list);
                });
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(categoriesContent.GetComponent<RectTransform>());
    }

    private void GeneratePartButtons(List<GameObject> parts)
    {
        ClearUI(partsContent);
        if (partsButtons == null) partsButtons = new List<Button>();
        else partsButtons.Clear();

        activePartIndex = GetActiveIndex(parts);

        // ¿La categoría tiene opción de "ninguno"?
        bool allowsEmpty = CategoryAllowsEmptyOption(parts);

        // Si se permite la opción de quitar, añadir botón "X"
        if (allowsEmpty)
        {
            GameObject emptyBtn = Instantiate(partButtonPrefab, partsContent.transform);
            Button emptyButtonComp = emptyBtn.GetComponent<Button>();
            partsButtons.Add(emptyButtonComp);
            emptyBtn.transform.GetChild(0).GetComponent<Image>().sprite = emptySprite;

            emptyButtonComp.interactable = (activePartIndex != -1);

            emptyButtonComp.onClick.AddListener(() =>
            {
                if (activePartIndex >= 0 && activePartIndex < parts.Count)
                {
                    parts[activePartIndex].SetActive(false);
                    partsButtons[activePartIndex + 1].interactable = true; // +1 por el botón "X"
                }
                activePartIndex = -1;
                emptyButtonComp.interactable = false;
            });
        }

        for (int i = 0; i < parts.Count; i++)
        {
            GameObject part = parts[i];
            GameObject btn = Instantiate(partButtonPrefab, partsContent.transform);
            Button buttonComponent = btn.GetComponent<Button>();
            partsButtons.Add(buttonComponent);

            SpritePart sp = part.GetComponent<SpritePart>();
            if (sp != null)
                btn.transform.GetChild(0).GetComponent<Image>().sprite = sp.sprite;

            int logicalIndex = allowsEmpty ? i + 1 : i;
            if (i == activePartIndex)
                buttonComponent.interactable = false;

            int capturedIndex = i;
            buttonComponent.onClick.AddListener(() =>
            {
                ChangePart(parts, capturedIndex);
            });
        }
    }

    private void ChangePart(List<GameObject> list, int indexToActivate)
    {
        bool allowsEmpty = CategoryAllowsEmptyOption(list);
        int offset = allowsEmpty ? 1 : 0;

        if (activePartIndex >= 0 && activePartIndex < list.Count)
        {
            partsButtons[activePartIndex + offset].interactable = true;
            list[activePartIndex].SetActive(false);
        }

        list[indexToActivate].SetActive(true);
        partsButtons[indexToActivate + offset].interactable = false;

        if (allowsEmpty)
            partsButtons[0].interactable = true;

        activePartIndex = indexToActivate;

    }

    private bool CategoryAllowsEmptyOption(List<GameObject> parts)
    {
        // Solo categorías opcionales permiten desactivarse
        return parts == currentOptions.hats ||
               parts == currentOptions.headphones ||
               parts == currentOptions.hair ||
               parts == currentOptions.watches ||
               parts == currentOptions.beards ||
               parts == currentOptions.eyebrows ||
               parts == currentOptions.glasses ||
               parts == currentOptions.mask ||
               parts == currentOptions.earrings;
    }



}
