using System.Collections.Generic;
using UnityEngine;

namespace CoinQuest
{
    public sealed class GameManager : MonoBehaviour
    {
        public enum FlowState { MainMenu, Playing, Paused, GameOver, Victory }
        public static GameManager Instance { get; private set; }
        public FlowState State { get; private set; }
        public bool IsPlaying { get { return State == FlowState.Playing; } }
        public bool HasAllRunes { get { return runeIds.Count >= 3; } }
        public int Coins { get; private set; }
        public int Runes { get { return runeIds.Count; } }
        public int Health { get; private set; } = 5;
        public bool FinalBossDefeated { get; private set; }
        public EnemyController FinalBoss { get; private set; }
        public Transform Player { get; private set; }
        public PlayerController Controller { get; private set; }
        public PlayerHealth PlayerVitals { get; private set; }
        public IslandLevelBuilder Builder { get; private set; }
        public UIManager UI { get; private set; }
        public AudioManager Audio { get; private set; }
        private readonly HashSet<int> runeIds = new HashSet<int>();
        private Vector3 checkpoint = new Vector3(0, 0.1f, 0);
        private TempleDoor door;
        private bool hasStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; State = FlowState.MainMenu; Health = 5;
        }
        private void OnDestroy() { if (Instance == this) Instance = null; Time.timeScale = 1f; }
        public void Configure(IslandLevelBuilder level, UIManager ui, AudioManager audio) { Builder = level; UI = ui; Audio = audio; }
        public void SetWorldReferences(Transform player, PlayerController controller, PlayerHealth vitals, TempleDoor templeDoor)
        {
            Player = player; Controller = controller; PlayerVitals = vitals; door = templeDoor;
            checkpoint = player != null ? player.position : Vector3.zero;
        }
        public void RegisterFinalBoss(EnemyController boss) { FinalBoss = boss; }
        public void OnFinalBossDefeated(EnemyController boss)
        {
            if (boss != null && boss == FinalBoss) { FinalBossDefeated = true; UI.ShowNotification("ANCIENT SKY GUARDIAN DEFEATED! Reach the treasure chest."); }
        }
        public void PrepareMenu()
        {
            SetState(FlowState.MainMenu);
            if (Controller != null) Controller.enabled = false;
            if (PlayerVitals != null) PlayerVitals.enabled = false;
            UI.ShowMenu(); UI.UpdateHud(this); LockCursor(false);
        }
        public void StartGame()
        {
            if (State != FlowState.MainMenu) return;
            if (hasStarted) Builder.RebuildWorld(this);
            hasStarted = true;
            SetState(FlowState.Playing);
            if (Controller != null) Controller.enabled = true;
            if (PlayerVitals != null) PlayerVitals.enabled = true;
            UI.ShowHud(); UI.UpdateHud(this); UI.ShowNotification("Keep running! Steer, jump obstacles, and find the golden wings.");
            LockCursor(true); Audio.Play("start");
        }
        public void Pause() { if (!IsPlaying) return; SetState(FlowState.Paused); UI.ShowPause(); LockCursor(false); }
        public void Resume() { if (State != FlowState.Paused) return; SetState(FlowState.Playing); UI.ShowHud(); LockCursor(true); }
        public void Restart()
        {
            ResetProgress(); Builder.RebuildWorld(this); hasStarted = false; State = FlowState.MainMenu; StartGame();
        }
        public void ReturnToMenu()
        {
            ResetProgress(); Builder.RebuildWorld(this); hasStarted = false; PrepareMenu();
        }
        private void ResetProgress()
        {
            Time.timeScale = 1f; Coins = 0; Health = 5; FinalBossDefeated = false; FinalBoss = null; runeIds.Clear(); checkpoint = new Vector3(0, 0.1f, 0);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { if (IsPlaying) Pause(); else if (State == FlowState.Paused) Resume(); }
            if (IsPlaying && Player != null && Player.position.y < -5f) RespawnAtCheckpoint();
        }
        public void CollectCoin()
        {
            if (!IsPlaying) return; Coins++; UI.UpdateHud(this); UI.ShowNotification("Coin collected!"); Audio.Play("coin");
        }
        public bool HasRune(int id) { return runeIds.Contains(id); }
        public void CollectRune(int id, string label)
        {
            if (!IsPlaying || !runeIds.Add(id)) return;
            UI.UpdateHud(this); UI.ShowNotification(label + " rune discovered!  " + Runes + "/3"); Audio.Play("rune");
            if (HasAllRunes && door != null) { door.Unlock(); UI.ShowNotification("The three runes resonate. The temple is unlocked!"); }
        }
        public void DamagePlayer(int amount, string message)
        {
            if (!IsPlaying || PlayerVitals == null || !PlayerVitals.TryTakeDamage(amount)) return;
            Health = PlayerVitals.CurrentHealth; UI.UpdateHud(this); UI.ShowNotification(message); Audio.Play("damage");
            if (Health <= 0) { SetState(FlowState.GameOver); UI.ShowGameOver(Coins); LockCursor(false); }
        }
        public void ActivateCheckpoint(Vector3 position) { checkpoint = position; UI.ShowNotification("Checkpoint activated!"); Audio.Play("rune"); }
        public void RespawnAtCheckpoint()
        {
            if (Controller != null) Controller.Teleport(checkpoint + Vector3.up * 0.08f);
            UI.ShowNotification("Back at your last checkpoint.");
        }
        public void Interact(IWorldInteractable target) { if (IsPlaying && target != null) target.Interact(this); }
        public void TryOpenDoor(TempleDoor target)
        {
            if (!HasAllRunes) { UI.ShowNotification("The temple is still locked. Find all three runes."); return; }
            target.Open();
        }
        public void TryClaimTreasure(TreasureChest chest)
        {
            if (!FinalBossDefeated) { UI.ShowNotification("Defeat the Ancient Sky Guardian first!"); return; }
            if (!HasAllRunes || door == null || !door.IsOpen) { UI.ShowNotification("The relic is sealed away. Unlock the temple first."); return; }
            chest.Claim();
        }
        public void CollectHealing(int amount)
        {
            if (!IsPlaying || PlayerVitals == null) return;
            int previous = PlayerVitals.CurrentHealth;
            PlayerVitals.Restore(amount);
            Health = PlayerVitals.CurrentHealth;
            if (Health > previous) { UI.UpdateHud(this); UI.ShowNotification("Healing fruit restored your health."); Audio.Play("rune"); }
        }
        public void TryOpenChest(TreasureChest chest) { TryClaimTreasure(chest); }
        public void Notify(string message) { if (UI != null) UI.ShowNotification(message); }
        public void Win() { if (!IsPlaying) return; SetState(FlowState.Victory); UI.ShowVictory(Coins, Runes); Audio.Play("victory"); LockCursor(false); }
        public void LockCursor(bool value)
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !value;
            if (Builder != null && Builder.GameCamera != null) { var rig = Builder.GameCamera.GetComponent<ThirdPersonCamera>(); if (rig != null) rig.SetInputEnabled(value); }
        }
        private void SetState(FlowState value) { State = value; Time.timeScale = value == FlowState.Paused || value == FlowState.GameOver || value == FlowState.Victory ? 0 : 1; }
    }
}

