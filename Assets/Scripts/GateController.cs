using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public DoorController DoorLeft;
    public DoorController DoorRight;
    public float DoorChangeFrequency = 2.0f;
    private float StateChangeCurrentTime = 0.0f;
    public bool IsFlipping = true;
	// Use this for initialization
	void Start ()
    {
		if (DoorLeft == null)
            Debug.LogError("Gate: Left Door is Null!");
        if (DoorRight == null)
            Debug.LogError("Gate: Right Door is Null!");
        DoorLeft.TruthDoor = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (IsFlipping)
        { 
		    StateChangeCurrentTime += Time.deltaTime;
            float StateChangePeriod = 1.0f / DoorChangeFrequency;
            if (StateChangeCurrentTime >= StateChangePeriod)
            {
                StateChangeCurrentTime = 0.0f;
                FlipDoors();
            }
        }
	}

    void FlipDoors()
    {
        DoorLeft.TruthDoor = !DoorLeft.TruthDoor;
        DoorRight.TruthDoor = !DoorRight.TruthDoor;
    }

    public void StopFlipping()
    {
        DoorLeft.TruthDoor = false;
        DoorRight.TruthDoor = false;
        IsFlipping = false;
    }

    public void StartFlipping()
    {
        if (!IsFlipping)
        { 
            if (!((DoorLeft.TruthDoor && !DoorRight.TruthDoor)||(!DoorLeft.TruthDoor && DoorRight.TruthDoor)))
            { 
                DoorLeft.TruthDoor = !DoorLeft.TruthDoor;
            }
            IsFlipping = true;
        }
    }
}
