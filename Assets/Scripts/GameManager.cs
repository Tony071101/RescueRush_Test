using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState { GameStart, Phase_One, Phase_Two, GameEnd }
    public GameState CurrentState { get; private set; }
    [SerializeField] private Transform[] catLocations;
    [SerializeField] private GameObject[] catPrefabs;
    [SerializeField] private PlayableDirector gameStartTimeline;
    [SerializeField] private GameObject gameStartBtn;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
        }

        Instance = this;

        GenerateCats();
        ResetCinemachine();
    }

    private void Start() {
        gameStartTimeline.stopped += OnGameStartTimelineFinished;
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
            case GameState.GameStart:
                gameStartTimeline.Play();
                Debug.Log("Game Start.");
                break;
            case GameState.Phase_One:
                Debug.Log("Phase 1.");
                break;
            case GameState.Phase_Two:
                HandlePhaseTwo();
                Debug.Log("Phase 2.");
                break;
            case GameState.GameEnd:
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
        ChangeState(GameState.GameStart);
    }

    private void OnGameStartTimelineFinished(PlayableDirector director)
    {
        if (director == gameStartTimeline)
        {
            StartCoroutine(ChangeToPhase1State(3f));
        }
    }

    private IEnumerator ChangeToPhase1State(float waitTime){
        yield return new WaitForSeconds(waitTime);

        ChangeState(GameState.Phase_One);
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

    private void OnDestroy()
    {
        gameStartTimeline.stopped -= OnGameStartTimelineFinished;
    }
}
