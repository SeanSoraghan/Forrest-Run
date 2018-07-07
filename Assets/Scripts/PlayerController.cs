using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public virtual void Interact()
    {
        Debug.Log("Interact!");
    }
}

public class CroakTrigger : AkCustomTrigger { }
public class DiedTrigger : AkCustomTrigger { }

public class PlayerController : MonoBehaviour
{
    public enum CameraState{Static, Shaking};

    public Camera MainCamera;
    public float MovementSpeed = 3.0f;
    public Transform PlayerModel;
    public float MaxRotation = 45.0f;
    private bool CanInteract = false;
    private bool InteractPressed = false;
    private Vector3 Velocity = Vector3.zero;
    private CharacterController Controller;
    private Vector3 PlayerToCamera;
    private InteractableObject InteractionObject = null;
    private bool MovementEnabled = false;
    private Animator PlayerAnimator;

    private Vector3 CameraCentrePosition;
    public CameraState CameraShakeState = CameraState.Static;
    public float CameraShakeDuration = 0.5f;
    public float CameraShakeAmplitude = 10.0f;
    public float CameraShakeRate = 5.0f;
    private float CameraShakeCurrentTime = 0.0f;

    public delegate void PlayerDied();
    public PlayerDied OnPlayerDied;

	// Use this for initialization
	void Start ()
    {
		Controller = GetComponent<CharacterController>();
        if (MainCamera == null)
            Debug.LogError("Player: MainCamera reference missing!");
        PlayerToCamera = MainCamera.transform.position - transform.position;
        CameraCentrePosition = MainCamera.transform.position;
        if (Controller == null)
            Debug.LogError("CharacterController Missing on Player Object!");
        if (PlayerModel == null)
            Debug.LogError("PlayerController: Player Model is null!");
        PlayerAnimator = GetComponentInChildren<Animator>();
        if(PlayerAnimator == null)
        {
            Debug.LogError("Player animator not found!");
        }
	}
	
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        GateController gate = hit.gameObject.GetComponentInParent<GateController>();
        if (gate != null)
            gate.StopFlipping();
        //DoorController door = hit.gameObject.GetComponentInParent<DoorController>();
        //if (door != null)
        //    door.Open();

        
    }

    public void SetMovementEnabled(bool enabled)
    {
        MovementEnabled = enabled;
        if (!MovementEnabled)
        {
            Velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter (Collider other)
    {
        SpikeController spike = other.gameObject.GetComponent<SpikeController>();
        if (spike != null)
        {
            Debug.Log("Spiked!");
            DiedTrigger diedSound = GetComponent<DiedTrigger>();
            if (diedSound != null)
                diedSound.TriggerSound();
            if(OnPlayerDied != null)
            {
                OnPlayerDied();
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (MovementEnabled)
        { 
		    Velocity.Set(Input.GetAxis("Horizontal"), 0, 1.0f/*Mathf.Max(0.0f, Input.GetAxis("Vertical"))*/);
            Velocity = transform.TransformDirection(Velocity);
            Velocity *= MovementSpeed;
        }
        if (Input.GetAxisRaw("Interact") != 0)
        {
            CroakTrigger croakSound = GetComponent<CroakTrigger>();
            if (croakSound != null)
                croakSound.TriggerSound();
            if (CanInteract)
            { 
                if (!InteractPressed)
                {
                    InteractPressed = true;
                    if (InteractionObject != null)
                    {
                        InteractionObject.Interact();
                    }
                }
            }
        }
        else
        {
            InteractPressed = false;
        }

        PlayerAnimator.SetFloat("Speed", Velocity.magnitude);
	}

    private void FixedUpdate ()
    {
        Vector3 ForwardVector = Velocity.normalized;
        Controller.Move(Velocity * Time.deltaTime);        
        PlayerModel.transform.rotation = Quaternion.FromToRotation(new Vector3(0.0f,0.0f,1.0f), ForwardVector);
        //Vector3 cameraPosition = CameraCentrePosition;// MainCamera.transform.position;
        Vector3 cameraPosition = transform.position + PlayerToCamera;
        /*MainCamera.transform.position*/CameraCentrePosition = cameraPosition;
        if (CameraShakeState == CameraState.Shaking)
        {
            AnimateCamera();
        }
        else
        {
            MainCamera.transform.position = CameraCentrePosition;
        } 
    }

    public void TriggerCameraShake()
    {
        CameraShakeState = CameraState.Shaking;
    }

    private void AnimateCamera()
    {
        CameraShakeCurrentTime += Time.deltaTime;
        if (CameraShakeCurrentTime >= CameraShakeDuration)
        {
            MainCamera.transform.position = CameraCentrePosition;
            CameraShakeState = CameraState.Static;
            CameraShakeCurrentTime = 0.0f;
        }
        else
        {
            float amplitude = CameraShakeCurrentTime / CameraShakeDuration;
            float offset = Mathf.Sin(CameraShakeCurrentTime * CameraShakeRate) * CameraShakeAmplitude * amplitude;
            Vector3 pos = CameraCentrePosition;
            pos.Set(pos.x + offset, pos.y, pos.z);
            MainCamera.transform.position = pos;
        }
    }

    public void SetInteractionObject(InteractableObject interactionObject)
    {
        InteractionObject = interactionObject;
        CanInteract = InteractionObject != null;
    }
}
