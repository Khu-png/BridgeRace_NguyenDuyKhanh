using UnityEngine;

public class Pause : UICanvas
{
    public void OnClickResume()
    {
        GameManager.Instance?.GameResume();
    }

    public void OnClickRestart()
    {
        GameManager.Instance?.GameRestart();
    }

    public void OnClickMenu()
    {
        GameManager.Instance?.GameMenu();
    }

    public override void BackKey()
    {
        OnClickResume();
    }
}
