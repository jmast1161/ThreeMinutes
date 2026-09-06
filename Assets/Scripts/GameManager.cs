using System.Collections;
using TMPro;
using UnityEngine;

public enum GameState
{
    Initialize,
    AwaitStart,
    WaitingGameplayInput
}

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Player player;

    private GameState gameState;

    [SerializeField]
    private GameObject coolantPrefab;

    [SerializeField]
    private GameObject wastePrefab;

    private const float spawnInterval = 1f;
    private int spawnWasteCounter = 0;
    private int spawnWateInterval = 0;
    private bool spawnHazards = true;
    private Coroutine spawnCoroutine;

    [SerializeField]
    private float remainingCoolant = 20;

    [SerializeField]
    private TextMeshProUGUI coolantCountText;

    [SerializeField]
    private float remainingTime = 180;
    
    [SerializeField]
    private TextMeshProUGUI remainingTimeText;
   
    [SerializeField]
    private TextMeshProUGUI winText;
    
    [SerializeField]
    private TextMeshProUGUI loseText;

    [SerializeField]
    private UnityEngine.UI.Button readyButton;

    [SerializeField]
    private AudioSource winSound;

    [SerializeField]
    private AudioSource loseSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameState = GameState.Initialize;
        HideGameEndText();
    }

    private void Initialize()
    {
        gameState = GameState.AwaitStart;
        player.CoolantReceived += OnCoolantReceived;
        player.HazardHit += OnHazardHit;
        player.PlayerReady += OnPlayerReady;
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        StopSpawn();
    }

    private void StopSpawn()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private void GameStart()
    {
        StopSpawn();
        spawnCoroutine = StartCoroutine(SpawnItems());
        spawnWateInterval = Random.Range(2, 5);
        remainingCoolant = 20;
        remainingTime = 180;
        readyButton.gameObject.SetActive(false);
        HideGameEndText();
        gameState = GameState.WaitingGameplayInput;
    }

    private void HideGameEndText()
    {
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
    }

    private void OnReadyButtonClicked() =>
        GameStart();

    // Update is called once per frame
    void Update()
    {
        switch(gameState)
        {
            case GameState.Initialize:
                player.Initialize();
                Initialize();
                break;
            case GameState.AwaitStart:
                break;
            case GameState.WaitingGameplayInput:
                player.MovePlayer();
                UpdateTime();
                UpdateCoolant();
                break;
        }        
    }

    private void GameWin()
    {
        winText.gameObject.SetActive(true);
        winSound.Play();
        GameEnd();
    }

    private void GameLose()
    {
        loseText.gameObject.SetActive(true);
        loseSound.Play();
        GameEnd();
    }

    private void GameEnd()
    {
        gameState = GameState.Initialize;
        readyButton.gameObject.SetActive(true);
    }

    private void UpdateTime()
    {
        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime < 0f) 
            {
                remainingTime = 0f;
                GameWin();
            }
        }

        remainingTimeText.text = ((int)remainingTime).ToString(); 
    }
    
    private void UpdateCoolant()
    {
        if (remainingCoolant > 0f)
        {
            remainingCoolant -= Time.deltaTime;
            if (remainingCoolant < 0f) 
            {
                remainingCoolant = 0f;
                GameLose();
            }
        }

        coolantCountText.text = ((int)remainingCoolant).ToString(); 
    }

    private IEnumerator SpawnItems()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            yield return wait;

            var spawnX = Random.Range(-8, 8);
            var spawnPrefab = spawnWasteCounter == spawnWateInterval && spawnHazards
                ? wastePrefab
                : coolantPrefab;
            Instantiate(spawnPrefab, new Vector2(spawnX, 5), Quaternion.identity);
            
            if (spawnHazards)
            {
                if (spawnWasteCounter >= spawnWateInterval)
                {
                    spawnWateInterval = Random.Range(2, 5);
                    spawnWasteCounter = 0;
                }

                ++spawnWasteCounter;
            }
        }
    }
    
    private void OnCoolantReceived(Player player)
    {
        remainingCoolant += 2;
        coolantCountText.text = ((int)remainingCoolant).ToString();
    }

    private void OnHazardHit(Player player) =>
        spawnHazards = false;

    private void OnPlayerReady(Player player) =>
        spawnHazards = true;
}
