using UnityEngine;

public class Lose : UICanvas
{
    public void OnClickRestart()
    {
        GameManager.Instance?.GameRestart(true);
    }

    public void OnClickMenu()
    {
        GameManager.Instance?.GameMenu();
    }
}
