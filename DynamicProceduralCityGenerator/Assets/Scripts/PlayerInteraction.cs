using UnityEngine;
using Invector;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction instance;
    [SerializeField] GameObject playerObject;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public Vector3 getPlayerPosition()
    {
        return playerObject.transform.position;
    }

    public void InitializePlayer(Vector3 startPosition)
    {
        transform.GetChild(0).position = startPosition;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
