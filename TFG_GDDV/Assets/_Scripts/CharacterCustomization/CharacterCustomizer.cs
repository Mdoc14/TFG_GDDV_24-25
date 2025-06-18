using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
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

public class CharacterCustomizer : NetworkBehaviour
{

    public RectTransform customizationUI;

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

    public CustomizationOptions currentOptions;
    private List<Button> partsButtons;
    private int activePartIndex = -1;

    private NetworkVariable<CharacterData> maleData = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<CharacterData> femaleData = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<CharacterData> maleCustomData = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<CharacterData> femaleCustomData = new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsClient)
        {
            maleData.OnValueChanged += OnMaleDataChanged;
            femaleData.OnValueChanged += OnFemaleDataChanged;
            maleCustomData.OnValueChanged += OnMaleCustomDataChanged;
            femaleCustomData.OnValueChanged += OnFemaleCustomDataChanged;
        }

        // Esto es nuevo:
        ApplyCharacterData(maleOptions, maleData.Value);
        ApplyCharacterData(femaleOptions, femaleData.Value);
        ApplyCharacterData(maleCustomOptions, maleCustomData.Value);
        ApplyCharacterData(femaleCustomOptions, femaleCustomData.Value);

        // Esto solo si eres owner
        if (IsOwner)
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

            LoadAllCustomization();
            SubmitCustomizationServerRpc(
                ToCharacterData(maleOptions),
                ToCharacterData(femaleOptions),
                ToCharacterData(maleCustomOptions),
                ToCharacterData(femaleCustomOptions)
            );

            SetModelClientRPC(currentOptions.modelType);
        }
    }


    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Cargamos la personalización una vez conectado
            LoadAllCustomization();

            // Ya no hace falta el callback
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
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

        if (!PlayerPrefs.HasKey($"{options.modelType}_hat"))
        {
            SaveCustomization(options);
        }
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
                SetCurrentOptions(maleOptions);
                break;
            case "female":
                SetCurrentOptions(femaleOptions);
                break;
            case "customMale":
                SetCurrentOptions(maleCustomOptions);
                break;
            case "customFemale":
                SetCurrentOptions(femaleCustomOptions);
                break;
        }

        GenerateCategoryButtons(currentOptions);
    }

    public void SetCurrentOptions(CustomizationOptions newOptions)
    {
        if (currentOptions != null && currentOptions.modelGameObject != null)
            SetLayerRecursively(currentOptions.modelGameObject, LayerMask.NameToLayer("HiddenCharacter"));

        currentOptions = newOptions;

        if (currentOptions != null && currentOptions.modelGameObject != null)
            SetLayerRecursively(currentOptions.modelGameObject, LayerMask.NameToLayer("CharacterPreview"));

        ActivateNetworkAnimatorForModel(currentOptions.modelGameObject);
        GetComponent<HostBehaviour>().animator = currentOptions.modelGameObject.GetComponent<Animator>();
    }
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child != null)
                SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    public void ActivateNetworkAnimatorForModel(GameObject modelGO)
    {
        var modelAnimator = modelGO.GetComponent<Animator>();
        if (modelAnimator == null)
        {
            Debug.LogWarning("Model does not have an Animator component.");
            return;
        }

        // Encuentra todos los NetworkAnimators del HostPlayer
        foreach (var netAnim in GetComponents<NetworkAnimator>())
        {
            // Compara la referencia del Animator que tiene asignado
            if (netAnim.Animator == modelAnimator)
            {
                netAnim.enabled = true;
            }
            else
            {
                netAnim.enabled = false;
            }
        }
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
                    partsButtons[activePartIndex + 1].interactable = true;
                }

                activePartIndex = -1;
                emptyButtonComp.interactable = false;

                // Añade esto para actualizar el índice en currentOptions
                if (parts == currentOptions.hats) currentOptions.currentHat = -1;
                else if (parts == currentOptions.headphones) currentOptions.currentHeadphone = -1;
                else if (parts == currentOptions.hair) currentOptions.currentHair = -1;
                else if (parts == currentOptions.shirts) currentOptions.currentShirt = -1;
                else if (parts == currentOptions.pants) currentOptions.currentPants = -1;
                else if (parts == currentOptions.shoes) currentOptions.currentShoes = -1;
                else if (parts == currentOptions.watches) currentOptions.currentWatch = -1;
                else if (parts == currentOptions.beards) currentOptions.currentBeard = -1;
                else if (parts == currentOptions.eyebrows) currentOptions.currentEyebrow = -1;
                else if (parts == currentOptions.glasses) currentOptions.currentGlasses = -1;
                else if (parts == currentOptions.mask) currentOptions.currentMask = -1;
                else if (parts == currentOptions.earrings) currentOptions.currentEarring = -1;
                else if (parts == currentOptions.rightHands) currentOptions.currentRightHand = -1;
                else if (parts == currentOptions.leftHands) currentOptions.currentLeftHand = -1;
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

        // Actualiza el índice en currentOptions 
        if (list == currentOptions.hats) currentOptions.currentHat = indexToActivate;
        else if (list == currentOptions.headphones) currentOptions.currentHeadphone = indexToActivate;
        else if (list == currentOptions.hair) currentOptions.currentHair = indexToActivate;
        else if (list == currentOptions.shirts) currentOptions.currentShirt = indexToActivate;
        else if (list == currentOptions.pants) currentOptions.currentPants = indexToActivate;
        else if (list == currentOptions.shoes) currentOptions.currentShoes = indexToActivate;
        else if (list == currentOptions.watches) currentOptions.currentWatch = indexToActivate;
        else if (list == currentOptions.beards) currentOptions.currentBeard = indexToActivate;
        else if (list == currentOptions.eyebrows) currentOptions.currentEyebrow = indexToActivate;
        else if (list == currentOptions.glasses) currentOptions.currentGlasses = indexToActivate;
        else if (list == currentOptions.mask) currentOptions.currentMask = indexToActivate;
        else if (list == currentOptions.earrings) currentOptions.currentEarring = indexToActivate;
        else if (list == currentOptions.rightHands) currentOptions.currentRightHand = indexToActivate;
        else if (list == currentOptions.leftHands) currentOptions.currentLeftHand = indexToActivate;


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

    public void SaveAllCustomization()
    {
        Debug.Log("Saving customization...");
        SaveCustomization(maleOptions);
        SaveCustomization(femaleOptions);
        SaveCustomization(maleCustomOptions);
        SaveCustomization(femaleCustomOptions);
        SubmitCustomizationServerRpc(ToCharacterData(maleOptions), ToCharacterData(femaleOptions), ToCharacterData(maleCustomOptions), ToCharacterData(femaleCustomOptions));
        SetModelClientRPC(currentOptions.modelType);
    }

    public void SaveCustomization(CustomizationOptions options)
    {
        string prefix = options.modelType;

        PlayerPrefs.SetInt($"{prefix}_hat", options.currentHat);
        PlayerPrefs.SetInt($"{prefix}_headphone", options.currentHeadphone);
        PlayerPrefs.SetInt($"{prefix}_hair", options.currentHair);
        PlayerPrefs.SetInt($"{prefix}_shirt", options.currentShirt);
        PlayerPrefs.SetInt($"{prefix}_pants", options.currentPants);
        PlayerPrefs.SetInt($"{prefix}_shoes", options.currentShoes);
        PlayerPrefs.SetInt($"{prefix}_watch", options.currentWatch);
        PlayerPrefs.SetInt($"{prefix}_beard", options.currentBeard);
        PlayerPrefs.SetInt($"{prefix}_eyebrow", options.currentEyebrow);
        PlayerPrefs.SetInt($"{prefix}_glasses", options.currentGlasses);
        PlayerPrefs.SetInt($"{prefix}_mask", options.currentMask);
        PlayerPrefs.SetInt($"{prefix}_earring", options.currentEarring);
        PlayerPrefs.SetInt($"{prefix}_rightHand", options.currentRightHand);
        PlayerPrefs.SetInt($"{prefix}_leftHand", options.currentLeftHand);


        PlayerPrefs.Save();
    }

    public void LoadAllCustomization()
    {
        Debug.Log("Loading customization...");
        LoadCustomization(maleOptions);
        LoadCustomization(femaleOptions);
        LoadCustomization(maleCustomOptions);
        LoadCustomization(femaleCustomOptions);
        SubmitCustomizationServerRpc(ToCharacterData(maleOptions), ToCharacterData(femaleOptions), ToCharacterData(maleCustomOptions), ToCharacterData(femaleCustomOptions));
        SetModelClientRPC(currentOptions.modelType);
    }

    public void LoadCustomization(CustomizationOptions options)
    {
        string prefix = options.modelType;

        options.currentHat = PlayerPrefs.GetInt($"{prefix}_hat", -1);
        options.currentHeadphone = PlayerPrefs.GetInt($"{prefix}_headphone", -1);
        options.currentHair = PlayerPrefs.GetInt($"{prefix}_hair", -1);
        options.currentShirt = PlayerPrefs.GetInt($"{prefix}_shirt", 0);
        options.currentPants = PlayerPrefs.GetInt($"{prefix}_pants", 0);
        options.currentShoes = PlayerPrefs.GetInt($"{prefix}_shoes", 0);
        options.currentWatch = PlayerPrefs.GetInt($"{prefix}_watch", -1);
        options.currentBeard = PlayerPrefs.GetInt($"{prefix}_beard", -1);
        options.currentEyebrow = PlayerPrefs.GetInt($"{prefix}_eyebrow", -1);
        options.currentGlasses = PlayerPrefs.GetInt($"{prefix}_glasses", -1);
        options.currentMask = PlayerPrefs.GetInt($"{prefix}_mask", -1);
        options.currentEarring = PlayerPrefs.GetInt($"{prefix}_earring", -1);
        options.currentRightHand = PlayerPrefs.GetInt($"{prefix}_rightHand", 0);
        options.currentLeftHand = PlayerPrefs.GetInt($"{prefix}_leftHand", 0);

        ApplyCustomization(options);

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

    public void ApplyCustomization(CustomizationOptions options)
    {
        ApplyPart(options.hats, options.currentHat);
        ApplyPart(options.headphones, options.currentHeadphone);
        ApplyPart(options.hair, options.currentHair);
        ApplyPart(options.shirts, options.currentShirt);
        ApplyPart(options.pants, options.currentPants);
        ApplyPart(options.shoes, options.currentShoes);
        ApplyPart(options.watches, options.currentWatch);
        ApplyPart(options.beards, options.currentBeard);
        ApplyPart(options.eyebrows, options.currentEyebrow);
        ApplyPart(options.glasses, options.currentGlasses);
        ApplyPart(options.mask, options.currentMask);
        ApplyPart(options.earrings, options.currentEarring);
        ApplyPart(options.rightHands, options.currentRightHand);
        ApplyPart(options.leftHands, options.currentLeftHand);
    }



    private void ApplyPart(List<GameObject> list, int index)
    {
        for (int i = 0; i < list.Count; i++)
            list[i].SetActive(i == index);
    }

    //////////////////////////////
    /// PARTES DE NETCODE
    //////////////////////////////

    private void OnMaleDataChanged(CharacterData previous, CharacterData current)
    {
        ApplyCharacterData(maleOptions, current);
    }
    private void OnFemaleDataChanged(CharacterData previous, CharacterData current)
    {
        ApplyCharacterData(femaleOptions, current);
    }
    private void OnMaleCustomDataChanged(CharacterData previous, CharacterData current)
    {
        ApplyCharacterData(maleCustomOptions, current);
    }
    private void OnFemaleCustomDataChanged(CharacterData previous, CharacterData current)
    {
        ApplyCharacterData(femaleCustomOptions, current);
    }

    private CharacterData ToCharacterData(CustomizationOptions options)
    {
        return new CharacterData
        {
            hat = options.currentHat,
            headphone = options.currentHeadphone,
            hair = options.currentHair,
            shirt = options.currentShirt,
            pants = options.currentPants,
            shoes = options.currentShoes,
            watch = options.currentWatch,
            beard = options.currentBeard,
            eyebrow = options.currentEyebrow,
            glasses = options.currentGlasses,
            mask = options.currentMask,
            earring = options.currentEarring,
            rightHand = options.currentRightHand,
            leftHand = options.currentLeftHand
        };
    }

    private void ApplyCharacterData(CustomizationOptions options, CharacterData data)
    {
        options.currentHat = data.hat;
        options.currentHeadphone = data.headphone;
        options.currentHair = data.hair;
        options.currentShirt = data.shirt;
        options.currentPants = data.pants;
        options.currentShoes = data.shoes;
        options.currentWatch = data.watch;
        options.currentBeard = data.beard;
        options.currentEyebrow = data.eyebrow;
        options.currentGlasses = data.glasses;
        options.currentMask = data.mask;
        options.currentEarring = data.earring;
        options.currentRightHand = data.rightHand;
        options.currentLeftHand = data.leftHand;

        Debug.Log($"Applying customization: {options.modelType}");
        Debug.Log($"Hat: {options.currentHat}, Headphone: {options.currentHeadphone}, Hair: {options.currentHair}, Shirt: {options.currentShirt}, Pants: {options.currentPants}, Shoes: {options.currentShoes}, Watch: {options.currentWatch}, Beard: {options.currentBeard}, Eyebrow: {options.currentEyebrow}, Glasses: {options.currentGlasses}, Mask: {options.currentMask}, Earring: {options.currentEarring}, RightHand: {options.currentRightHand}, LeftHand: {options.currentLeftHand}");
        ApplyCustomization(options);

    }

    [ServerRpc]
    public void SubmitCustomizationServerRpc(CharacterData _maleData, CharacterData _femaleData, CharacterData _maleCustomData, CharacterData _femaleCustomData)
    {
        maleData.Value = _maleData;
        femaleData.Value = _femaleData;
        maleCustomData.Value = _maleCustomData;
        femaleCustomData.Value = _femaleCustomData;
    }

    [ClientRpc]
    public void SetModelClientRPC(string modelType)
    {
        SetModelType(modelType);
    }

}

public struct CharacterData : INetworkSerializable
{
    public int hat, headphone, hair, shirt, pants, shoes, watch, beard, eyebrow, glasses, mask, earring, rightHand, leftHand;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hat);
        serializer.SerializeValue(ref headphone);
        serializer.SerializeValue(ref hair);
        serializer.SerializeValue(ref shirt);
        serializer.SerializeValue(ref pants);
        serializer.SerializeValue(ref shoes);
        serializer.SerializeValue(ref watch);
        serializer.SerializeValue(ref beard);
        serializer.SerializeValue(ref eyebrow);
        serializer.SerializeValue(ref glasses);
        serializer.SerializeValue(ref mask);
        serializer.SerializeValue(ref earring);
        serializer.SerializeValue(ref rightHand);
        serializer.SerializeValue(ref leftHand);
    }
}

