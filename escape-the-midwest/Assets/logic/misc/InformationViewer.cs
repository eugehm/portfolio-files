using UnityEngine;
using UnityEngine.UI;

public class InformationViewer : MonoBehaviour
{
    static InformationViewer instance;

    CanvasGroup cg;

    [SerializeField] Text text;
    [SerializeField] Button ok_button;

    public static bool IsVisible()
    {
        if (instance == null)
            return false;
        return instance.cg.alpha > 0.0f;
    }

    void Start()
    {
        instance = this;

        cg = GetComponent<CanvasGroup>();
        ok_button.onClick.AddListener(OnClick);
        Hide();
    }

    public static void Show(string s)
    {
        instance.cg.alpha = 1.0f;
        instance.cg.interactable = true;
        instance.cg.blocksRaycasts = true;

        instance.text.text = s;
    }

    void Hide()
    {
        cg.alpha = 0.0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    void OnClick()
    {
        Hide();
    }

    void Update()
    {
        
    }
}
