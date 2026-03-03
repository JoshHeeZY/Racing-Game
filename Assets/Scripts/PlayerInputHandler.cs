using UnityEngine;
using UnityEngine.InputSystem;

namespace WanganMidnight
{
    /// <summary>
    /// Reads from the Input System and forwards normalized values to CarController.
    /// Disable this component to lock inputs during the race start sequence.
    /// </summary>
    [RequireComponent(typeof(CarController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction _throttleAction;
        private InputAction _steerAction;
        private InputAction _brakeAction;

        private CarController _carController;

        public float Throttle { get; private set; }
        public float Steer { get; private set; }
        public bool Brake { get; private set; }

        private void Awake()
        {
            _carController = GetComponent<CarController>();

            InputActionMap drivingMap = inputActions.FindActionMap("Driving", throwIfNotFound: true);
            _throttleAction = drivingMap.FindAction("Throttle", throwIfNotFound: true);
            _steerAction = drivingMap.FindAction("Steer", throwIfNotFound: true);
            _brakeAction = drivingMap.FindAction("Brake", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            _throttleAction?.Enable();
            _steerAction?.Enable();
            _brakeAction?.Enable();
        }

        private void OnDisable()
        {
            _throttleAction?.Disable();
            _steerAction?.Disable();
            _brakeAction?.Disable();

            // Zero out inputs when disabled so the car doesn't keep moving.
            Throttle = 0f;
            Steer = 0f;
            Brake = false;
            _carController?.SetInputs(0f, 0f, false);
        }

        private void Update()
        {
            Throttle = _throttleAction.ReadValue<float>();
            Steer = _steerAction.ReadValue<float>();
            Brake = _brakeAction.IsPressed();

            _carController.SetInputs(Throttle, Steer, Brake);
        }
    }
}
