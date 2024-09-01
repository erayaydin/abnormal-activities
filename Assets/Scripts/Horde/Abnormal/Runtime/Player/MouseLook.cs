using Horde.Abnormal.Input;
using Horde.Abnormal.Shared;
using UnityEngine;

namespace Horde.Abnormal.Player
{
    /// <summary>
    /// Handles mouse look functionality for rotating the player and the camera from a first-person perspective.
    /// </summary>
    public class MouseLook : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The main camera used for rotation.
        /// </summary>
        [SerializeField]
        [Tooltip("The main camera to use for rotation.")]
        private Camera mainCamera;

        /// <summary>
        /// The sensitivity of the mouse input.
        /// </summary>
        [SerializeField]
        [Tooltip("The sensitivity of the mouse input.")]
        private Vector2 sensitivity;

        /// <summary>
        /// The minimum angle limits for the camera rotation.
        /// </summary>
        [SerializeField]
        [Tooltip("The minimum angle limits for the camera rotation.")]
        private Vector2 minimumLookLimit;

        /// <summary>
        /// The maximum angle limits for the camera rotation.
        /// </summary>
        [SerializeField]
        [Tooltip("The maximum angle limits for the camera rotation.")]
        private Vector2 maximumLookLimit;

        /// <summary>
        /// The player game object, used to rotate the player body.
        /// </summary>
        private GameObject _player;
        
        /// <summary>
        /// The current rotation angles for the camera in x and y directions.
        /// </summary>
        private Vector2 _rotation = Vector2.zero;
        
        /// <summary>
        /// The original rotation of the player, used for calculating relative rotation.
        /// </summary>
        private Quaternion _playerOriginalRot;

        /// <summary>
        /// The original rotation of the camera, used for calculating relative rotation.
        /// </summary>
        private Quaternion _originalRot;

        #endregion

        #region Unity Events

        /// <summary>
        /// Initializes the main camera and player object references.
        /// </summary>
        private void Awake()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }

            _player = transform.root.gameObject;
        }

        /// <summary>
        /// Locks the cursor and initializes the original rotation values for the player and the camera.
        /// </summary>
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            _originalRot = transform.localRotation;
            _playerOriginalRot = _player.transform.localRotation;
        }

        /// <summary>
        /// Handles the mouse input to rotate the camera and the player character based on user input.
        /// </summary>
        private void Update()
        {
            var lookDelta = InputHandler.ReadInput<Vector2>("Look", "Player");
            
            if (Cursor.lockState == CursorLockMode.None)
                return;
            
            _rotation.x += lookDelta.x * sensitivity.x / 30 * mainCamera.fieldOfView;
            _rotation.y += lookDelta.y * sensitivity.y / 30 * mainCamera.fieldOfView;

            _rotation.x = _rotation.x.Normalize().Clamp(minimumLookLimit.x, maximumLookLimit.x);
            _rotation.y = _rotation.y.Normalize().Clamp(minimumLookLimit.y, maximumLookLimit.y);
            
            var xQuaternion = Quaternion.AngleAxis(_rotation.x, Vector3.up);
            var yQuaternion = Quaternion.AngleAxis(_rotation.y, Vector3.left);

            var playerRotation = _playerOriginalRot * xQuaternion;
            var lookRotation = _originalRot * yQuaternion;
            
            _player.transform.localRotation = playerRotation;
            transform.localRotation = lookRotation;
        }

        #endregion
    }
}