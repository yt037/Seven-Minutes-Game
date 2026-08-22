using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Timer timer;

    [SerializeField] private bool hasKeycard;
    [SerializeField] private bool guardWarned;
    public bool HasKeycard => hasKeycard;
    public bool GuardWarned => guardWarned;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

<<<<<<< Updated upstream
// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
=======
    private void Start()
    {
        timer = gameObject.GetComponent<Timer>();
>>>>>>> Stashed changes
    }

    public void CollectKeycard()
    {
        hasKeycard = true;
    }

    public void WarnGuard()
    {
        guardWarned = true;
    }
<<<<<<< Updated upstream
=======

    public void StartRobbery()
    {
        robberyStarted = true;
    }

    public void StartVaultAttempt()
    {
        vaultAttemptStarted = true;
    }

    public void StartPoliceResponse()
    {
        policeResponseStarted = true;
    }

    public void EndLoop()
    {
        ResetLoopState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetLoopState()
    {
        hasKeycard = false;
        guardWarned = false;
        robberyStarted = false;
        vaultAttemptStarted = false;
        policeResponseStarted = false;
    }
>>>>>>> Stashed changes
}
