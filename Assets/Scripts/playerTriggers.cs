using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerTriggers : MonoBehaviour
{
    private GameManager _gameManager;
    public GameObject vCam2;

    private void Start()
    {
        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CamTrigger":
                vCam2.SetActive(true);
                break;
            case "Coletável":
                _gameManager.SetGem(1);
                Destroy(other.gameObject);
                break;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "CamTrigger":
                vCam2.SetActive(false);
                break;
        }
    }
}
