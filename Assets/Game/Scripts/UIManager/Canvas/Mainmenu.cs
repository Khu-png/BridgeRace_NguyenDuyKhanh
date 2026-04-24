using UnityEngine;

public class Mainmenu : UICanvas
{
    public void OnClickStart()
    {
        GameManager.Instance?.GameStart();
    }

    public void OnClickSetting()
    {
        UIManager.Instance?.OpenUI<Setting>();
    }
}
