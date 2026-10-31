using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private float _jumpBufferTime = 0.12f;
    [SerializeField] private float _dashBufferTime = 0.12f;
    
    public int Horizontal { get; private set; }
    public int Vertical { get; private set; }
    public bool JumpBuffered => _jumpBufferTimer > 0;
    public bool DashBuffered => _dashBufferTimer > 0f;

    public bool JumpHeld { get; private set; }
    //public bool CrouchHeld { get; private set; }
    
    private float _jumpBufferTimer;
    private float _dashBufferTimer;

    // Update is called once per frame
    private void Update()
    {
        Horizontal = (int)Input.GetAxisRaw("Horizontal");
        Vertical = (int)Input.GetAxisRaw("Vertical");
        
        _jumpBufferTimer = Math.Max(0, _jumpBufferTimer - Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferTimer = _jumpBufferTime;
        }

        _dashBufferTimer = Mathf.Max(
            0f,
            _dashBufferTimer - Time.deltaTime
        );

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _dashBufferTimer = _dashBufferTime;
        }
        
        JumpHeld = Input.GetButton("Jump");
        //CrouchHeld = Input.GetButton("Crouch");
    }

    public bool ConsumeJump()
    {
        if (!JumpBuffered) return false;
        _jumpBufferTimer = 0;
        return true;
    }
    
    public bool ConsumeDash()
    {
        if (!DashBuffered)
            return false;

        _dashBufferTimer = 0f;
        return true;
    }
}
