// GameManager.cs
// Session state, run flags, run lifecycle, deaths and ending resolution.
//
// Singleton with state handoff. GameManager lives in Bank.unity and survives
// scene loads. When Bank is loaded again a fresh GameManager exists in the new
// scene. Instead of destroying the newcomer (which would break every UnityEvent
// in the scene pointing at it) the newcomer adopts the session from the old
// instance and the old one is destroyed. Scene wiring always points at the
// live instance. ClueLog and AudioManager sit on the same object and follow it.
//
// Knowledge is session only: it lives in State and is gone when the process ends.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Everything that survives a run. Plain C# object handed from instance to instance.
    public class Session
    {
        public List<string> endingsUnlocked = new List<string>();
        public int runsCompleted;
        public List<string> clues = new List<string>();
        public HashSet<string> persistentFlags = new HashSet<string>();
        public HashSet<string> itemsSeen = new HashSet<string>();
        public EndingResult lastResult = new EndingResult();
        public bool failureClueGiven;

        public bool HeroDone => endingsUnlocked.Contains(GameIds.EndingHero);
        public bool IsFirstRun => runsCompleted == 0;
    }

    [Tooltip("F5 to F9 force each ending. Editor and development builds only.")]
    [SerializeField] private bool debugHotkeys = true;

    [Tooltip("Seconds after stepping outside before the ending fires, so the side door has shut.")]
    [SerializeField] private float outsideDelay = 1f;

    public Session State { get; private set; } = new Session();
    public bool RunOver { get; private set; }
    public Timer Timer => timer;
    public ClueLog Clues => clueLog;
    public ItemLog Items => itemLog;

    public event Action<string> OnFlagSet;

    private readonly HashSet<string> run = new HashSet<string>();
    private Timer timer;
    private Player player;
    private ClueLog clueLog;
    private ItemLog itemLog;
    private bool runActive;
    private Scene runScene;
    private bool flashed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            State = Instance.State;
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        clueLog = GetComponent<ClueLog>();
        itemLog = GetComponent<ItemLog>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == GameIds.SceneBank) BeginRun();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameIds.SceneBank) BeginRun();
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!debugHotkeys || !runActive || RunOver) return;

        if (Keys.FunctionPressed(5)) { SetFlag(GameIds.FatalDialogue); EndRun(); }
        if (Keys.FunctionPressed(6)) { SetFlag(GameIds.Outside); EndRun(); }
        if (Keys.FunctionPressed(7)) { SetFlag(GameIds.AlarmTriggered); SetFlag(GameIds.TaserUsed); EndRun(); }
        if (Keys.FunctionPressed(8)) { SetFlag(GameIds.Outside); SetFlag(GameIds.HasMoneybag); EndRun(); }
        if (Keys.FunctionPressed(9))
        {
            if (!State.endingsUnlocked.Contains(GameIds.EndingHero)) State.endingsUnlocked.Add(GameIds.EndingHero);
            SetFlag(GameIds.AlarmTriggered); SetFlag(GameIds.TaserUsed); SetFlag(GameIds.HasEvidence); EndRun();
        }
#endif
    }

    // ---------------------------------------------------------------- run lifecycle

    private void BeginRun()
    {
        // Both Start and OnSceneLoaded can reach here for the same scene load,
        // so compare the Scene itself rather than a handle. Scene defines ==.
        Scene scene = SceneManager.GetActiveScene();
        if (runActive && runScene == scene) return;

        runActive = true;
        runScene = scene;
        RunOver = false;
        flashed = false;
        run.Clear();
        Time.timeScale = 1f;

        // FindAnyObjectByType: there is exactly one of each in the Bank scene,
        // and it does not pay the cost of ordering results by instance id.
        timer = FindAnyObjectByType<Timer>();
        player = FindAnyObjectByType<Player>();
    }

    public void AbortRun()
    {
        runActive = false;
        RunOver = true;
        Time.timeScale = 1f;
        if (timer != null) timer.Stop();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        runActive = false;
        SceneManager.LoadScene(GameIds.SceneBank);
    }

    public void GoToMenu()
    {
        AbortRun();
        SceneManager.LoadScene(GameIds.SceneMainMenu);
    }

    // ---------------------------------------------------------------- flags

    public bool HasFlag(string flag) => !string.IsNullOrEmpty(flag) && run.Contains(flag);

    public void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        if (!run.Add(flag)) return;
        OnFlagSet?.Invoke(flag);
    }

    public void ClearFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        run.Remove(flag);
    }

    public bool KnowsPersistent(string flag) => !string.IsNullOrEmpty(flag) && State.persistentFlags.Contains(flag);

    public void LearnPersistent(string flag)
    {
        if (!string.IsNullOrEmpty(flag)) State.persistentFlags.Add(flag);
    }

    public void AddClue(string clueId)
    {
        if (clueLog != null) clueLog.Add(clueId);
    }

    // ---------------------------------------------------------------- world entry points (called from Zones, Timer, readers)

    public void EnterBank()
    {
        if (timer != null) timer.Begin();
    }

    public void StartHeist()
    {
        SetFlag(GameIds.HeistStarted);
    }

    public void KillAtFrontDoor()
    {
        if (RunOver) return;
        SetFlag(GameIds.FatalDoor);
        Die(GameIds.DlgFrontDoor);
    }

    // Fired by Timer.onEscortExpired, 30 s after the staff door was opened.
    public void EscortDeadline()
    {
        if (RunOver || HasFlag(GameIds.InCorridor)) return;
        SetFlag(GameIds.FatalDeadline);
        Die(GameIds.DlgDeadline);
    }

    public void OnTimerExpired()
    {
        if (RunOver) return;
        SetFlag(GameIds.TimerExpired);
        EndRun();
    }

    public void SteppedOutside()
    {
        if (RunOver || HasFlag(GameIds.Outside)) return;
        SetFlag(GameIds.Outside);
        StartCoroutine(OutsideRoutine());
    }

    private IEnumerator OutsideRoutine()
    {
        if (player != null) player.SetFrozen(true);
        yield return new WaitForSeconds(outsideDelay);
        if (RunOver) yield break;

        if (HasFlag(GameIds.HasMoneybag))
        {
            // Mr Wilco and Vander are waiting. FlagListeners move them into place.
            SetFlag(GameIds.CriminalMeet);
            SetFlag(GameIds.ResolveNow);
            if (DialogueRunner.Instance != null) DialogueRunner.Instance.PlayForced(GameIds.DlgOutsideCriminal);
            else EndRun();
        }
        else
        {
            Die(GameIds.DlgShotUnseen);
        }
    }

    // Red flash, then the given line, then the run ends. Pass an empty
    // reference to end straight after the flash.
    public void Die(string dialogueRef)
    {
        if (RunOver) return;
        SetFlag(GameIds.ResolveNow);
        StartCoroutine(DieRoutine(dialogueRef));
    }

    private IEnumerator DieRoutine(string dialogueRef)
    {
        if (player != null) player.SetFrozen(true);
        if (timer != null) timer.Stop();

        DialogueRunner runner = DialogueRunner.Instance;
        if (runner != null) runner.Interrupt();

        if (AudioManager.Instance != null) AudioManager.Instance.Play(GameIds.SfxGunshot);

        bool done = false;
        if (runner != null && runner.ui != null)
        {
            flashed = true;
            runner.ui.Flash(() => done = true);
            while (!done) yield return null;
        }

        if (runner != null && !string.IsNullOrEmpty(dialogueRef) && runner.PlayForced(dialogueRef))
            yield break;                // OnDialogueFinished ends the run

        EndRun();
    }

    // Wired to DialogueRunner.onFinished.
    public void OnDialogueFinished()
    {
        if (!RunOver && HasFlag(GameIds.ResolveNow)) EndRun();
    }

    // ---------------------------------------------------------------- resolution

    public void EndRun()
    {
        if (RunOver) return;

        EndingResult result = EndingSystem.Resolve(run, State.HeroDone);
        if (result.IsNone)
        {
            Debug.LogError("EndRun called with no terminal flag set. Flags: " + string.Join(",", run));
            result = new EndingResult(GameIds.EndingFailure, "", GameIds.CauseUnknown);
        }

        Finish(result);
    }

    private void Finish(EndingResult result)
    {
        RunOver = true;
        runActive = false;
        if (timer != null) timer.Stop();
        if (player != null) player.SetFrozen(true);

        State.lastResult = result;
        State.runsCompleted++;
        if (!State.endingsUnlocked.Contains(result.ending)) State.endingsUnlocked.Add(result.ending);

        switch (result.ending)
        {
            case GameIds.EndingFailure:
                if (!State.failureClueGiven) { State.failureClueGiven = true; AddClue(GameIds.ClueFollowPlot); }
                break;
            case GameIds.EndingEscape:
                AddClue(GameIds.ClueSomeoneOutside);
                AddClue(GameIds.ClueUnknownShooter);
                break;
            case GameIds.EndingHero:
                AddClue(GameIds.ClueLeaderNotMastermind);
                break;
            case GameIds.EndingCriminal:
                AddClue(GameIds.ClueManagerMastermind);
                break;
        }

        StartCoroutine(LoadEndingRoutine(result));
    }

    private IEnumerator LoadEndingRoutine(EndingResult result)
    {
        DialogueRunner runner = DialogueRunner.Instance;
        if (runner != null) runner.Interrupt();

        if (result.ending == GameIds.EndingFailure && !flashed && runner != null && runner.ui != null)
        {
            bool done = false;
            runner.ui.Flash(() => done = true);
            while (!done) yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(EndingSystem.SceneFor(result.ending));
    }

    [ContextMenu("Run ending self test")]
    private void RunSelfTest()
    {
        Debug.Log(EndingSystem.SelfTest());
    }
}
