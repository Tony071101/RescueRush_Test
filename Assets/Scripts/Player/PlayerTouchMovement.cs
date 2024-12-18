using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerTouchMovement : MonoBehaviour
{
    public static PlayerTouchMovement Instance { get; private set; }
    [SerializeField] private Vector2 JoystickSize = new Vector2(100, 100);
    [SerializeField] private FloatingJoystick joyStick;
    
    private NavMeshAgent player;
    private Animator _anim;
    private Finger movementFinger;
    private Vector2 movementAmount;
    private bool autoMovingInPhase2 = false;
    private float defaultSpeed;

    private void Awake() {
        if(Instance != null) {
            Destroy(this.gameObject);
        }

        Instance = this;
    }

    private void Start() {
        player = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        player.speed = PlayerPrefs.GetFloat("PlayerSpeed", 2.5f);
        defaultSpeed = player.speed;
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
        if(GameManager.Instance.CurrentState == GameManager.GameState.Phase_Two || GameManager.Instance.isChangingSpeed == true) {
            ChangePlayerSpeed(GameManager.Instance.speedBoostMultiplier);
            GameManager.Instance.SpawnFloatingText(TouchedFinger.screenPosition);
        }

        if(GameManager.Instance.CurrentState != GameManager.GameState.Phase_One) {
            return;
        }

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
        PlayerAutoMove();
    }

    private void PlayerMove(){
        if(GameManager.Instance.CurrentState != GameManager.GameState.Phase_One) {
            _anim.SetFloat("Velocity", 0);
            return;
        }

        Vector3 scaledMovement = player.speed * Time.deltaTime * new Vector3(movementAmount.x, 0, movementAmount.y);
        if(movementAmount.sqrMagnitude > 0) {
            player.transform.LookAt(player.transform.position + scaledMovement, Vector3.up);
            player.Move(scaledMovement);
            _anim.SetFloat("Velocity", movementAmount.magnitude);
        } else {
            _anim.SetFloat("Velocity", 0);
        }
    }

    private void PlayerAutoMove() {
        if(GameManager.Instance.CurrentState != GameManager.GameState.Phase_Two) {
            return;
        }

        if(!autoMovingInPhase2) {
            transform.position = new Vector3(50f, 0f, 80f);
            autoMovingInPhase2 = true;
        }

        transform.rotation = Quaternion.identity;
        Vector3 forwardDirection = Vector3.forward;
        player.Move(forwardDirection * player.speed * Time.deltaTime);

        _anim.SetFloat("Velocity", player.speed);
    }

    public void ChangePlayerSpeed(float speedMultiplier) {
        player.speed += speedMultiplier;
        player.speed = Mathf.Round(player.speed * 10f) / 10f;
    }

    public void ResetDefaultSpeed() {
        player.speed = defaultSpeed;
    }

    public float GetPlayerSpeed() {
        return player.speed;
    }
    
    public void SetPlayerSpeed(float upgradeSpeed) {
        upgradeSpeed = Mathf.Round(upgradeSpeed * 10f) / 10f;
        PlayerPrefs.SetFloat("PlayerSpeed", upgradeSpeed);
        PlayerPrefs.Save();
        player.speed = upgradeSpeed;
        defaultSpeed = upgradeSpeed;
    }
}
