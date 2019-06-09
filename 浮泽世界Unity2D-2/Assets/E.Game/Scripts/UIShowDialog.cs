// Instantiates a tooltip while the cursor is over this UI element.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIShowDialog : MonoBehaviour
{
    public GameObject tooltipPrefab;
    [TextArea(1, 30)] public string text = "";

    // instantiated tooltip
    [SerializeField] GameObject current;

    void CreateToolTip()
    {
        // instantiate
        current = Instantiate(tooltipPrefab, transform.position, Quaternion.identity);

        // put to foreground
        //current.transform.SetParent(transform.root, true); // canvas
        current.transform.SetAsLastSibling(); // last one means foreground
    }

    void ShowToolTip(float delay)
    {
        Invoke(nameof(CreateToolTip), delay);
    }

    void DestroyToolTip()
    {
        // stop any running attempts to show it
        CancelInvoke(nameof(CreateToolTip));

        // destroy it
        Destroy(current);
    }

    private void OnMouseEnter()
    {
        ShowToolTip(0.5f);
    }
    private void OnMouseExit()
    {
        //DestroyToolTip();
    }

    void Update()
    {
        if (current) current.GetComponentInChildren<Text>().text = text;
    }

    void OnDisable()
    {
        DestroyToolTip();
    }

    void OnDestroy()
    {
        DestroyToolTip();
    }
}
