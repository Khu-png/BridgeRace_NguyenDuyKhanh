using TMPro;
using UnityEngine;

public class LevelText : UICanvas
{
    private TextMeshProUGUI levelText;

    protected override void OnInit()
    {
        base.OnInit();
        levelText = GetComponent<TextMeshProUGUI>();
    }

    public void SetLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }
    }
}
