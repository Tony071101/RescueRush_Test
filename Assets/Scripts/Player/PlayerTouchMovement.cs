using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerTouchMovement : MonoBehaviour
{
    [SerializeField] private Vector2 JoystickSize = new Vector2(100, 100);
    [SerializeField] private FloatingJoystick joyStick;
    
    private NavMeshAgent player;
    private Animator _anim;
    private Finger movementFinger;
    private Vector2 movementAmount;

    private void Start() {
        player = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
    }

    private void OnEnable() {
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleLoseFinger;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable() {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleLoseFinger;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable();
    }

    private void HandleFingerDown(Finger TouchedFinger)
    {
        if(movementFinger == null && TouchedFinger.screenPosition.y <= Screen.height / 2f) {
            movementFinger = TouchedFinger;
            movementAmount = Vector2.zero;
            joyStick.gameObject.SetActive(true);
            joyStick.rectTransform.sizeDelta = JoystickSize;
            joyStick.rectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
        }
    }

    private void HandleLoseFinger(Finger LostFinger)
    {
        if(LostFinger == movementFinger) {
            movementFinger = null;
            joyStick.knob.anchoredPosition = Vector2.zero;
            joyStick.gameObject.SetActive(false);
            movementAmount = Vector2.zero;
        }
    }

    private void HandleFingerMove(Finger MovedFinger)
    {
        if(MovedFinger == movementFinger) {
            Vector2 knobPosition;
            float maxMovement = JoystickSize.x / 2f;
            ETouch.Touch currentTouch = MovedFinger.currentTouch;

            if(Vector2.Distance(currentTouch.screenPosition, joyStick.rectTransform.anchoredPosition) > maxMovement) {
                knobPosition = (currentTouch.screenPosition - joyStick.rectTransform.anchoredPosition).normalized * maxMovement;
            }
            else {
                knobPosition = currentTouch.screenPosition - joyStick.rectTransform.anchoredPosition;
            }

            joyStick.knob.anchoredPosition = knobPosition;
            movementAmount = knobPosition / maxMovement;
        }
    }

    private Vector2 ClampStartPosition(Vector2 startPosition) {
        if(startPosition.x < JoystickSize.x / 2) {
            startPosition.x = JoystickSize.x / 2;
        } else if (startPosition.x > Screen.width - JoystickSize.x / 2) {
            startPosition.x = Screen.width - JoystickSize.x / 2;
        }
        
        if(startPosition.y < JoystickSize.y / 2) {
            startPosition.y = JoystickSize.y / 2;
        } else if (startPosition.y > Screen.height - JoystickSize.y / 2) {
            startPosition.y = Screen.height - JoystickSize.y / 2;
        }

        return startPosition;
    }

    private void Update() {
        PlayerMove();
    }

    private void PlayerMove(){
        Vector3 scaledMovement = player.speed * Time.deltaTime * new Vector3(movementAmount.x, 0, movementAmount.y);
        if(movementAmount.sqrMagnitude > 0) {
            player.transform.LookAt(player.transform.position + scaledMovement, Vector3.up);
            player.Move(scaledMovement);
            _anim.SetFloat("Velocity", movementAmount.magnitude);
        } else {
            _anim.SetFloat("Velocity", 0);
        }
    }
}
