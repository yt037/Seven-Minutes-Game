// EndingSystem.cs
// Pure ending logic. No scene access, no MonoBehaviour. GameManager calls
// Resolve when a run ends; EndingScreen calls EndingForScene to know which
// ending it is showing. SelfTest proves every terminal flag set maps to an ending.
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class EndingResult
{
    public string ending = "";
    public string variant = "";
    public string cause = "";

    public bool IsNone => string.IsNullOrEmpty(ending);

    public EndingResult() { }

    public EndingResult(string ending, string variant = "", string cause = "")
    {
        this.ending = ending;
        this.variant = variant;
        this.cause = cause;
    }

    public override string ToString() => $"{ending}{(variant == "" ? "" : "/" + variant)}{(cause == "" ? "" : " (" + cause + ")")}";
}

public static class EndingSystem
{
    // Top to bottom, first match wins. See the plan, section 2.
    public static EndingResult Resolve(ICollection<string> run, bool heroDone)
    {
        bool Has(string f) => run.Contains(f);

        if (Has(GameIds.FatalDialogue)) return new EndingResult(GameIds.EndingFailure, "", GameIds.CauseDialogue);
        if (Has(GameIds.FatalDoor)) return new EndingResult(GameIds.EndingFailure, "", GameIds.CauseFrontDoor);
        if (Has(GameIds.FatalDeadline)) return new EndingResult(GameIds.EndingFailure, "", GameIds.CauseDeadline);
        if (Has(GameIds.TimerExpired)) return new EndingResult(GameIds.EndingFailure, "", GameIds.CauseExpiry);

        bool alarm = Has(GameIds.AlarmTriggered);
        string variant = alarm ? GameIds.VariantSirens : GameIds.VariantNone;

        if (Has(GameIds.TaserUsed))
        {
            if (!alarm) return new EndingResult(GameIds.EndingFailure, "", GameIds.CauseTaserNoAlarm);
            if (Has(GameIds.HasEvidence) && heroDone) return new EndingResult(GameIds.EndingTrue);
            return new EndingResult(GameIds.EndingHero);
        }

        if (Has(GameIds.Outside))
        {
            if (Has(GameIds.HasMoneybag)) return new EndingResult(GameIds.EndingCriminal, variant);
            return new EndingResult(GameIds.EndingEscape, variant);
        }

        return new EndingResult();
    }

    public static string SceneFor(string ending)
    {
        switch (ending)
        {
            case GameIds.EndingFailure: return GameIds.SceneEndingFailure;
            case GameIds.EndingEscape: return GameIds.SceneEndingEscape;
            case GameIds.EndingHero: return GameIds.SceneEndingHero;
            case GameIds.EndingCriminal: return GameIds.SceneEndingCriminal;
            case GameIds.EndingTrue: return GameIds.SceneEndingTrue;
            default: return GameIds.SceneEndingFailure;
        }
    }

    public static string EndingForScene(string sceneName)
    {
        switch (sceneName)
        {
            case GameIds.SceneEndingFailure: return GameIds.EndingFailure;
            case GameIds.SceneEndingEscape: return GameIds.EndingEscape;
            case GameIds.SceneEndingHero: return GameIds.EndingHero;
            case GameIds.SceneEndingCriminal: return GameIds.EndingCriminal;
            case GameIds.SceneEndingTrue: return GameIds.EndingTrue;
            default: return "";
        }
    }

    // Enumerates every subset of the resolver's input flags (512) with heroDone
    // both ways. Any subset containing a terminal flag must resolve to an ending.
    public static string SelfTest()
    {
        string[] flags = GameIds.ResolverFlags;
        string[] terminal = { GameIds.FatalDialogue, GameIds.FatalDoor, GameIds.FatalDeadline, GameIds.TimerExpired, GameIds.TaserUsed, GameIds.Outside };

        var sb = new StringBuilder();
        int failures = 0;
        var counts = new Dictionary<string, int>();

        for (int mask = 0; mask < (1 << flags.Length); mask++)
        {
            var set = new HashSet<string>();
            for (int i = 0; i < flags.Length; i++)
                if ((mask & (1 << i)) != 0) set.Add(flags[i]);

            bool hasTerminal = false;
            foreach (string t in terminal) if (set.Contains(t)) hasTerminal = true;

            for (int h = 0; h < 2; h++)
            {
                EndingResult r = Resolve(set, h == 1);

                if (hasTerminal && r.IsNone)
                {
                    failures++;
                    sb.AppendLine("NO ENDING for " + string.Join(",", set) + " heroDone=" + (h == 1));
                }

                if (!hasTerminal && !r.IsNone)
                {
                    failures++;
                    sb.AppendLine("ENDING WITHOUT TERMINAL FLAG for " + string.Join(",", set));
                }

                string key = r.IsNone ? "none" : r.ToString();
                counts[key] = counts.TryGetValue(key, out int c) ? c + 1 : 1;
            }
        }

        sb.Insert(0, $"EndingSystem self test: {failures} failures over {2 << flags.Length} combinations.\n");
        foreach (var kv in counts) sb.AppendLine($"  {kv.Key}: {kv.Value}");
        return sb.ToString();
    }
}
