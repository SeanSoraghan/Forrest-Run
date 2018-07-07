using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkCustomTrigger : AkTriggerBase
{
    public void TriggerSound()
    {
        if(triggerDelegate != null) 
            triggerDelegate(null);
    }
}

public class AkMovementTrigger : AkCustomTrigger { }
public class AnimationEventReciever : MonoBehaviour
{

	public void FrontFeet()
    {
        AkMovementTrigger movementSound = GetComponent<AkMovementTrigger>();
        if (movementSound != null)
            movementSound.TriggerSound();
    }

    public void BackFeet()
    {
        AkMovementTrigger movementSound = GetComponent<AkMovementTrigger>();
        if (movementSound != null)
            movementSound.TriggerSound();
    }
}
