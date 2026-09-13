// GameIds.cs
// The only registry of string ids. Scenes, endings, flags, clues, dialogue
// references and sound ids. If a string appears in JSON or in an Inspector
// field it must match a constant here.
public static class GameIds
{
    // Scenes
    public const string SceneMainMenu = "MainMenu";
    public const string SceneBank = "Bank";
    public const string SceneEndingFailure = "Ending_Failure";
    public const string SceneEndingEscape = "Ending_Escape";
    public const string SceneEndingHero = "Ending_Hero";
    public const string SceneEndingCriminal = "Ending_Criminal";
    public const string SceneEndingTrue = "Ending_True";

    // Ending ids
    public const string EndingFailure = "failure";
    public const string EndingEscape = "escape";
    public const string EndingHero = "hero";
    public const string EndingCriminal = "criminal";
    public const string EndingTrue = "true_ending";

    // Ending variants
    public const string VariantNone = "";
    public const string VariantSirens = "sirens";

    // Failure causes
    public const string CauseDialogue = "dialogue";
    public const string CauseFrontDoor = "front_door";
    public const string CauseDeadline = "deadline";
    public const string CauseExpiry = "expiry";
    public const string CauseTaserNoAlarm = "taser_no_alarm";
    public const string CauseUnknown = "unknown";

    // Run flags (cleared every run)
    public const string SkipAhead = "skip_ahead";
    public const string HeistStarted = "heist_started";
    public const string GuardsStoodDown = "guards_stood_down";
    public const string CustomerShot = "customer_shot";
    public const string EscortReady = "escort_ready";
    public const string PlayerJoined = "player_joined";
    public const string HasStaffCard = "has_staff_card";
    public const string HasOfficeKey = "has_office_key";
    public const string HasEvidence = "has_evidence";
    public const string HasTaser = "has_taser";
    public const string HasExitCard = "has_exit_card";
    public const string HasMoneybag = "has_moneybag";
    public const string AlarmTriggered = "alarm_triggered";
    public const string InCorridor = "in_corridor";
    public const string LeaderAtVault = "leader_at_vault";
    public const string SentToSecurity = "sent_to_security";
    public const string InSecurity = "in_security";
    public const string VaultOpen = "vault_open";
    public const string BagOffered = "bag_offered";
    public const string CrewLeaving = "crew_leaving";
    public const string TaserUsed = "taser_used";
    public const string FatalDialogue = "fatal_dialogue";
    public const string FatalDoor = "fatal_door";
    public const string FatalDeadline = "fatal_deadline";
    public const string TimerExpired = "timer_expired";
    public const string Outside = "outside";
    public const string ResolveNow = "resolve_now";
    public const string TellerNudged = "teller_nudged";
    public const string SideExitUsed = "side_exit_used";
    public const string ManagerGone = "manager_gone";
    public const string CriminalMeet = "criminal_meet";

    // The nine flags the ending resolver reads. Used by EndingSystem.SelfTest.
    public static readonly string[] ResolverFlags =
    {
        FatalDialogue, FatalDoor, FatalDeadline, TimerExpired,
        TaserUsed, AlarmTriggered, HasEvidence, Outside, HasMoneybag
    };

    // Clue ids
    public const string ClueManagerClock = "clue_manager_clock";
    public const string ClueGuardBreak = "clue_guard_break";
    public const string ClueLineDown = "clue_line_down";
    public const string ClueManagerDoor = "clue_manager_door";
    public const string ClueThreeGuards = "clue_three_guards";
    public const string ClueAuditMonday = "clue_audit_monday";
    public const string ClueGuardsUnarmed = "clue_guards_unarmed";
    public const string ClueCardOnDesk = "clue_card_on_desk";
    public const string ClueManagerStoodDown = "clue_manager_stood_down";
    public const string ClueFiveInside = "clue_five_inside";
    public const string ClueFriendInside = "clue_friend_inside";
    public const string ClueAlarmButton = "clue_alarm_button";
    public const string ClueNoReturn = "clue_no_return";
    public const string ClueSideExitExpected = "clue_side_exit_expected";
    public const string ClueManagerMissing = "clue_manager_missing";
    public const string ClueInsideMan = "clue_inside_man";
    public const string ClueAlarmLive = "clue_alarm_live";
    public const string ClueLeaderTakesOrders = "clue_leader_takes_orders";
    public const string ClueKeyUnderCarpet = "clue_key_under_carpet";
    public const string ClueNotebook = "clue_notebook";
    public const string ClueFollowPlot = "clue_follow_plot";
    public const string ClueSomeoneOutside = "clue_someone_outside";
    public const string ClueUnknownShooter = "clue_unknown_shooter";
    public const string ClueLeaderNotMastermind = "clue_leader_not_mastermind";
    public const string ClueManagerMastermind = "clue_manager_mastermind";

    // Dialogue references used from code. Format: file or file#node.
    public const string DlgVault = "robber_leader#vault";
    public const string DlgVaultAlarm = "robber_leader#vault_alarm";
    public const string DlgDeadline = "robber_goon_1#shoot_slow";
    public const string DlgFrontDoor = "robber_goon_1#shoot";
    public const string DlgShotUnseen = "bank_manager#shot_unseen";
    public const string DlgOutsideCriminal = "bank_manager#outside_criminal";
    public const string DlgTellerNudge = "player_self#tellers_later";

    // Sound ids. AudioManager maps these to clips when the audio owner adds them.
    public const string SfxGunshot = "gunshot";
    public const string SfxAlarm = "alarm";
    public const string SfxSirens = "sirens";
    public const string SfxDoor = "door";
    public const string SfxDoorClose = "door_close";
    public const string SfxInteraction = "interaction";
    public const string SfxFootstep = "footstep";
    public const string SfxClue = "clue";
    public const string SfxScream = "scream";
    public const string BgmMain = "bgm_main";
    public const string BgmHeist = "bgm_heist";
    public const string SfxEndingFailure = "ending_failure";
    public const string SfxEndingEscape = "ending_escape";
    public const string SfxEndingHero = "ending_hero";
    public const string SfxEndingCriminal = "ending_criminal";
    public const string SfxEndingTrue = "ending_true";
    public const string SfxCardReader = "card_reader";
}
