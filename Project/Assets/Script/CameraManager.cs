using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject TopCamera;
    private Transform topCameraTranform;

    [SerializeField]
    private CinemachineCamera topCamera;

    [SerializeField]
    private CinemachineCamera p1Camera;

    [SerializeField]
    private CinemachineCamera p2Camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        topCameraTranform = TopCamera.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        {
            if (PlayerManager.Instance.player.isHost)
            {
                p2Camera.Priority = 0;
                topCameraTranform.rotation = Quaternion.Euler(90, 0, 0);
                if (!GridManager.Instance.isShowing)
                {
                    p1Camera.Priority = 100;
                    topCamera.Priority = 0;
                }
                else
                {
                    p1Camera.Priority = 0;
                    topCamera.Priority = 100;
                }
            }
            else
            {
                p1Camera.Priority = 0;
                topCameraTranform.rotation = Quaternion.Euler(90, 180, 0);

                if (!GridManager.Instance.isShowing)
                {
                    p2Camera.Priority = 100;
                    topCamera.Priority = 0;
                }
                else
                {
                    p2Camera.Priority = 0;
                    topCamera.Priority = 100;
                }
            }
        }
    }
}
