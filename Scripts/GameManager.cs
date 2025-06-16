using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int initialShapesCount = 39;
    [SerializeField] private int actionBarCapacity = 7;
    [SerializeField] private float spawnDelay = 0.2f;
    public bool IsSpawning { get; private set; }
    public bool isGameOver;

    [Header("References")]
    [SerializeField] private GameObject shapePrefab;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private RectTransform actionBar;
    [SerializeField] private GameObject actionBarItemPrefab;
    [SerializeField] private ShapeTypeAsset[] shapeTypeAssets;
    [SerializeField] private AnimalTypeAsset[] animalTypeAssets;
    [SerializeField] private Color[] borderColors;

    public event System.Action OnTripleRemoved;

    private List<Shape> shapesInField = new List<Shape>();
    private List<ActionBarAnimal> actionBarItems = new List<ActionBarAnimal>();
    private List<ShapeData> allPossibleShapes = new List<ShapeData>();
    private HorizontalLayoutGroup actionBarLayout;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeActionBar();
            InitializeGame();
        }
        else
            Destroy(gameObject);
    }

    private void InitializeActionBar()
    {
        if (actionBar == null)
        {
            Debug.LogError("Action Bar не назначен!");
            return;
        }

        actionBarLayout = actionBar.GetComponent<HorizontalLayoutGroup>();

        if (actionBarLayout == null)
            actionBarLayout = actionBar.gameObject.AddComponent<HorizontalLayoutGroup>();

        actionBarLayout.childAlignment = TextAnchor.MiddleLeft;
        actionBarLayout.spacing = 8f;
        actionBarLayout.childControlHeight = true;
    }

    private void InitializeGame()
    {
        if (shapesInField.Count > 0 || actionBarItems.Count > 0)
        {
            Debug.LogWarning("Game already initialized. Skipping...");
            return;
        }

        if (shapePrefab == null)
        {
            Debug.LogError("Префаб не добавлен!");
            return;
        }

        if (actionBarItemPrefab == null)
        {
            Debug.LogError("Префаб для элементов Action Bar не назначен!");
            return;
        }

        if (shapeTypeAssets == null || shapeTypeAssets.Length == 0)
        {
            Debug.LogError("Shape type assets не существует!");
            return;
        }

        if (animalTypeAssets == null || animalTypeAssets.Length == 0)
        {
            Debug.LogError("Animal type assets не существует!");
            return;
        }

        if (borderColors == null || borderColors.Length == 0)
        {
            Debug.LogError("Не добавлены цвета рамки!");
            return;
        }

        GenerateShapePool();
        GenerateLevel(initialShapesCount);
    }

    private void GenerateShapePool()
    {
        allPossibleShapes.Clear();

        foreach (ShapeTypeAsset shapeType in shapeTypeAssets)
        {
            foreach (AnimalTypeAsset animalType in animalTypeAssets)
            {
                foreach (Color color in borderColors)
                    allPossibleShapes.Add(new ShapeData(shapeType, color, animalType));
            }
        }

        Debug.Log($"Создано {allPossibleShapes.Count} возможных комбинаций!");
    }

    public void GenerateLevel(int shapesCount) => StartCoroutine(SpawnShapesCoroutine(shapesCount));

    private IEnumerator SpawnShapesCoroutine(int shapesToSpawn)
    {
        IsSpawning = true;
        UIManager.Instance?.UpdateReshuffle(false);

        foreach (Shape shape in shapesInField.ToList())
            if (shape != null) Destroy(shape.gameObject);

        foreach (ActionBarAnimal item in actionBarItems.ToList())
            if (item != null) Destroy(item.gameObject);

        shapesInField.Clear();
        actionBarItems.Clear();

        if (shapesToSpawn % 3 != 0)
        {
            shapesToSpawn = Mathf.Max(3, (shapesToSpawn / 3) * 3);
            Debug.LogWarning($"{shapesToSpawn}: число shapesToSpawn должно быть кратно 3!");
        }

        List<ShapeData> shapesToSpawnList = new List<ShapeData>();
        for (int i = 0; i < shapesToSpawn / 3; i++)
        {
            if (allPossibleShapes.Count == 0) break;

            ShapeData randomShape = allPossibleShapes[Random.Range(0, allPossibleShapes.Count)];
            shapesToSpawnList.Add(randomShape);
            shapesToSpawnList.Add(randomShape);
            shapesToSpawnList.Add(randomShape);
        }

        for (int i = 0; i < shapesToSpawnList.Count; i++)
        {
            int randomIndex = Random.Range(i, shapesToSpawnList.Count);
            ShapeData temp = shapesToSpawnList[i];
            shapesToSpawnList[i] = shapesToSpawnList[randomIndex];
            shapesToSpawnList[randomIndex] = temp;
        }

        foreach (ShapeData shapeData in shapesToSpawnList)
        {
            SpawnShape(shapeData);
            yield return new WaitForSeconds(spawnDelay);
        }

        IsSpawning = false;
        UIManager.Instance?.UpdateReshuffle(true);
    }

    private void SpawnShape(ShapeData shapeData)
    {
        if (shapePrefab == null)
            return;

        AudioManager.Instance.PlaySFX("Spawn");

        Vector3 spawnPos = spawnArea.position;
        spawnPos.x += Random.Range(-2f, 2f);
        GameObject shapeObj = Instantiate(shapePrefab, spawnPos, Quaternion.identity);

        if (shapeObj == null) return;

        Shape shape = shapeObj.GetComponent<Shape>();
        if (shape == null)
        {
            Destroy(shapeObj);
            return;
        }

        shape.Initialize(shapeData);
        shape.EnablePhysics();
        shapesInField.Add(shape);
    }

    public void OnShapeClicked(Shape shape)
    {
        if (actionBarItems.Count >= actionBarCapacity || IsSpawning)
            return;

        if (!shapesInField.Contains(shape))
            return;

        AudioManager.Instance.PlaySFX("Click");
        shapesInField.Remove(shape);
        Destroy(shape.gameObject);
        CreateActionBarItem(shape.ShapeData);
        CheckForMatches();
    }

    private void CreateActionBarItem(ShapeData shapeData)
    {
        GameObject itemObj = Instantiate(actionBarItemPrefab, actionBar);
        ActionBarAnimal actionBarItem = itemObj.GetComponent<ActionBarAnimal>();
        actionBarItem.Initialize(shapeData);
        actionBarItems.Add(actionBarItem);
    }

    private void CheckForMatches()
    {
        if (actionBarItems.Count < 3) return;

        int lastIndex = actionBarItems.Count - 1;
        ActionBarAnimal item1 = actionBarItems[lastIndex];
        ActionBarAnimal item2 = actionBarItems[lastIndex - 1];
        ActionBarAnimal item3 = actionBarItems[lastIndex - 2];

        if (item1.ShapeData.Equals(item2.ShapeData) && item1.ShapeData.Equals(item3.ShapeData))
        {
            RemoveActionBarItems(item1, item2, item3);

            if (shapesInField.Count == 0 && actionBarItems.Count == 0)
                ShowWinScreen();
        }
        else if (actionBarItems.Count >= actionBarCapacity)
            ShowLoseScreen();
    }


    private void RemoveActionBarItems(params ActionBarAnimal[] items) => StartCoroutine(RemoveActionBarItemsCoroutine(items));

    private IEnumerator RemoveActionBarItemsCoroutine(ActionBarAnimal[] items)
    {
        AudioManager.Instance?.PlaySFX("Collect");

        foreach (ActionBarAnimal item in items)
        {
            if (actionBarItems.Contains(item))
            {
                actionBarItems.Remove(item);
                Destroy(item.gameObject);
            }
        }

        yield return null;
        OnTripleRemoved?.Invoke();
    }

    public void ReshuffleField()
    {
        if (IsSpawning) return;

        StartCoroutine(ReshuffleCoroutine());
    }

    public bool CanReshuffle() => !IsSpawning && !isGameOver;

    private IEnumerator ReshuffleCoroutine()
    {
        int totalShapes = shapesInField.Count + actionBarItems.Count;

        foreach (ActionBarAnimal item in actionBarItems.ToList())
            if (item != null) Destroy(item.gameObject);

        actionBarItems.Clear();

        foreach (Shape shape in shapesInField.ToList())
            if (shape != null) Destroy(shape.gameObject);

        shapesInField.Clear();

        yield return new WaitForEndOfFrame();

        GenerateLevel(totalShapes);
    }

    private void ResetGameState()
    {
        IsSpawning = false;
        isGameOver = false;

        OnTripleRemoved = null;
    }

    public void RestartGame()
    {
        ResetGameState();

        StartCoroutine(ClearObjectsBeforeRestart());
    }

    private IEnumerator ClearObjectsBeforeRestart()
    {
        yield return null;

        foreach (Shape shape in shapesInField.ToList())
            if (shape != null) Destroy(shape.gameObject);

        foreach (ActionBarAnimal item in actionBarItems.ToList())
            if (item != null) Destroy(item.gameObject);

        shapesInField.Clear();
        actionBarItems.Clear();

        InitializeGame();
    }

    private void ShowWinScreen()
    {
        AudioManager.Instance.PlaySFX("Win");
        if (UIManager.Instance != null) UIManager.Instance.ShowWinScreen();
    }

    private void ShowLoseScreen()
    {
        AudioManager.Instance.PlaySFX("Lose");
        if (UIManager.Instance != null) UIManager.Instance.ShowLoseScreen();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}