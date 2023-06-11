using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnloadByDistance : MonoBehaviour
{
    [SerializeField] GameObject model;

    private void Update()
    {
        if ((PlayerInteraction.instance.getPlayerPosition() - model.transform.position).magnitude < Camera.main.farClipPlane)
        {
            model.SetActive(true);
        }
        else
        {
            model.SetActive(false);
        }
    }
}
