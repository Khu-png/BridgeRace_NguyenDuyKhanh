using UnityEngine;

public class Lose : UICanvas
{
    public void OnClickRestart()
    {
        GameManager.Instance?.GameRestart();
    }

    public void OnClickMenu()
    {
        GameManager.Instance?.GameMenu();
    }
}
