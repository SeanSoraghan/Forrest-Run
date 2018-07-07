using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrigger : AkCustomTrigger { }

public class SpikeController : MonoBehaviour
{
    public enum SpikeState{Lowered, Raised, Lowering, Rising};

    public float MovementSpeed = 3.0f;
    public float StateReachedThreshold = 0.1f;
    public Vector3 LoweredPosition;
    public Vector3 RaisedPosition;
    
    public SpikeState State;

    public float RiseFallFrequency = 2.0f;
    private float RiseFallCurrentTime = 0.0f;
    private float LerpTime = 0.0f;
    public Quaternion LoweredRotation;
    public Quaternion RaisedRotation;
    private void Start ()
    {
        Vector3 p = transform.position;
        LoweredPosition.Set(p.x, LoweredPosition.y, p.z);
        RaisedPosition.Set(p.x, RaisedPosition.y, p.z);
        LoweredRotation = transform.rotation;
        Vector3 LoweredRotationEuler = LoweredRotation.eulerAngles;
        RaisedRotation = Quaternion.Euler(LoweredRotationEuler.x, LoweredRotationEuler.y + 180.0f, LoweredRotationEuler.z);
    }
    // Create an extra "spike tip" controller ...?
    //private void OnControllerColliderHit (ControllerColliderHit hit)
    //{
    //    PlayerController player = hit.gameObject.GetComponent<PlayerController>();
    //    if (player != null)
    //        player.TakeDamage();
    //}

    private void Update ()
    {
        float RiseFallPeriod = 1.0f / RiseFallFrequency;
        RiseFallCurrentTime += Time.deltaTime;
        if (RiseFallCurrentTime >= RiseFallPeriod)
        {
            RiseFallCurrentTime = 0.0f;
            if (State == SpikeState.Lowered)
                Rise();
            else if (State == SpikeState.Raised)
                Lower();
        }
    }

    private void FixedUpdate ()
    {
        if (State == SpikeState.Rising || State == SpikeState.Lowering)
        {
            Vector3 Start = State == SpikeState.Rising ? LoweredPosition : RaisedPosition;
            Vector3 End = State == SpikeState.Rising ? RaisedPosition : LoweredPosition;
            Vector3 lerped = Vector3.Lerp(Start, End, LerpTime*LerpTime);

            Quaternion StartRot = State == SpikeState.Rising ? LoweredRotation : RaisedRotation;
            Quaternion EndRot = State == SpikeState.Rising ? RaisedRotation : LoweredRotation;
            Quaternion lerpedRot = Quaternion.Lerp(StartRot, EndRot, LerpTime*LerpTime);
            LerpTime += Time.deltaTime * MovementSpeed;
            
            transform.position = lerped;
            transform.rotation = lerpedRot;
            Vector3 CurrentPos = transform.position;
            if (Vector3.Distance(CurrentPos, End) <= StateReachedThreshold)
            {
                transform.position = End;
                transform.rotation = EndRot;
                State = State == SpikeState.Rising ? SpikeState.Raised : SpikeState.Lowered;
                LerpTime = 0.0f;
            }
        }
    }

    public void Rise()
    {
        if (State == SpikeState.Lowered)
        {
            State = SpikeState.Rising;
            SpikeTrigger spikeSound = GetComponent<SpikeTrigger>();
            if (spikeSound != null)
                spikeSound.TriggerSound();
        }
    }

    public void Lower()
    {
        if (State == SpikeState.Raised)
        {
            State = SpikeState.Lowering;
        }
    }
}
