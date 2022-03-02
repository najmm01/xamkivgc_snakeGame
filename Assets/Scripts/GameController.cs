using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    private const float WallDelta = 0.4f;
    private const int NewPartsCount = 5;
    private const float PartsAddDuration = 0.5f;
    internal static float SnakeSpeed = 1.0f;
    internal static bool Alive = false;

    private Vector2 upperLeft, upperRight, lowerLeft, lowerRight;
    [SerializeField] private float innerAreaOffset = 2.0f;
    [SerializeField] private float boundaryOffset = 5.0f;
    [SerializeField] private float propMinOffset = 1.0f;
    [SerializeField] private Transform snake;
    [SerializeField] private SnakeHead snakeHead;
    private FollowerPart snakeTail;
    [SerializeField] private FollowerPart followerPartPrefab;
    [SerializeField] private Sprite bodyPartSprite;
    [SerializeField] private Sprite tailSprite;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private GameObject tapText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private TextMeshProUGUI scoreText, hiScoreText, levelText;
    private int newPartsRequestCount = 0;
    private Coroutine newPartsRoutine;
    [SerializeField] private GameObject eggPrefab, goldenEggPrefab, spikePrefab;
    private int levelEggCount;
    private int level, score, hiScore;
    private List<GameObject> resettableObjects = new List<GameObject>();
    [Space]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] gulpSounds;
    [SerializeField] private AudioClip deathSound;

    public int NewPartsRequestCount
    {
        get => newPartsRequestCount;

        set
        {
            if (value < 0) return; //safety
            if (value < newPartsRequestCount) newPartsRoutine = null; //coroutine was completed

            newPartsRequestCount = value;

            if (newPartsRequestCount > 0 && newPartsRoutine == null)
            {
                newPartsRoutine = StartCoroutine(AddSnakeParts());
            }
        }
    }

    private int Level
    {
        get => level;

        set
        {
            level = value;
            levelEggCount = 4 + level * 2;
            SnakeSpeed = Mathf.Min(1 + level / 2.0f, 6);

            levelText.text = $"Level: {level}";
        }
    }

    private int Score
    {
        get => score;
        set
        {
            score = value;
            scoreText.text = $"Score: {score}";

            if (score > HiScore) HiScore = score;
        }
    }

    private int HiScore
    {
        get => hiScore;
        set
        {
            hiScore = value;
            hiScoreText.text = $"HiScore: {hiScore}";
        }
    }

    internal void GameOver()
    {
        audioSource.PlayOneShot(deathSound);

        tapText.SetActive(true);
        gameOverText.SetActive(true);

        StopNewPartsRequests();

        Alive = false;
    }

    internal void EatEgg(GameObject egg)
    {
        Score++;

        audioSource.PlayOneShot(gulpSounds[Random.Range(0, gulpSounds.Length)]);

        Destroy(egg);
        NewPartsRequestCount++;
        levelEggCount--;

        if (levelEggCount == 0) LevelUp();
        else if (levelEggCount == 1) CreateProp(goldenEggPrefab);
        else CreateProp(eggPrefab);
    }

    private void CreateProp(GameObject prefab)
    {
        while (true)
        {
            var pos = GetRandomPosition();
            if (Physics2D.OverlapCircle(pos, propMinOffset) != null) continue;

            GameObject prop = Instantiate(prefab, pos, Quaternion.identity);
            resettableObjects.Add(prop);
            break;
        }
    }

    private Vector2 GetRandomPosition()
    {
        Vector2 position = Vector2.zero;
        switch (Random.Range(1, 5))
        {
            case 1:
                //area 1
                position.x = Random.Range(upperLeft.x + boundaryOffset, -innerAreaOffset);
                position.y = Random.Range(lowerLeft.y + boundaryOffset, upperLeft.y - boundaryOffset);
                break;
            case 2:
                //area 2
                position.x = Random.Range(innerAreaOffset, upperRight.x - boundaryOffset);
                position.y = Random.Range(lowerRight.y + boundaryOffset, upperRight.y - boundaryOffset);
                break;
            case 3:
                //area 3
                position.x = Random.Range(-innerAreaOffset, innerAreaOffset);
                position.y = Random.Range(innerAreaOffset, upperLeft.y - boundaryOffset);
                break;
            case 4:
                //area 4
                position.x = Random.Range(-innerAreaOffset, innerAreaOffset);
                position.y = Random.Range(-innerAreaOffset, lowerLeft.y + boundaryOffset);
                break;
        }

        return position;
    }

    private void LevelUp()
    {
        Level++;

        ResetGame();
    }

    private void Awake()
    {
        //turn off V-Sync in order to get Unity to adhere to the target framerate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        tapText.SetActive(true);
        gameOverText.SetActive(false);

        //calculate screen bounds
        var upperLeftScreen = new Vector2(0, Screen.height);
        var upperRightScreen = new Vector2(Screen.width, Screen.height);
        var lowerLeftScreen = new Vector2(0, 0);
        var lowerRightScreen = new Vector2(Screen.width, 0);
        var camera = Camera.main;
        upperLeft = camera.ScreenToWorldPoint(upperLeftScreen);
        upperRight = camera.ScreenToWorldPoint(upperRightScreen);
        lowerLeft = camera.ScreenToWorldPoint(lowerLeftScreen);
        lowerRight = camera.ScreenToWorldPoint(lowerRightScreen);

        CreateWalls();
    }

    private void ResetGame()
    {
        StopNewPartsRequests();

        //reset snake
        snakeHead.ResetMovement();
        snakeTail = null;

        //clean up eggs, snake parts, spikes
        foreach (var obj in resettableObjects)
        {
            Destroy(obj);
        }
        resettableObjects.Clear();

        CreateProp(eggPrefab);

        CreateProp(spikePrefab);

        NewPartsRequestCount = 1; //add parts at the very start
    }

    private void CreateWalls()
    {
        CreateWall(upperLeft, upperRight, new Vector2(WallDelta, 0)); //topWall
        CreateWall(upperRight, lowerRight, new Vector2(0, -WallDelta)); //topWall
        CreateWall(lowerRight, lowerLeft, new Vector2(-WallDelta, 0)); //topWall
        CreateWall(lowerLeft, upperLeft, new Vector2(0, WallDelta)); //topWall
    }

    private void CreateWall(Vector2 start, Vector2 end, Vector2 delta)
    {
        int count = Mathf.CeilToInt(Vector2.Distance(start, end) / WallDelta);
        for (int i = 0; i < count; i++)
        {
            var randomRot = Random.Range(-180, 180);
            var randomScale = Random.Range(1f, 2.5f);
            var rock = Instantiate(rockPrefab, start, Quaternion.Euler(0, 0, randomRot), transform);
            rock.transform.localScale = new Vector3(randomScale, randomScale, 1);

            start += delta;
        }
    }

    private void StopNewPartsRequests()
    {
        if (newPartsRoutine != null) StopCoroutine(newPartsRoutine);

        NewPartsRequestCount = 0;
    }

    private IEnumerator AddSnakeParts()
    {
        BodyPart lastTarget = snakeHead;
        if (snakeTail != null)
        {
            snakeTail.SetSprite(bodyPartSprite);
            lastTarget = snakeTail;
        }

        var delay = new WaitForSeconds(PartsAddDuration / NewPartsCount);
        for (int partsToAdd = NewPartsCount; partsToAdd > 0; partsToAdd--)
        {
            yield return delay;

            var part = Instantiate(followerPartPrefab,
                lastTarget.transform.position, lastTarget.transform.rotation, snake);
            part.SortingOrder = lastTarget.SortingOrder - 1;
            part.Target = lastTarget;
            lastTarget = part;

            resettableObjects.Add(part.gameObject);
        }

        snakeTail = lastTarget as FollowerPart;
        snakeTail.SetSprite(tailSprite);

        NewPartsRequestCount--;
    }

    //from gameover or in the beginning
    private void NewGame()
    {
        tapText.SetActive(false);
        gameOverText.SetActive(false);
        Level = 1;
        Score = 0;
        ResetGame();

        Alive = true;
    }

    private void Update()
    {
#if !UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
#endif

        if (Alive) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Ended)
            {
                NewGame();
                break;
            }
        }
#else
        if (Input.GetMouseButtonUp(0))
        {
            NewGame();
        }
#endif
    }
}
