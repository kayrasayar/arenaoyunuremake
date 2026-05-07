using UnityEngine;

public class OverworldCamera : MonoBehaviour
{
    [Header("Kamera Ayarları")]
    public float zoomSpeed = 20f;
    public float minZoomDistance = 15f;
    public float maxZoomDistance = 80f;
    public float panSpeed = 1f;
    public float rotationSpeed = 150f;
    public float cameraAngle = 35f;

    [Header("Sınır Ayarları")]
    public float minX = -100f;
    public float maxX = 100f;
    public float minY = 15f;
    public float maxY = 50f;
    public float minZ = -100f;
    public float maxZ = 100f;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    private float lastMouseX;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("OverworldCamera: Camera bileşeni bulunamadı!");
            return;
        }

        transform.rotation = Quaternion.Euler(cameraAngle, transform.eulerAngles.y, 0f);
    }

    void Update()
    {
        if (cam == null) return;

        HandleZoom();
        HandlePan();
        HandleRotation();
        ClampPosition();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f)) return;

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoomDistance, maxZoomDistance);
        }
        else
        {
            Vector3 forward = transform.forward;
            transform.position += forward * scroll * zoomSpeed;
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButton(1) && isDragging)
        {
            Vector3 difference = Input.mousePosition - dragOrigin;
            Vector3 right = transform.right;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 move = (-right * difference.x + -forward * difference.y) * panSpeed * Time.deltaTime;
            transform.position += move;
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMouseX = Input.mousePosition.x;
        }

        if (Input.GetMouseButton(2))
        {
            float deltaX = Input.mousePosition.x - lastMouseX;
            transform.Rotate(Vector3.up, deltaX * rotationSpeed * Time.deltaTime, Space.World);
            lastMouseX = Input.mousePosition.x;
        }
    }

    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
}