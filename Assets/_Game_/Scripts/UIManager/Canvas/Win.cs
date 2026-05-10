using UnityEngine;

public class Win : UICanvas
{
    public void OnClickNext()
    {
        CloseDirectly();
        GameManager.Instance?.GameNextLevel();
    }

    public void OnClickMenu()
    {
        CloseDirectly();
        GameManager.Instance?.GameMenu();
    }
}
