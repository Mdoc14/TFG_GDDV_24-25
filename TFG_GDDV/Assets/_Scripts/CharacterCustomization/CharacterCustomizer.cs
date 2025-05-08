using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CustomizationOptions
{
    public List<GameObject> hats;
    public List<GameObject> headphones;
    public List<GameObject> hair;
    public List<GameObject> shirts;
    public List<GameObject> pants;
    public List<GameObject> shoes;
    public List<GameObject> watches;

    [HideInInspector]
    public int currentHat = -1;
    [HideInInspector]
    public int currentHeadphone = -1;
    [HideInInspector]
    public int currentHair = -1;
    [HideInInspector]
    public int currentShirt = -1;
    [HideInInspector]
    public int currentPants = -1;
    [HideInInspector]
    public int currentShoes = -1;
    [HideInInspector]
    public int currentWatch = -1;
    [HideInInspector]


    // Opcionales por sexo
    public List<GameObject> beards;       // Solo male
    public List<GameObject> eyebrows;     // Male/Female
    public List<GameObject> glasses;
    public List<GameObject> mask;
    public List<GameObject> earrings;     // Solo female
    public List<GameObject> rightHands;        // Solo female
    public List<GameObject> leftHands;        // Solo female

    [HideInInspector]
    public int currentBeard = -1;
    [HideInInspector]
    public int currentEyebrow = -1;
    [HideInInspector]
    public int currentGlasses = -1;
    [HideInInspector]
    public int currentMask = -1;
    [HideInInspector]
    public int currentEarring = -1;
    [HideInInspector]
    public int currentRightHand = -1;
    [HideInInspector]
    public int currentLeftHand = -1;
}

public class CharacterCustomizer : MonoBehaviour
{
    [Header("Non-custom character face Options")]
    public CustomizationOptions maleOptions;
    public CustomizationOptions femaleOptions;

    [Header("Custom character face Options")]
    public CustomizationOptions maleCustomOptions;
    public CustomizationOptions femaleCustomOptions;

    private void Start()
    {
        InitializeCustomizationOptions(maleOptions);
        InitializeCustomizationOptions(femaleOptions);
        InitializeCustomizationOptions(maleCustomOptions);
        InitializeCustomizationOptions(femaleCustomOptions);
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
        if (optionsList != null)
        {
            foreach (var currentOption in optionsList)
            {
                if (currentOption.activeSelf)
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

    public void ActivateNewObject(List<GameObject> categoryList, ref int currentIndex, int newIndex)
    {
        if (categoryList == null || categoryList.Count == 0) return;

        // Desactivar el objeto actual si hay uno activo
        if (currentIndex >= 0 && currentIndex < categoryList.Count && categoryList[currentIndex] != null)
        {
            categoryList[currentIndex].SetActive(false);
        }

        // Activar el nuevo objeto si es válido
        if (newIndex >= 0 && newIndex < categoryList.Count && categoryList[newIndex] != null)
        {
            categoryList[newIndex].SetActive(true);
            currentIndex = newIndex;
        }
        else
        {
            currentIndex = -1; // El índice nuevo no era válido
        }
    }

    public void ActivateObjectByCategory(CustomizationOptions customization, string categoryName, int newIndex)
    {
        switch (categoryName.ToLower())
        {
            case "hat":
                ActivateNewObject(customization.hats, ref customization.currentHat, newIndex);
                break;
            case "headphone":
                ActivateNewObject(customization.headphones, ref customization.currentHeadphone, newIndex);
                break;
            case "hair":
                ActivateNewObject(customization.hair, ref customization.currentHair, newIndex);
                break;
            case "shirt":
                ActivateNewObject(customization.shirts, ref customization.currentShirt, newIndex);
                break;
            case "pants":
                ActivateNewObject(customization.pants, ref customization.currentPants, newIndex);
                break;
            case "shoes":
                ActivateNewObject(customization.shoes, ref customization.currentShoes, newIndex);
                break;
            case "watch":
                ActivateNewObject(customization.watches, ref customization.currentWatch, newIndex);
                break;
            case "beard":
                ActivateNewObject(customization.beards, ref customization.currentBeard, newIndex);
                break;
            case "eyebrow":
                ActivateNewObject(customization.eyebrows, ref customization.currentEyebrow, newIndex);
                break;
            case "glasses":
                ActivateNewObject(customization.glasses, ref customization.currentGlasses, newIndex);
                break;
            case "mask":
                ActivateNewObject(customization.mask, ref customization.currentMask, newIndex);
                break;
            case "earring":
                ActivateNewObject(customization.earrings, ref customization.currentEarring, newIndex);
                break;
            case "righthand":
                ActivateNewObject(customization.rightHands, ref customization.currentRightHand, newIndex);
                break;
            case "lefthand":
                ActivateNewObject(customization.leftHands, ref customization.currentLeftHand, newIndex);
                break;
            default:
                Debug.LogWarning($"Unknown category: {categoryName}");
                break;
        }
    }

    public void SelectCustomizationOption(string characterType, string categoryName, int index)
    {
        CustomizationOptions options = GetCustomizationOptions(characterType.ToLower());
        if (options == null)
        {
            Debug.LogWarning($"Unknown character type: {characterType}");
            return;
        }

        ActivateObjectByCategory(options, categoryName, index);
    }

    private CustomizationOptions GetCustomizationOptions(string characterType)
    {
        return characterType switch
        {
            "male" => maleOptions,
            "female" => femaleOptions,
            "malecustom" => maleCustomOptions,
            "femalecustom" => femaleCustomOptions,
            _ => null,
        };
    }


}
