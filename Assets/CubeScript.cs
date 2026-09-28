using UnityEngine;

/// <summary>
/// Roulement d'un cube à l'aide de quaternions.
/// - Flèches : déplacement dans les 4 directions (le cube roule)
/// - Spacebar : rotation sur place (yaw) via quaternion
/// Le cube roule sur le plan XZ (sol), autour de l'axe perpendiculaire
/// à la direction de déplacement.
/// </summary>
public class CubeRollingController : MonoBehaviour
{
    [Header("Paramètres de déplacement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Paramètres de rotation sur place")]
    [SerializeField] private float yawSpeed = 90f; // degrés par seconde

    [Header("Taille du cube")]
    [Tooltip("Demi-côté du cube (pour le calcul du roulement). " +
             "Pour un cube de taille 1, mettre 0.5.")]
    [SerializeField] private float halfSize = 0.5f;

    // Orientation courante du cube (état interne)
    private Quaternion currentRotation = Quaternion.identity;

    private void Start()
    {
        currentRotation = transform.rotation;
    }

    private void Update()
    {
        HandleRolling();
        HandleYawRotation();
    }

    /// <summary>
    /// Déplacement + roulement dans les 4 directions avec les flèches.
    /// </summary>
    private void HandleRolling()
    {
        // Direction dans le repère LOCAL du cube (pour tenir compte du yaw)
        Vector3 localDir = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            localDir += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow))
            localDir += Vector3.back;
        if (Input.GetKey(KeyCode.LeftArrow))
            localDir += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow))
            localDir += Vector3.right;

        if (localDir.sqrMagnitude < 0.0001f)
            return;

        localDir.Normalize();

        // 1) Déplacement effectif (dans le repère monde)
        Vector3 worldDir = transform.TransformDirection(localDir);
        float distance = moveSpeed * Time.deltaTime;
        transform.position += worldDir * distance;

        // 2) Calcul de l'angle de roulement : θ = d / r
        float angleDeg = (distance / halfSize) * Mathf.Rad2Deg;
        float halfAngleRad = angleDeg * Mathf.Deg2Rad * 0.5f;
        float sinHalf = Mathf.Sin(halfAngleRad);
        float cosHalf = Mathf.Cos(halfAngleRad);

        // 3) Détermination de l'axe de roulement (repère LOCAL du cube)
        //    - Avant  (+Z) → axe +X
        //    - Arrière(-Z) → axe -X
        //    - Droite (+X) → axe -Z
        //    - Gauche (-X) → axe +Z
        Vector3 localAxis;
        if (localDir == Vector3.forward)
            localAxis = Vector3.right;
        else if (localDir == Vector3.back)
            localAxis = Vector3.left;
        else if (localDir == Vector3.right)
            localAxis = Vector3.back;
        else // localDir == Vector3.left
            localAxis = Vector3.forward;

        // 4) Construction du quaternion de rotation (sans Quaternion.AngleAxis)
        //    q = (sin(θ/2) * axe, cos(θ/2))
        Quaternion rollDelta = new Quaternion(
            localAxis.x * sinHalf,
            localAxis.y * sinHalf,
            localAxis.z * sinHalf,
            cosHalf
        );

        // 5) Composition : on applique la rotation dans le repère LOCAL
        //    (pré-multiplication car l'axe est exprimé en local)
        currentRotation = transform.rotation * rollDelta;

        // 6) Application au transform
        transform.rotation = currentRotation;
    }

    /// <summary>
    /// Rotation sur place (yaw) autour de l'axe Y avec la barre espace.
    /// Construite manuellement en quaternion, sans Quaternion.Euler.
    /// </summary>
    private void HandleYawRotation()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            float angleDeg = yawSpeed * Time.deltaTime;
            float halfAngleRad = angleDeg * Mathf.Deg2Rad * 0.5f;

            float w = Mathf.Cos(halfAngleRad);
            float y = Mathf.Sin(halfAngleRad);

            // Quaternion de rotation autour de Y (up) : (0, sin(θ/2), 0, cos(θ/2))
            Quaternion yawDelta = new Quaternion(0f, y, 0f, w);

            // Rotation dans le repère LOCAL (post-multiplication)
            currentRotation = currentRotation * yawDelta;
            transform.rotation = currentRotation;
        }
    }
}