using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    [Header("Powers")]
    [SerializeField]
    private Power[] m_powers;

    private Dictionary<int, bool> m_powerAvailability;
    public int NbrOfAvailablePowers { get; private set; }

    private int m_selectedPowerIndex;

    private delegate int IndexChangeDelegate();
    IndexChangeDelegate m_indexChangeDelegateMethod;

    [Header("Animation")]
    [SerializeField]
    private Animator m_armsAnimator;

    private void Awake()
    {
        CreatePowerAvailabilities();
    }

    private void Start()
    {
        ResetAnimator();

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

    public void SelectPower(int powerIndex)
    {
        if (-1 < powerIndex && powerIndex < m_powers.Length)
        {
            if (m_powerAvailability[powerIndex])
            {
                int previousPower = m_selectedPowerIndex;
                m_selectedPowerIndex = powerIndex;

                ChangePowerAnimation(previousPower, m_selectedPowerIndex);
            }
        }
        else
        {
            Debug.LogError("There isn't any power for the powerIndex " + powerIndex);
        }
    }

    public void SelectNextPower()
    {
        m_indexChangeDelegateMethod = CalculateNextIndex;

        ChangePower();
    }

    private int CalculateNextIndex()
    {
        return (m_selectedPowerIndex + 1) % m_powers.Length;
    }

    public void SelectPreviousPower()
    {
        m_indexChangeDelegateMethod = CalculatePreviousIndex;

        ChangePower();
    }

    private int CalculatePreviousIndex()
    {
        return (m_powers.Length + m_selectedPowerIndex - 1) % m_powers.Length;
    }

    private void ChangePower()
    {
        // Only allow to switch if there's more than one power available
        if (NbrOfAvailablePowers > 1)
        {
            int previousPower = m_selectedPowerIndex;

            do
            {
                m_selectedPowerIndex = m_indexChangeDelegateMethod();
            }
            while (!m_powerAvailability[m_selectedPowerIndex] && previousPower != m_selectedPowerIndex);

            if (m_selectedPowerIndex != previousPower)
            {
                ChangePowerAnimation(previousPower, m_selectedPowerIndex);
            }
        }
    }

    private void ChangePowerAnimation(int previousPower, int currentPower)
    {
        m_armsAnimator.SetBool(m_powers[previousPower].IsIdleParamHashId, false);
        m_armsAnimator.SetBool(m_powers[currentPower].IsIdleParamHashId, true);
    }

    public bool IsSelectedPowerChargeable()
    {
        return m_powers[m_selectedPowerIndex].IsChargeable;
    }

    public bool IsSelectedPowerCharging()
    {
        return m_powers[m_selectedPowerIndex].IsCharging;
    }

    public void StartChargingPower()
    {
        m_powers[m_selectedPowerIndex].StartCharging();
        Debug.Log(Time.frameCount + " Charge");
    }

    public void StopChargingPower()
    {
        m_powers[m_selectedPowerIndex].StopCharging();
        Debug.Log(Time.frameCount + " Stop");
    }

    public void UsePower()
    {
        m_powers[m_selectedPowerIndex].Use();
        Debug.Log(Time.frameCount + " Use");
    }
}
