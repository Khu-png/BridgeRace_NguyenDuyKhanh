using UnityEngine;

public class JoystickCanvas : UICanvas
{
    [SerializeField] private Joystick joystick;

    public Joystick Joystick => joystick;

    protected override void OnInit()
    {
        base.OnInit();

        if (joystick == null)
        {
            joystick = GetComponentInChildren<Joystick>(true);
        }
    }
}
