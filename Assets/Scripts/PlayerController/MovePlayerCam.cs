using UnityEngine;

public class MovePlayerCam : MonoBehaviour
{
    [SerializeField]
    private Transform cameraPosition;

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
