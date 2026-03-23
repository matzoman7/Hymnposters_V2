using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera.transform.position = new Vector3(-7.65999985f, 5.78999996f, -13.1099997f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayerOneCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.74000001f, 5.78999996f, -11.1099997f);
    }

    private void PlayerTwoCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.74000001f, 5.78999996f, -12.54f);
    }

    private void PlayerThreeCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.74000001f, 5.78999996f, -14.0699997f);
    }

    private void PlayerFourCameraPosition()
    {
        mainCamera.transform.position = new Vector3(-2.74000001f, 5.78999996f, -15.5f);
    }
}
