using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpScreenBehaviour : MonoBehaviour
{
    public TextMeshProUGUI paginationText;
    public GameObject[] pages;
    public Button backButton;
    public Button nextButton;
    public int pageIndex = 0;
    public int totalPages = 5; 

    public void IncrementPageIndex()
    {
        pageIndex++;
        if (pageIndex >= totalPages)
        {
            pageIndex = 0; 
        }
        ShowPage(pageIndex);
    }
    public void DecrementPageIndex()
    {
        pageIndex--;
        if (pageIndex < 0)
        {
            pageIndex = totalPages - 1; 
        }
        ShowPage(pageIndex);
    }
    private void UpdatePaginationText()
    {
        paginationText.text = $"{pageIndex + 1} / {totalPages}";
    }
    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }
        UpdatePaginationText();
    }
    private void Start()
    {
        ShowPage(pageIndex);
    }
}
