using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    [Header("Powers")]
    [SerializeField]
    private Power[] m_powers;

    private Dictionary<int, bool> m_powerAvailability;
    public int NbrOfAvailablePowers { get; private set; }

    private int m_selectedPower;

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
            SelectPower(m_selectedPower);
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
                    m_selectedPower = i;
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
                int previousPower = m_selectedPower;
                m_selectedPower = powerIndex;

                ChangePowerAnimation(previousPower, m_selectedPower);
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
        return (m_selectedPower + 1) % m_powers.Length;
    }

    public void SelectPreviousPower()
    {
        m_indexChangeDelegateMethod = CalculatePreviousIndex;

        ChangePower();
    }

    private int CalculatePreviousIndex()
    {
        return (m_powers.Length + m_selectedPower - 1) % m_powers.Length;
    }

    private void ChangePower()
    {
        int previousPower = m_selectedPower;

        do
        {
            m_selectedPower = m_indexChangeDelegateMethod();
        }
        while (!m_powerAvailability[m_selectedPower] && previousPower != m_selectedPower);

        if (m_selectedPower != previousPower)
        {
            ChangePowerAnimation(previousPower, m_selectedPower);
        }
    }

    private void ChangePowerAnimation(int previousPower, int currentPower)
    {
        m_armsAnimator.SetBool(m_powers[previousPower].IsIdleParamHashId, false);
        m_armsAnimator.SetBool(m_powers[currentPower].IsIdleParamHashId, true);
    }

    public void ChargePower()
    {

    }

    public void UsePower()
    {

    }
}
