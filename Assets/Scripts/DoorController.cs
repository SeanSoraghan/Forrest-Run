using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : InteractableObject
{
    public enum DoorState{Open, Closed, Opening, Closing};

    [Range(0.1f, 2.0f)]
    public float MovementSpeed = 1.0f;
    public Vector3 RotationValuesOpen;
    public Vector3 RotationValuesClosed;

    public DoorState State;
    public float StateReachedThreshold = 0.1f;

    private Material Mat;
    private Color MatColor;

    private bool IsTruthDoor = false;
    public bool TruthDoor
    {
        get{ return IsTruthDoor; }
        set
        {
            IsTruthDoor = value;
            if(Mat != null)
            { 
                if(IsTruthDoor)
                {
                    Mat.color = MatColor;
                }
                else
                {
                    Color c = Mat.color;
                    c.a = 0.1f;
                    Mat.color = c;
                }
            }
        }
    }

    private float LerpTime = 0.0f;

    public delegate void DoorOpened(bool isTruthDoor);
    public DoorOpened OnDoorOpened;

    private void Start ()
    {
        Mat = GetComponentInChildren<Renderer>().material;
        MatColor = Mat.color;
    }

    private void FixedUpdate()
    {
        if (State == DoorState.Opening || State == DoorState.Closing)
        {
            Quaternion Start = State == DoorState.Opening ? Quaternion.Euler(RotationValuesClosed) 
                                                          : Quaternion.Euler(RotationValuesOpen);
            Quaternion End = State == DoorState.Opening ? Quaternion.Euler(RotationValuesOpen) 
                                                        : Quaternion.Euler(RotationValuesClosed);
            Quaternion lerped = Quaternion.Lerp(Start, End, LerpTime);
            LerpTime += Time.deltaTime * MovementSpeed;
            
            transform.rotation = lerped;
            Vector3 CurrentRotationEuler = transform.rotation.eulerAngles;
            if (Vector3.Distance(CurrentRotationEuler, End.eulerAngles) <= StateReachedThreshold)
            {
                transform.rotation = End;
                State = State == DoorState.Opening ? DoorState.Open : DoorState.Closed;
                LerpTime = 0.0f;
            }
        }
    }

    

    private void OnTriggerEnter (Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !DoorIsMoving())
        {
            player.SetInteractionObject(this);
        }
    }

    private void OnTriggerExit (Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetInteractionObject(null);
        }
    }

    public override void Interact()
    {
        if(State == DoorState.Closed)
            Open();
        else if (State == DoorState.Open)
            Close();
    }

    public void Open()
    {
        if (State == DoorState.Closed)
        {
            State = DoorState.Opening;
            if (OnDoorOpened != null)
                OnDoorOpened(TruthDoor);
        }
    }

    public void Close()
    {
        if (State == DoorState.Open)
        {
            State = DoorState.Closing;
        }
    }

    public void CloseInstant()
    {
        if (State == DoorState.Open || State == DoorState.Opening)
        {
            transform.rotation = Quaternion.Euler(RotationValuesClosed);
            State = DoorState.Closed;
        }
    }

    public bool DoorIsMoving()
    {
        return State == DoorState.Closing || State == DoorState.Opening;
    }
}
