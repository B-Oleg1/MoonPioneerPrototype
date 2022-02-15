using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{
    [SerializeField]
    private FixedJoystick _fixedJoystick;

    private Rigidbody _rigidbody;

    private float _playerSpeed = 3;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(_fixedJoystick.Horizontal * _playerSpeed, _rigidbody.velocity.y, _fixedJoystick.Vertical * _playerSpeed);
    }
}