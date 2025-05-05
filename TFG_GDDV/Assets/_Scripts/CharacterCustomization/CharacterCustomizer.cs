using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomizer : MonoBehaviour
{
    [Header("Male Customization Options")]
    [Tooltip("Hair Options")]
    public List<GameObject> maleHats;
    
    [Tooltip("Hair Options")]
    public List<GameObject> maleHairs;

    [Tooltip("Shirt Options")]
    public List<GameObject> maleShirts;

    [Tooltip("Pants Options")]
    public List<GameObject> malePants;

    [Tooltip("Accessories")]
    public List<GameObject> maleAccessories;
}
