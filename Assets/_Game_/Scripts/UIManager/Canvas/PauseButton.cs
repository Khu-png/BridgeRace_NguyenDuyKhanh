using UnityEngine;

public class PauseButton : UICanvas
{
    public void OnClickPause()
    {
        GameManager.Instance?.GamePause();
    }
}
