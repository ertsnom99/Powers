using System.Collections.Generic;
using UnityEngine;

public enum ArmSide
{
    Left,
    Right
}

public enum SelectPowerOption
{
    Previous,
    Next
}

/*
- Manage powers:
- Manage the availabilitie of the powers
- Allow to change to a specific power or switch to previous or next power
- Act as an interface to start/stop charging currently selected power and to use currently selected power
*/

public class PowerManager : MonoBehaviour, IAnimatorEventSubscriber
{
    [Header("Animation")]
    [SerializeField]
    private Animator m_armsAnimator;
    [SerializeField]
    private ArmsEventsManager m_armsEventsManager;

    [Header("Powers")]
    [SerializeField]
    private Power[] m_powers;

    private Dictionary<int, bool> m_powerAvailability;
    public int NbrOfAvailablePowers { get; private set; }

    private bool m_isChangingPower = false;

    private int m_previousPowerIndex;
    private int m_selectedPowerIndex;

    private delegate int IndexChangeDelegate();
    IndexChangeDelegate m_indexChangeDelegateMethod;

    private void Awake()
    {
        CreatePowerAvailabilities();
    }

    private void Start()
    {
        ResetAnimator();
        InitialiseAllPowers();
        SubscribeToEvents();

        if (NbrOfAvailablePowers > 0)
        {
            SelectPower(m_selectedPowerIndex);
        }
    }

    private void CreatePowerAvailabilities()
    {
        NbrOfAvailablePowers = 0;

        m_powerAvailability = new Dictionary<int, bool>();
        bool foundFirstAvailablePower = false;

        bool powerAvailable;

        for (int i = 0; i < m_powers.Length; i++)
        {
            // TODO: Correctly check if the power is available
            powerAvailable = true;

            m_powerAvailability.Add(i, powerAvailable);

            if (powerAvailable)
            {
                NbrOfAvailablePowers++;

                if (!foundFirstAvailablePower)
                {
                    m_selectedPowerIndex = i;
                    foundFirstAvailablePower = true;
                }
            }
        }
    }

    private void ResetAnimator()
    {
        foreach(Power power in m_powers)
        {
            m_armsAnimator.SetBool(power.IsIdleParamHashId, false);
        }
    }

    private void InitialiseAllPowers()
    {
        for (int i = 0; i< m_powers.Length; i++)
        {
            m_powers[i].Initialise(m_armsAnimator, m_armsEventsManager, false);
        }
    }

    private void SubscribeToEvents()
    {
        m_armsEventsManager.Subscribe(ArmsEventsManager.LEFT_ARM_HIDE_POWER_EVENT, this);
        m_armsEventsManager.Subscribe(ArmsEventsManager.LEFT_ARM_SHOW_POWER_EVENT, this);
        m_armsEventsManager.Subscribe(ArmsEventsManager.RIGHT_ARM_HIDE_POWER_EVENT, this);
        m_armsEventsManager.Subscribe(ArmsEventsManager.RIGHT_ARM_SHOW_POWER_EVENT, this);
    }

    public void SelectPower(int powerIndex)
    {
        if (-1 < powerIndex && powerIndex < m_powers.Length)
        {
            if (m_powerAvailability[powerIndex])
            {
                m_previousPowerIndex = m_selectedPowerIndex;
                m_selectedPowerIndex = powerIndex;

                ChangePowerAnimation(m_previousPowerIndex, m_selectedPowerIndex);
            }
        }
        else
        {
            Debug.LogError("There isn't any power for the powerIndex " + powerIndex);
        }
    }

    // Returns if the current power was changed
    public bool SelectPower(SelectPowerOption selectOption)
    {
        int selectedPowerIndexBeforeChange = m_selectedPowerIndex;
        
        switch(selectOption)
        {
            case SelectPowerOption.Previous:
                m_indexChangeDelegateMethod = CalculatePreviousIndex;
                break;
            case SelectPowerOption.Next:
                m_indexChangeDelegateMethod = CalculateNextIndex;
                break;
            default:
                Debug.LogError("Unpredicted SelectPowerOption: " + selectOption);
                break;
        }

        ChangePower();

        return selectedPowerIndexBeforeChange != m_selectedPowerIndex;
    }

    private int CalculateNextIndex()
    {
        return (m_selectedPowerIndex + 1) % m_powers.Length;
    }

    private int CalculatePreviousIndex()
    {
        return (m_powers.Length + m_selectedPowerIndex - 1) % m_powers.Length;
    }

    // Used to go to next or previous power based on m_indexChangeDelegateMethod
    private void ChangePower()
    {
        // Only allow to change while no power is being changed at that moment and if there's more than one power available
        if (!m_isChangingPower && m_powers[m_selectedPowerIndex].CanBeStop && NbrOfAvailablePowers > 1)
        {
            int selectedPowerIndexBeforeChange = m_selectedPowerIndex;
            int lastIterationPower = m_selectedPowerIndex;

            do
            {
                m_selectedPowerIndex = m_indexChangeDelegateMethod();
            }
            while (!m_powerAvailability[m_selectedPowerIndex] && lastIterationPower != m_selectedPowerIndex);

            if (m_selectedPowerIndex != lastIterationPower)
            {
                m_isChangingPower = true;

                m_previousPowerIndex = selectedPowerIndexBeforeChange;
                ChangePowerAnimation(lastIterationPower, m_selectedPowerIndex);
            }
        }
    }

    private void ChangePowerAnimation(int previousPower, int currentPower)
    {
        m_armsAnimator.SetBool(m_powers[previousPower].IsIdleParamHashId, false);
        m_armsAnimator.SetBool(m_powers[currentPower].IsIdleParamHashId, true);
    }

    private void ShowPower(int powerIndex, bool show)
    {
        m_powers[powerIndex].Show(show);
    }

    // Returns if the current power started charging
    public bool StartChargingPower()
    {
        bool chargeStarted = false;

        if (m_powers[m_selectedPowerIndex].IsChargeable && !m_powers[m_selectedPowerIndex].IsCharging)
        {
            chargeStarted = m_powers[m_selectedPowerIndex].StartCharging();
        }

        return chargeStarted;
    }

    public bool StopChargingPower()
    {
        bool chargeEnded = false;

        if (m_powers[m_selectedPowerIndex].IsChargeable && m_powers[m_selectedPowerIndex].IsCharging)
        {
            chargeEnded = m_powers[m_selectedPowerIndex].StopCharging();
        }

        return chargeEnded;
    }

    public bool UsePower()
    {
        return m_powers[m_selectedPowerIndex].Use();
    }

    public bool CancelPower()
    {
        return m_powers[m_selectedPowerIndex].Cancel();
    }

    // Methods of the IAnimatorEventSubscriber interface
    public void NotifyEvent(string eventName)
    {
        switch(eventName)
        {
            case ArmsEventsManager.LEFT_ARM_HIDE_POWER_EVENT:
                ShowPower(m_previousPowerIndex, false);
                break;
            case ArmsEventsManager.LEFT_ARM_SHOW_POWER_EVENT:
                ShowPower(m_selectedPowerIndex, true);
                m_isChangingPower = false;
                break;
            case ArmsEventsManager.RIGHT_ARM_HIDE_POWER_EVENT:

                break;
            case ArmsEventsManager.RIGHT_ARM_SHOW_POWER_EVENT:

                break;
            default:
                Debug.LogWarning("Unpredicted event: " + eventName);
                break;
        }
    }
}
