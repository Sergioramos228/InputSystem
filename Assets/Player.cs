using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _takeDistance;
    [SerializeField] private float _holdDistance;
    [SerializeField] private float _throwForce;

    private GameObject _currentObject;
    private PlayerInput _input;
    private Vector2 _direction;
    private Vector2 _rotate;
    private Vector2 _rotation;


    private void Awake()
    {
        _input = new();
        _input.Enable();

        _input.Player.PickUp.performed += ctx => TryPickUp();
        _input.Player.Drop.performed += ctx => Throw(true);
        _input.Player.Throw.performed += ctx => Throw();
        _input.Player.Click.performed += ctx =>
        {
            if (ctx.interaction is MultiTapInteraction)
                Shoot();
        };
    }

    private void Update()
    {
        _rotate = _input.Player.Look.ReadValue<Vector2>();
        _direction = _input.Player.Move.ReadValue<Vector2>();
        Look(_rotate);
        Move(_direction);
    }

    private void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.1)
            return;

        float scaleMoveSpeed = _moveSpeed * Time.deltaTime;
        Vector3 move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, direction.y);
        transform.position += move * scaleMoveSpeed;
    }

    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.1)
            return;

        float scaledRotateSpeed = _rotateSpeed * Time.deltaTime;
        _rotation.y += rotate.x * scaledRotateSpeed;
        _rotation.x = Mathf.Clamp(_rotation.x - rotate.y * scaledRotateSpeed, -90, 90);
        transform.localEulerAngles = _rotation;
    }

    private void TryPickUp()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hitinfo, _takeDistance) && hitinfo.collider.gameObject.isStatic)
        {
            _currentObject = hitinfo.collider.gameObject;

            _currentObject.transform.position = default;
            _currentObject.transform.SetParent(transform, worldPositionStays: false);
            _currentObject.transform.localPosition += new Vector3(0, 0, _holdDistance);

            _currentObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void Throw(bool drop = false)
    {
        _currentObject.transform.parent = null;
        Rigidbody rigibody = _currentObject.GetComponent<Rigidbody>();
        rigibody.isKinematic = false;

        if (!drop)
            rigibody.AddForce(transform.forward * _throwForce, ForceMode.Impulse);
    }

    private void Shoot()
    {
        Debug.Log("Shoot");
    }
}
