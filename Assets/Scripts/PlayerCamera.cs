using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Player player;
    private readonly float _moveDelay = 0.5f, _rotDelay = 0.5f;

    private Vector3 _currentPos;
    private float _lastPlayerY;

    // careful, these vectors components are handled differently
    private Vector3 _currentRot, _startRot, _endRot;
    private float _startRotTime;

    public void TargetRot(float y)
    {
        // schedule a rotation about the Y axis
        _startRot.y = transform.rotation.eulerAngles.y;
        _endRot.y = y;
        
        // avoid modulo issues
        if (_startRot.y - y > 180) _endRot.y += 360;
        else if (y - _startRot.y > 180) _startRot.y += 360;
        _startRotTime = Time.time;
    }

    private float SmoothStep(float t)
    {
        // use Perlin's smootherStep function
        return t * t * t * (t * (t * 6 - 15) + 10);
    }
    
    void Start()
    {
        // force starting position to avoid clipping in ground
        _currentPos = player.transform.position + new Vector3(0, 3, 0);
        _startRot = _currentRot = _endRot = new Vector3(45, 0, 0);
        _startRotTime = Time.time;
    }

    void Update()
    {
        Transform tr = transform;
        Vector3 pPos = player.transform.position;
        Vector3 m = player.Body.Movement;

        // position and X rotation
        // don't update if player is moving up, or jumping into no above block
        float dy = player.Body.Movement.y;
        if (dy == 0 || (dy < 0 && pPos.y < player.GroundedHeight)) _lastPlayerY = player.transform.position.y;

        // edit position: use last Y, shift camera when walking
        pPos.y = _lastPlayerY + player.Body.MoveRelative.z * (m.x * m.x + m.z * m.z) * 10;

        float fps = Time.deltaTime == 0 ? 10e6f : _moveDelay / Time.deltaTime;
        _currentPos = (_currentPos * (fps - 1) + pPos) / fps;
        _currentRot.x = (_currentRot.x * (fps - 1) + 100 - _lastPlayerY * 10) / fps;
        
        // Y rotation
        float end = _startRotTime+_rotDelay;
        if (Time.time < end)
        {
            float t = SmoothStep((Time.time - _startRotTime) / _rotDelay);
            _currentRot.y = _startRot.y + (_endRot.y - _startRot.y) * t;
        }
        transform.rotation = Quaternion.Euler(_currentRot);

        // also back up to avoid clipping in the ground
        tr.position = _currentPos + tr.rotation * new Vector3(0, 0, -20);
    }
}
