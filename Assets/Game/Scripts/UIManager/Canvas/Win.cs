using UnityEngine;

public class Win : UICanvas
{
    public void OnClickNext()
    {
        GameManager.Instance?.GameNextLevel();
    }

    public void OnClickMenu()
    {
        GameManager.Instance?.GameMenu();
    }
}
