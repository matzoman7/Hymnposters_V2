using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //mainCamera.transform.position = new Vector3(-5.03000021f, 5.15999985f, -13.1599998f);
        mainCamera.transform.position = new Vector3(-2.70000005f, 5.48000002f, -11.0799999f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayerZeroCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.70000005f, 5.48000002f, -11.0799999f);
    }

    public void PlayerOneCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.70000005f, 5.48000002f, -12.5799999f);
    }

    public void PlayerTwoCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.70000005f, 5.48000002f, -14.1000004f);
    }

    public void PlayerThreeCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.70000005f, 5.48000002f, -15.5200005f);
    }
}
