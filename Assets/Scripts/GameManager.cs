using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool hasKeycard;
    [SerializeField] private bool guardWarned;
    [SerializeField] private bool robberyStarted;
    [SerializeField] private bool vaultAttemptStarted;
    [SerializeField] private bool policeResponseStarted;

    public bool HasKeycard => hasKeycard;
    public bool GuardWarned => guardWarned;
    public bool RobberyStarted => robberyStarted;
    public bool VaultAttemptStarted => vaultAttemptStarted;
    public bool PoliceResponseStarted => policeResponseStarted;

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

    public void CollectKeycard()
    {
        hasKeycard = true;
    }

    public void WarnGuard()
    {
        guardWarned = true;
    }

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
}
