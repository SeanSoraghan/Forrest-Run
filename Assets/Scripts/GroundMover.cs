using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorTrigger : AkCustomTrigger { }
public class MusicTrigger : AkCustomTrigger { }
 
public class GroundMover : MonoBehaviour
{
    public Transform TerrainFront;
    public Transform TerrainBack;

    public GameObject DoorsCurrent;
    public GameObject DoorsNext;

    public float PlayerBeyondDoorThreshold = 2.0f;
    public Transform PlayerTransform;
    public float Radius = 10.0f;
	
    public float TooCloseThreshold = 2.0f;

    public float TerrainLength = 240.0f;

    public Text Instructions;
    public GameObject ButtonObject;
    private List<GameObject> Spikes = new List<GameObject>();

    private Vector3 TerrainFrontStartPosition = Vector3.zero;
    private Vector3 TerrainBackStartPosition = Vector3.zero;
    private Vector3 DoorsCurrentStartPosition = Vector3.zero;
    private Vector3 DoorsNextStartPosition = Vector3.zero;
    private Vector3 PlayerStartPosition = Vector3.zero;
    private uint MusicPlayingID = 0;
    private bool EscapePressed = false;
    // Use this for initialization
	void Start ()
    {
        if (PlayerTransform == null)
            Debug.Log("Ground Mover: PlayerTransform is null!");
        PlayerTransform.gameObject.GetComponent<PlayerController>().OnPlayerDied += ResetGame;
        if (DoorsCurrent == null)
            Debug.LogError("Ground Mover: Doors Current is null!");
        if (DoorsNext == null)
            Debug.LogError("Ground Mover: Doors Next is null!");
        DoorController[] doors = DoorsCurrent.GetComponentsInChildren<DoorController>();
        foreach (DoorController door in doors)
            door.OnDoorOpened += DoorOpened;
        doors = DoorsNext.GetComponentsInChildren<DoorController>();
        foreach (DoorController door in doors)
            door.OnDoorOpened += DoorOpened;

        if(ButtonObject == null)
            Debug.LogError("Environment controller: Button not found!");
        ButtonObject.GetComponent<Button>().onClick.AddListener(GameStarted);
        TerrainFrontStartPosition = TerrainFront.position;
        TerrainBackStartPosition = TerrainBack.position;
        DoorsCurrentStartPosition = DoorsCurrent.transform.position;
        DoorsNextStartPosition = DoorsNext.transform.position;
        PlayerStartPosition = PlayerTransform.position;
	}

    private void Update ()
    {
        if(Input.GetAxisRaw("Cancel") != 0)
        { 
            if (!EscapePressed)
            {
                EscapePressed = true;
                Application.Quit();
            }
        }
        else
        {
            EscapePressed = false;
        }
    }
    // Update is called once per frame
    void FixedUpdate ()
    {
		UpdatePlayerGroundPosition();

        if (PlayerTransform.transform.position.z - DoorsCurrent.transform.position.z > PlayerBeyondDoorThreshold)
        { 
            DoorController[] doors = DoorsCurrent.GetComponentsInChildren<DoorController>();
            foreach (DoorController d in doors)
            { 
                d.CloseInstant();
            }

            Vector3 doorsPos = DoorsCurrent.transform.position;
            doorsPos.Set(doorsPos.x, doorsPos.y, doorsPos.z + Radius * 2.0f);
            DoorsCurrent.transform.position = doorsPos;
            GateController gate = DoorsCurrent.GetComponent<GateController>();
            if (gate != null)
            {
                gate.DoorChangeFrequency = Random.Range(0.5f, 3.0f);
                gate.StartFlipping();
            }

            GameObject temp = DoorsCurrent;
            DoorsCurrent = DoorsNext;
            DoorsNext = temp;
        }
	}

    void UpdatePlayerGroundPosition()
    {
        float PlayerZ = PlayerTransform.position.z;
        if (PlayerZ > TerrainFront.position.z - TerrainLength * 0.4f && PlayerZ < TerrainFront.position.z + TerrainLength * 0.5f)
        {
            Debug.Log(PlayerZ + " | " + TerrainFront.position.z);
            Vector3 currentBackPos = TerrainBack.transform.position;
            currentBackPos.Set(currentBackPos.x, currentBackPos.y, currentBackPos.z + TerrainLength * 2.0f);
            TerrainBack.transform.position = currentBackPos;
            Transform oldBack = TerrainBack;
            TerrainBack = TerrainFront;
            TerrainFront = oldBack;
        }
    }

    void SpawnSpikes()
    {
        DestroySpikes();
        Debug.Log("Spawning Spikes");

        int numSpikesToSpawn = Random.Range(8, 11);
        float minZ = DoorsNext.transform.position.z - Radius * 0.15f;
        float maxZ = DoorsNext.transform.position.z - Radius * 0.6f;
        for (int s = 0; s < numSpikesToSpawn; ++s)
        { 
            float zPos = Random.Range(minZ, maxZ + 1);
            float xPos = Random.Range(-4, 5);
            while(!SpawnPositionValid(new Vector3(xPos, -1, zPos)))
            {
                zPos = Random.Range(minZ, maxZ + 1);
                xPos = Random.Range(-4, 5);
            }
            GameObject spike = Instantiate(Resources.Load(Random.Range(0,2) == 1 ? "SpikeTree" : "SpikeTree2", typeof(GameObject))) as GameObject;
            Spikes.Add(spike);
            Vector3 spikePos = spike.transform.position;
            Vector3 loweredPos = spike.GetComponent<SpikeController>().LoweredPosition;
            spikePos.Set(xPos, loweredPos.y, zPos);
            spike.transform.position = spikePos;
            SpikeController spikeController = spike.GetComponent<SpikeController>();
            if(spikeController != null)
            {
                spikeController.State = SpikeController.SpikeState.Lowered;
                spikeController.RiseFallFrequency = Random.Range(0.5f, 1.5f);
            }
        }
    }

    public void DestroySpikes()
    {
        Debug.Log("Destroying Spikes");
        for(int s = 0; s < Spikes.Count; ++s)
        {
            GameObject.Destroy(Spikes[s]);
        }
        Spikes.Clear();
    }

    void DoorOpened(bool TruthDoor)
    {
        DoorTrigger doorSound = GetComponent<DoorTrigger>();
        if (doorSound != null)
            doorSound.TriggerSound();
        Debug.Log("DoorOpenedCallback");
        if (!TruthDoor)
        { 
            AkSoundEngine.SetRTPCValue("Dare", 1.0f);
            SpawnSpikes();
            PlayerTransform.gameObject.GetComponent<PlayerController>().TriggerCameraShake();
        }
        else
        {
            DestroySpikes();
            AkSoundEngine.SetRTPCValue("Dare", 0.0f);
        }
    }

    bool SpawnPositionValid(Vector3 pos)
    {
        foreach (GameObject spike in Spikes)
        {
            if(Vector3.Distance(pos, spike.transform.position) <= TooCloseThreshold)
            {
                return false;
            }
        }
        return true;
    }

    public void GameStarted()
    {
        MusicPlayingID = AkSoundEngine.PostEvent("Music", gameObject);
    }

    public void ResetGame()
    {
        TerrainFront.position = TerrainFrontStartPosition;
        TerrainBack.position = TerrainBackStartPosition;
        DoorsCurrent.transform.position = DoorsCurrentStartPosition;
        DoorsNext.transform.position = DoorsNextStartPosition;
        PlayerTransform.position = PlayerStartPosition;
        PlayerTransform.gameObject.GetComponent<PlayerController>().SetMovementEnabled(false);
        DoorController[] doors = DoorsCurrent.GetComponentsInChildren<DoorController>();
        foreach (DoorController d in doors)
        { 
            d.CloseInstant();
        }
        doors = DoorsNext.GetComponentsInChildren<DoorController>();
        foreach (DoorController d in doors)
        { 
            d.CloseInstant();
        }
        GateController gate = DoorsCurrent.GetComponent<GateController>();
        if (gate != null)
        {
            gate.DoorChangeFrequency = Random.Range(0.5f, 3.0f);
            gate.StartFlipping();
        }
        ButtonObject.SetActive(true);
        Instructions.gameObject.SetActive(true);
        DestroySpikes();

        AkSoundEngine.StopPlayingID(MusicPlayingID);
        AkSoundEngine.SetRTPCValue("Dare", 0.0f);
    }
}
