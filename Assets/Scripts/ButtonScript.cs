using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public Text t;
    private Button b;

	// Use this for initialization
	void Start ()
    {
	    b = GetComponent<Button>();
        if (b == null)
            Debug.Log("Button script: Button not found!");
        b.onClick.AddListener(ButtonClicked);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void ButtonClicked()
    {
        b.gameObject.SetActive(false);
        t.gameObject.SetActive(false);
    }
}
