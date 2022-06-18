using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipStatusManager : MonoBehaviour
{
    [SerializeField] private RessourceSystemManager sysMngr;
    [SerializeField] private Image lifeBarFill;
    [SerializeField] private TextMeshProUGUI shipStatusComment;
 
    private float lifeBarInitialWidth = 0f;

    private void Awake()
    {
        lifeBarInitialWidth = lifeBarFill.GetComponent<RectTransform>().rect.width;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var hpRatio = sysMngr.HP.current / sysMngr.HP.max;
        var newbarWidth = hpRatio * lifeBarInitialWidth;
        lifeBarFill.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newbarWidth);
        lifeBarFill.color = Color.Lerp(Color.red, Color.blue, hpRatio);
        shipStatusComment.text = GetCurrentStatusComment(hpRatio);
    }

    private string GetCurrentStatusComment(float hpRatio)
    {
        if (hpRatio >= 0.5) return "Nominal";
        if (hpRatio >= 0.3) return "Needs Attention";
        return "CRITICAL";
    }
}
