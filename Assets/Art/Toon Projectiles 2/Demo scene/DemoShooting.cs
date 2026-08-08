using UnityEngine;
using UnityEngine.InputSystem;

public class DemoShooting : MonoBehaviour {
    public GameObject FirePoint;
    public Camera Cam;
    public float MaxLength;
    public GameObject[] Prefabs;

    private Ray RayMouse;
    private Vector3 direction;
    private Quaternion rotation;

    [Header("GUI")]
    private float windowDpi;
    private int Prefab;
    private GameObject Instance;
    private float hSliderValue = 0.1f;
    private float fireCountdown = 0f;

    //Double-click protection
    private float buttonSaver = 0f;

    //For Camera shake 
    public Animation camAnim;

    private void Start() {
        if (Screen.dpi < 1) {
            windowDpi = 1;
        }

        if (Screen.dpi < 200) {
            windowDpi = 1;
        } else {
            windowDpi = Screen.dpi / 200f;
        }

        Counter(0);
    }

    private void Update() {
        //SingleValue shoot
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            camAnim.Play(camAnim.clip.name);
            Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
        }

        //Fast shooting
        if (Mouse.current.rightButton.wasPressedThisFrame && fireCountdown <= 0f) {
            Instantiate(Prefabs[Prefab], FirePoint.transform.position, FirePoint.transform.rotation);
            fireCountdown = 0;
            fireCountdown += hSliderValue;
        }
        fireCountdown -= Time.deltaTime;

        //To change projectiles
        if (Keyboard.current.aKey.wasPressedThisFrame && buttonSaver >= 0.4f)// left button
        {
            buttonSaver = 0f;
            Counter(-1);
        }
        if (Keyboard.current.dKey.wasPressedThisFrame && buttonSaver >= 0.4f)// right button
        {
            buttonSaver = 0f;
            Counter(+1);
        }
        buttonSaver += Time.deltaTime;

        //To rotate Fire point
        if (Cam != null) {
            var mousePos = Mouse.current.position.value;
            RayMouse = Cam.ScreenPointToRay(mousePos);
            if (Physics.Raycast(RayMouse.origin, RayMouse.direction, out var hit, MaxLength)) {
                RotateToMouseDirection(gameObject, hit.point);
            }
        } else {
            Debug.Log("No camera");
        }
    }

    //GUI Text
    private void OnGUI() {
        GUI.Label(new Rect(10 * windowDpi, 5 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Use left mouse button to single shoot!");
        GUI.Label(new Rect(10 * windowDpi, 25 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Use and hold the right mouse button for quick shooting!");
        GUI.Label(new Rect(10 * windowDpi, 45 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Fire rate:");
        hSliderValue = GUI.HorizontalSlider(new Rect(70 * windowDpi, 50 * windowDpi, 100 * windowDpi, 20 * windowDpi), hSliderValue, 0.0f, 1.0f);
        GUI.Label(new Rect(10 * windowDpi, 65 * windowDpi, 400 * windowDpi, 20 * windowDpi), "Use the keyboard buttons A/<- and D/-> to change projectiles!");
    }

    // To change items (Amount - effectPrefab number)
    private void Counter(int count) {
        Prefab += count;
        if (Prefab > Prefabs.Length - 1) {
            Prefab = 0;
        } else if (Prefab < 0) {
            Prefab = Prefabs.Length - 1;
        }
    }

    //To rotate Fire point
    private void RotateToMouseDirection(GameObject obj, Vector3 destination) {
        direction = destination - obj.transform.position;
        rotation = Quaternion.LookRotation(direction);
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);
    }
}
