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

    protected FPSCameraMovement m_cameraControl;
    protected RBCharacterMovement m_movementScript;

    protected virtual void Awake()
    {
        noControlInputs = new Inputs();

        ControlsEnabled = true;

        m_cameraControl = GetComponentInChildren<FPSCameraMovement>();
        m_movementScript = GetComponent<RBCharacterMovement>();

        if (!m_cameraControl)
        {
            Debug.LogError("No FPSCameraMovement found on childrens");
        }
    }

    protected virtual void Update()
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
                }
                else
                {
                    UpdateCamera(noControlInputs, noControlInputs.LockOn);
                    UpdateMovement(noControlInputs, noControlInputs.LockOn);
                }
                
                OnUpdate(inputs);
            }
            else
            {
                UpdateCamera(noControlInputs, noControlInputs.LockOn);
                UpdateMovement(noControlInputs, noControlInputs.LockOn);
                
                OnUpdate(noControlInputs);
            }
        }
    }

    protected virtual Inputs FetchInputs()
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
        }

		inputs = OnFetchInputs(inputs);

        return inputs;
	}

    public void SetKeyboardUse(bool useKeyboard)
    {
        m_useKeyboard = useKeyboard;
    }

	protected virtual Inputs OnFetchInputs(Inputs inputs)
	{
		return inputs;
    }

    protected void UpdateCamera(Inputs inputs, bool lockOn)
    {
        Transform[] lockableTargets = null;

        if (lockOn)
        {
            // TODO: Get all targets that can be locked on
        }

        m_cameraControl.RotateCamera(inputs, lockableTargets);
    }

    protected void UpdateMovement(Inputs inputs, bool lockOn)
    {
        m_movementScript.UpdateMovement(inputs, lockOn ? m_cameraControl.TargetLockedOn : null);
    }

    private bool ControlsCharacter()
    {
        return ControlsEnabled;
    }

    protected virtual bool OnPreventMovementControlCheck()
    {
        return false;
    }

    protected virtual void OnUpdate(Inputs inputs) { }
    
    public void EnableControl(bool enable)
    {
        ControlsEnabled = enable;
    }
}
