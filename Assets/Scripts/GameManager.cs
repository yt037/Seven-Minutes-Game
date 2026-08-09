using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CollectKeycard()
    {
        hasKeycard = true;
    }

    public void WarnGuard()
    {
        guardWarned = true;
    }
}
