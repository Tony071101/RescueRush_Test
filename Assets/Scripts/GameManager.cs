using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState { Default, GameStart, Phase_One, Phase_Two, GameEnd }
    public GameState CurrentState { get; private set; }
    [SerializeField] private Transform[] catLocations;
    [SerializeField] private GameObject[] catPrefabs;
    [SerializeField] private PlayableDirector gameStartTimeline;

    [Header("UIs")]
    [SerializeField] private GameObject gameStartBtn;
    [SerializeField] private GameObject upgradeSpeedBtn;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private TextMeshProUGUI coinCurrency_txt;
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private GameObject notEnoughMoneyTextPrefab;
    [SerializeField] private TextMeshProUGUI coinEarned_txt;
    [SerializeField] private TextMeshProUGUI catSaved_txt;
    [SerializeField] private TextMeshProUGUI countDown_txt;
    public float speedBoostMultiplier { get; private set; } = 0.1f;
    public bool isChangingSpeed { get; private set; } = false;
    private int boostDuration = 5;
    private float coinCurrency;
    private float newCoinCurrency;
    private float upgradeCost;
    private MovingCube movingCube;
    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
        }

        Instance = this;

        GenerateCats();
        ResetCinemachine();
        coinCurrency = PlayerPrefs.GetFloat("CoinCurrency");
        coinCurrency = Mathf.Round(coinCurrency * 10f) / 10f;
        newCoinCurrency = 0;
    }

    private void Start() {
        ChangeState(GameState.Default);
        movingCube = FindObjectOfType<MovingCube>();
        gameStartTimeline.stopped += OnGameStartTimelineFinished;

        upgradeCost = PlayerPrefs.GetFloat("UpgradeCost", 100f);
    }

    private void Update() {
        upgradeSpeedBtn.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = PlayerTouchMovement.Instance.GetPlayerSpeed().ToString();
        upgradeSpeedBtn.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = upgradeCost.ToString();
        coinCurrency_txt.text = "Coin: " + coinCurrency.ToString();
    } 

    private void FixedUpdate() {
        if(CurrentState == GameState.Phase_One || CurrentState == GameState.Phase_Two) {
            newCoinCurrency += 0.1f;
            newCoinCurrency = Mathf.Round(newCoinCurrency * 10f) / 10f;
        }
    }  

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        HandleGameStateChanged(newState);
    }

    private void HandleGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Default:
                break;
            case GameState.GameStart:
                gameStartTimeline.Play();
                movingCube.moveSpeed = 15f;
                Debug.Log("Game Start.");
                break;
            case GameState.Phase_One:
                movingCube.moveSpeed = 11f;
                Debug.Log("Phase 1.");
                break;
            case GameState.Phase_Two:
                HandlePhaseTwo();
                Debug.Log("Phase 2.");
                break;
            case GameState.GameEnd:
                PlayerTouchMovement.Instance.ResetDefaultSpeed();
                SaveCurrency(newCoinCurrency);
                ShowPanel();
                movingCube.moveSpeed = 0f;
                Debug.Log("Game End.");
                break;
        }
    }

    private void GenerateCats() {
        for (int i = 0; i < catLocations.Length; i++) {
            int s = Random.Range(0, catPrefabs.Length);

            Instantiate(catPrefabs[s], catLocations[i].position, Quaternion.identity);
        }
    }

    public void GameStartBtn() {
        gameStartBtn.SetActive(false);
        upgradeSpeedBtn.SetActive(false);
        coinCurrency_txt.enabled = false;
        ChangeState(GameState.GameStart);
    }

    public void OnGameStartTimelineFinished(PlayableDirector director)
    {
        if (director == gameStartTimeline)
        {
            StartCoroutine(ChangeToPhase1State(3f));
        }
    }

    public IEnumerator ChangeToPhase1State(float waitTime){
        isChangingSpeed = true;
        countDown_txt.gameObject.SetActive(true);
        for (int i = (int)waitTime; i > 0; i--)
        {
            countDown_txt.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countDown_txt.gameObject.SetActive(false);

        PlayerTouchMovement.Instance.ChangePlayerSpeed(speedBoostMultiplier);
        ChangeState(GameState.Phase_One);
        isChangingSpeed = false;

        yield return new WaitForSeconds(boostDuration);

        PlayerTouchMovement.Instance.ResetDefaultSpeed();
    }

    private void HandlePhaseTwo() {
        var adjustTranposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if(adjustTranposer != null) {
            adjustTranposer.m_FollowOffset = new Vector3(3f, 1f, 3f);
        }
    }

    private void ResetCinemachine() {
        var adjustTranposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if(adjustTranposer != null) {
            adjustTranposer.m_FollowOffset = new Vector3(0f, 3f, -3f);
        }
    }

    public void CalculateUpgradeSpeed() {
        if(coinCurrency >= upgradeCost) {
            float updateSpeed = PlayerTouchMovement.Instance.GetPlayerSpeed();
            updateSpeed += speedBoostMultiplier;
            PlayerTouchMovement.Instance.SetPlayerSpeed(updateSpeed);

            coinCurrency -= upgradeCost;
            coinCurrency = Mathf.Round(coinCurrency * 10f) / 10f;
            SaveCurrency(-upgradeCost);

            upgradeCost *= 1.1f; // This will increase the cost by 10% each time
            upgradeCost = Mathf.Round(upgradeCost * 10f) / 10f;

            PlayerPrefs.SetFloat("UpgradeCost", upgradeCost);
            PlayerPrefs.Save();

        } else {
            Instantiate(notEnoughMoneyTextPrefab, notEnoughMoneyTextPrefab.transform.position, Quaternion.identity);
        }
    }

    private void OnDestroy()
    {
        gameStartTimeline.stopped -= OnGameStartTimelineFinished;
    }

    private void SaveCurrency(float coinCurrency) {
        float currentCoinCurrency = PlayerPrefs.GetFloat("CoinCurrency", 0f);
    
        currentCoinCurrency += coinCurrency;
        
        PlayerPrefs.SetFloat("CoinCurrency", currentCoinCurrency);
        PlayerPrefs.Save();
    }

    public void SpawnFloatingText(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane + 3f));
        Instantiate(floatingTextPrefab, worldPosition, Camera.main.transform.rotation);
    }

    private void ShowPanel() {
        gameEndPanel.SetActive(true);
        coinEarned_txt.text = "Coin earned: " + newCoinCurrency.ToString();
        ColliderCheck colliderCheck = FindObjectOfType<ColliderCheck>();
        if (colliderCheck != null)
        {
            int totalCatsSaved = colliderCheck.GetTotalCatsSaved();
            catSaved_txt.text = "Cat saved: " + totalCatsSaved.ToString();
        }
        else
        {
            catSaved_txt.text = "Cat saved: 0";
        }
    }

    public void Quit() {
        Application.Quit();
    }

    public void RestartGame() {
        ChangeState(GameState.Default);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
