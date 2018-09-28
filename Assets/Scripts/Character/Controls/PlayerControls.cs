using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(RBCharacterMovement))]

public class PlayerControls : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    private bool m_useKeyboard;

    private Inputs noControlInputs;

    public bool ControlsEnabled { get; private set; }

    private FPSCameraMovement m_cameraControl;
    private RBCharacterMovement m_movementScript;
    private PowerManager m_powerManager;
    
    private void Awake()
    {
        noControlInputs = new Inputs();

        ControlsEnabled = true;

        m_cameraControl = GetComponentInChildren<FPSCameraMovement>();
        m_movementScript = GetComponent<RBCharacterMovement>();
        m_powerManager = GetComponentInChildren<PowerManager>();

        if (!m_cameraControl)
        {
            Debug.LogError("No FPSCameraMovement found on childrens");
        }

        if (!m_powerManager)
        {
            Debug.LogError("No PowerManager found on childrens");
        }
    }

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > 0.0f)
        {
            // Get the inputs used during this frame
            Inputs inputs = FetchInputs();

            if (ControlsCharacter())
            {
                // Movement and camera update
                if (!OnPreventMovementControlCheck())
                {
                    UpdateCamera(inputs, inputs.LockOn);
                    UpdateMovement(inputs, inputs.LockOn);
                    UpdatePower(inputs);
                }
                else
                {
                    UpdateCamera(noControlInputs, noControlInputs.LockOn);
                    UpdateMovement(noControlInputs, noControlInputs.LockOn);
                    UpdatePower(noControlInputs);
                }
            }
            else
            {
                UpdateCamera(noControlInputs, noControlInputs.LockOn);
                UpdateMovement(noControlInputs, noControlInputs.LockOn);
                UpdatePower(noControlInputs);
            }
        }
    }

    private Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();
        
	    if (m_useKeyboard)
	    {
            // Inputs from the keyboard
            inputs.vertical = Input.GetAxisRaw("Vertical");
            inputs.horizontal = Input.GetAxisRaw("Horizontal");
            inputs.running = Input.GetButton("Run");
            inputs.jump = Input.GetButtonDown("Jump");
            inputs.xAxis = Input.GetAxis("Mouse X");
            inputs.yAxis = Input.GetAxis("Mouse Y");

            inputs.LockOn = Input.GetButton("Lock");

            inputs.PreviousPower = Input.GetKeyDown("q");
            inputs.NextPower = Input.GetKeyDown("e");
        }
	    else
	    {
            // Inputs grom the
            inputs.vertical = Input.GetAxisRaw("Controller Vertical");
            inputs.horizontal = Input.GetAxisRaw("Controller Horizontal");
            inputs.running = Input.GetButton("Controller Run");
            inputs.jump = Input.GetButtonDown("Controller Jump");
            inputs.xAxis = Input.GetAxis("Right Stick X");
            inputs.yAxis = Input.GetAxis("Right Stick Y");

            inputs.LockOn = Input.GetButton("Controller Lock");

            inputs.PreviousPower = Input.GetButtonDown("Left Bumper");
            inputs.NextPower = Input.GetButtonDown("Right Bumper");
        }

        return inputs;
	}

    public void SetKeyboardUse(bool useKeyboard)
    {
        m_useKeyboard = useKeyboard;
    }

    private bool ControlsCharacter()
    {
        return ControlsEnabled;
    }

    private bool OnPreventMovementControlCheck()
    {
        return false;
    }

    private void UpdateCamera(Inputs inputs, bool lockOn)
    {
        Transform[] lockableTargets = null;

        if (lockOn)
        {
            // TODO: Get all targets that can be locked on
        }

        m_cameraControl.RotateCamera(inputs, lockableTargets);
    }

    private void UpdateMovement(Inputs inputs, bool lockOn)
    {
        m_movementScript.UpdateMovement(inputs, lockOn ? m_cameraControl.TargetLockedOn : null);
    }

    private void UpdatePower(Inputs inputs)
    {
        // Only allow to switch if there's more than one power available
        if (m_powerManager.NbrOfAvailablePowers > 1)
        {
            if (inputs.PreviousPower)
            {
                m_powerManager.SelectPreviousPower();
            }
            else if (inputs.NextPower)
            {
                m_powerManager.SelectNextPower();
            }
        }
    }
    
    public void EnableControl(bool enable)
    {
        ControlsEnabled = enable;
    }
}
