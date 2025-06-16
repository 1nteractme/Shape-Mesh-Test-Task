using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private HorizontalLayoutGroup animalSpritesLayout;

    [Header("Buttons")]
    [SerializeField] private Button reshuffleButton;
    [SerializeField] private Button restartWinButton;
    [SerializeField] private Button restartLoseButton;

    [Header("Screens")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject gameScreen;

    private bool isReshuffling = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        reshuffleButton.onClick.AddListener(OnReshuffleClicked);
        restartWinButton.onClick.AddListener(RestartGame);
        restartLoseButton.onClick.AddListener(RestartGame);

        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        gameScreen.SetActive(true);

        UpdateReshuffle(false);
        StartCoroutine(FixLayoutsAfterLoad());
    }

    public void UpdateReshuffle(bool interactable)
    {
        reshuffleButton.interactable = interactable;

        Image buttonImage = reshuffleButton.GetComponentInChildren<Image>();
        if (buttonImage != null)
            buttonImage.color = interactable ? Color.white : new Color(1f, 1f, 1f, 0.7f);
        else
            Debug.LogWarning("Image component not found on reshuffle button or its children", reshuffleButton);

    }

    private void OnReshuffleClicked()
    {
        if (isReshuffling || GameManager.Instance == null || GameManager.Instance.IsSpawning || !reshuffleButton.interactable)
            return;

        StartCoroutine(HandleReshuffle());
    }

    private IEnumerator HandleReshuffle()
    {
        isReshuffling = true;
        UpdateReshuffle(false);

        GameManager.Instance.ReshuffleField();

        yield return new WaitWhile(() => GameManager.Instance.IsSpawning);

        if (!winScreen.activeSelf && !loseScreen.activeSelf)
            UpdateReshuffle(GameManager.Instance.CanReshuffle());


        isReshuffling = false;
    }

    public void ShowWinScreen()
    {
        winScreen.SetActive(true);
        loseScreen.SetActive(false);
        gameScreen.SetActive(false);
        UpdateReshuffle(false);
    }

    public void ShowLoseScreen()
    {
        loseScreen.SetActive(true);
        winScreen.SetActive(false);
        gameScreen.SetActive(false);
        UpdateReshuffle(false);
    }
    public void RestartGame() => StartCoroutine(RestartRoutine());

    private IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();

            winScreen.SetActive(false);
            loseScreen.SetActive(false);
            gameScreen.SetActive(true);
        }
        else
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }

    private IEnumerator FixLayoutsAfterLoad()
    {
        yield return new WaitForSeconds(0.1f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(animalSpritesLayout.GetComponent<RectTransform>());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        reshuffleButton.onClick.RemoveListener(OnReshuffleClicked);
        restartWinButton.onClick.RemoveListener(RestartGame);
        restartLoseButton.onClick.RemoveListener(RestartGame);
    }
}