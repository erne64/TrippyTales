using UnityEngine;

// Ruota costantemente un oggetto attorno ai suoi assi.
public class RotateItem : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 100f, 0f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
