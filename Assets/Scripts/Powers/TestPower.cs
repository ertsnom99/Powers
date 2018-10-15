using UnityEngine;

public class TestPower : Power
{
    protected override void SubscribeToEvents() { }

    public override void Show(bool show) { }

    public override bool Use()
    {
        return false;
    }

    public override bool Cancel()
    {
        return false;
    }

    // Methods of the IAnimatorEventSubscriber interface used by the parent class
    public override void NotifyEvent(string eventName)
    {
        switch (eventName)
        {
            default:
                Debug.LogWarning("Unpredicted event: " + eventName);
                break;
        }
    }
}
