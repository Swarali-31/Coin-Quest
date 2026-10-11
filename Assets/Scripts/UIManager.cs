using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinQuest
{
    public sealed class UIManager : MonoBehaviour
    {
        private GameManager manager;
        private GameObject menu, howTo, settings, hud, pause, gameOver, victory;
        private Text healthText, coinText, runeText, distanceText, promptText, notificationText, echoText, selectedExplorerText;
        private Text flightText, bossText, bossInstructions;
        private Image bossFill;
        private GameObject bossHealthPanel;
        private Coroutine notificationRoutine;
        private bool muted;
        private float nextEchoRefresh;

        private void Update()
        {
            if (Time.unscaledTime >= nextEchoRefresh)
            {
                nextEchoRefresh = Time.unscaledTime + 0.15f;
                if (hud != null && hud.activeSelf) { UpdateEchoStatus(); UpdateFlightAndBoss(); UpdateDistance(); }
            }
        }

        public void Build(GameManager game)
        {
            manager = game;
            if (hud != null) return;
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
            }
            var canvasObject = new GameObject("Coin Quest Canvas");
            var canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = 0.5f;

            menu = MakePanel(canvas.transform, "Title Menu", new Vector2(0.5f, 0.52f), new Vector2(850, 790), new Color(0.035f, 0.09f, 0.12f, 0.91f));
            Label(menu.transform, "COIN QUEST", 62, new Color(1f, 0.76f, 0.2f), new Vector2(0.5f, 0.92f), new Vector2(780, 78));
            Label(menu.transform, "THE LOST TEMPLE", 30, new Color(0.49f, 0.82f, 1f), new Vector2(0.5f, 0.84f), new Vector2(720, 48));
            Label(menu.transform, "Run the ancient trail. Reach the temple and defeat its guardian.", 20, Color.white, new Vector2(0.5f, 0.75f), new Vector2(760, 52));
            Label(menu.transform, "Collect coins, find three runes, and claim the treasure.", 17, new Color(0.83f, 0.9f, 0.88f), new Vector2(0.5f, 0.66f), new Vector2(780, 50));
            var characterPanel = MakePanel(menu.transform, "Choose Your Explorer", new Vector2(0.5f, 0.49f), new Vector2(720, 132), new Color(0.08f, 0.17f, 0.2f, 0.95f));
            Label(characterPanel.transform, "CHOOSE YOUR EXPLORER", 20, new Color(0.55f, 0.85f, 0.98f), new Vector2(0.5f, 0.82f), new Vector2(680, 34));
            Button(characterPanel.transform, "MALE EXPLORER", new Vector2(0.29f, 0.48f), new Vector2(280, 48), () => SelectExplorer(false), false);
            Button(characterPanel.transform, "FEMALE EXPLORER", new Vector2(0.71f, 0.48f), new Vector2(280, 48), () => SelectExplorer(true), false);
            selectedExplorerText = Label(characterPanel.transform, "SELECTED: MALE EXPLORER", 15, new Color(1f, 0.82f, 0.35f), new Vector2(0.5f, 0.13f), new Vector2(600, 25));
            Button(menu.transform, "PLAY", new Vector2(0.5f, 0.32f), new Vector2(410, 58), manager.StartGame, true);
            Button(menu.transform, "HOW TO PLAY", new Vector2(0.5f, 0.235f), new Vector2(410, 52), ShowHowTo, false);
            Button(menu.transform, "SETTINGS", new Vector2(0.5f, 0.155f), new Vector2(410, 50), ShowSettings, false);
            Button(menu.transform, "QUIT", new Vector2(0.5f, 0.08f), new Vector2(410, 46), Quit, false);

            howTo = MakePanel(canvas.transform, "How To Play", new Vector2(0.5f, 0.5f), new Vector2(900, 700), new Color(0.035f, 0.09f, 0.12f, 0.96f));
            Label(howTo.transform, "HOW TO PLAY", 40, new Color(1f, 0.76f, 0.2f), new Vector2(0.5f, 0.87f), new Vector2(820, 60));
            Label(howTo.transform, "Auto-run forward through a long adventure.\nA/D or Left/Right - Steer     W/Up - Boost\nS/Down - Slow     Mouse - Look/Aim\nSpace - Jump; wings lift automatically for 20s     Ctrl - Land early\nShift - Dodge     Left Click - Attack Guardian\nQ - Stun Guardian     E - Interact     Esc - Pause\n\nCollect three runes, claim the golden wings, and defeat the final guardian.", 22, Color.white, new Vector2(0.5f, 0.54f), new Vector2(790, 360));
            Button(howTo.transform, "BACK", new Vector2(0.5f, 0.13f), new Vector2(300, 58), ShowMenu, false);

            settings = MakePanel(canvas.transform, "Settings", new Vector2(0.5f, 0.5f), new Vector2(680, 460), new Color(0.035f, 0.09f, 0.12f, 0.96f));
            Label(settings.transform, "SETTINGS", 42, new Color(1f, 0.76f, 0.2f), new Vector2(0.5f, 0.78f), new Vector2(600, 70));
            Button(settings.transform, "SOUND: ON", new Vector2(0.5f, 0.5f), new Vector2(380, 64), ToggleSound, false);
            Label(settings.transform, "Mouse look sensitivity can be adjusted in the camera Inspector.", 18, Color.white, new Vector2(0.5f, 0.33f), new Vector2(580, 56));
            Button(settings.transform, "BACK", new Vector2(0.5f, 0.15f), new Vector2(280, 56), ShowMenu, false);

            hud = new GameObject("Gameplay HUD", typeof(RectTransform)); hud.transform.SetParent(canvas.transform, false);
            var hudRect = hud.GetComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero; hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero; hudRect.offsetMax = Vector2.zero;
            hudRect.pivot = new Vector2(0.5f, 0.5f);
            var trackerPanel = MakePanel(hud.transform, "Adventure Trackers", new Vector2(0.5f, 0.92f), new Vector2(760, 142), new Color(0.025f, 0.065f, 0.08f, 0.94f));
            coinText = Label(trackerPanel.transform, "COINS: 0", 22, new Color(1f, 0.82f, 0.28f), new Vector2(0.26f, 0.72f), new Vector2(340, 42), TextAnchor.MiddleCenter);
            runeText = Label(trackerPanel.transform, "RUNES: 0/3", 20, new Color(0.62f, 0.9f, 1f), new Vector2(0.74f, 0.72f), new Vector2(340, 42), TextAnchor.MiddleCenter);
            healthText = Label(trackerPanel.transform, "HEALTH: 5 / 5", 21, new Color(1f, 0.78f, 0.72f), new Vector2(0.26f, 0.28f), new Vector2(340, 42), TextAnchor.MiddleCenter);
            distanceText = Label(trackerPanel.transform, "DISTANCE: 0 m / 300 m", 20, new Color(0.9f, 0.96f, 1f), new Vector2(0.74f, 0.28f), new Vector2(340, 42), TextAnchor.MiddleCenter);
            Button(hud.transform, "PAUSE", new Vector2(0.94f, 0.91f), new Vector2(112, 42), manager.Pause, true);

            var controls = MakePanel(hud.transform, "Controls Panel", new Vector2(0.27f, 0.16f), new Vector2(360, 250), new Color(0.025f, 0.065f, 0.08f, 0.82f));
            Label(controls.transform, "AUTO-RUN FORWARD\nA/D OR LEFT/RIGHT - STEER\nW/UP - BOOST   S/DOWN - SLOW\nSPACE - JUMP; WINGS AUTO-LIFT 20s\nCTRL - LAND EARLY   SHIFT - DODGE\nLEFT CLICK - ATTACK GUARDIAN\nQ - STUN GUARDIAN   E - INTERACT\nESC - PAUSE", 14, Color.white, new Vector2(0.06f, 0.5f), new Vector2(328, 218), TextAnchor.MiddleLeft);
            echoText = Label(hud.transform, "ECHO READY [Q]", 17, new Color(0.45f, 0.85f, 1f), new Vector2(0.79f, 0.16f), new Vector2(300, 42), TextAnchor.MiddleRight);
            promptText = Label(hud.transform, "", 20, Color.white, new Vector2(0.5f, 0.30f), new Vector2(740, 42));
            notificationText = Label(hud.transform, "", 20, new Color(1f, 0.91f, 0.55f), new Vector2(0.5f, 0.40f), new Vector2(780, 40));
            flightText = Label(hud.transform, "", 18, new Color(1f,0.82f,0.25f), new Vector2(0.5f,0.75f), new Vector2(300,34), TextAnchor.MiddleRight);
            bossHealthPanel = MakePanel(hud.transform, "Boss Health", new Vector2(0.5f,0.70f), new Vector2(620,94), new Color(0.04f,0.05f,0.08f,0.9f));
            bossText = Label(bossHealthPanel.transform, "ANCIENT SKY GUARDIAN", 16, new Color(0.8f,0.9f,1f), new Vector2(0.5f,0.82f), new Vector2(500,24));
            bossInstructions = Label(bossHealthPanel.transform, "W/S MOVE   |   A/D DODGE   |   LEFT CLICK ATTACK   |   Q STUN GUARDIAN", 14, Color.white, new Vector2(0.5f,0.46f), new Vector2(500,22));
            var bossBar = MakePanel(bossHealthPanel.transform, "Boss Health Bar", new Vector2(0.5f,0.13f), new Vector2(470,14), new Color(0.25f,0.12f,0.18f,1f));
            var fillObject = new GameObject("Boss Health Fill"); fillObject.transform.SetParent(bossBar.transform,false);
            var fillRect = fillObject.AddComponent<RectTransform>(); fillRect.anchorMin=Vector2.zero; fillRect.anchorMax=Vector2.one; fillRect.offsetMin=Vector2.zero; fillRect.offsetMax=Vector2.zero;
            bossFill = fillObject.AddComponent<Image>(); bossFill.color = new Color(0.55f,0.2f,0.9f);
            bossHealthPanel.SetActive(false);

            pause = MakePanel(canvas.transform, "Paused", new Vector2(0.5f, 0.5f), new Vector2(620, 540), new Color(0.02f, 0.05f, 0.08f, 0.96f));
            Label(pause.transform, "PAUSED", 46, new Color(1f, 0.76f, 0.2f), new Vector2(0.5f, 0.82f), new Vector2(540, 70));
            Button(pause.transform, "RESUME", new Vector2(0.5f, 0.61f), new Vector2(380, 62), manager.Resume, true);
            Button(pause.transform, "RESTART", new Vector2(0.5f, 0.45f), new Vector2(380, 62), manager.Restart, false);
            Button(pause.transform, "MAIN MENU", new Vector2(0.5f, 0.29f), new Vector2(380, 62), manager.ReturnToMenu, false);

            gameOver = MakePanel(canvas.transform, "Game Over", new Vector2(0.5f, 0.5f), new Vector2(680, 560), new Color(0.12f, 0.035f, 0.04f, 0.97f));
            Label(gameOver.transform, "GAME OVER", 48, new Color(1f, 0.35f, 0.28f), new Vector2(0.5f, 0.8f), new Vector2(600, 70));
            Label(gameOver.transform, "Your adventure is not over yet. Try again!", 22, Color.white, new Vector2(0.5f, 0.66f), new Vector2(600, 55));
            Button(gameOver.transform, "RETRY", new Vector2(0.5f, 0.46f), new Vector2(380, 62), manager.Restart, true);
            Button(gameOver.transform, "MAIN MENU", new Vector2(0.5f, 0.29f), new Vector2(380, 62), manager.ReturnToMenu, false);

            victory = MakePanel(canvas.transform, "Victory", new Vector2(0.5f, 0.5f), new Vector2(780, 620), new Color(0.1f, 0.075f, 0.02f, 0.97f));
            Label(victory.transform, "VICTORY!", 52, new Color(1f, 0.78f, 0.17f), new Vector2(0.5f, 0.82f), new Vector2(700, 75));
            Label(victory.transform, "The Lost Temple has revealed its secrets.\nYou have claimed the ancient relic!", 25, Color.white, new Vector2(0.5f, 0.65f), new Vector2(700, 100));
            Label(victory.transform, "", 26, new Color(1f, 0.86f, 0.38f), new Vector2(0.5f, 0.48f), new Vector2(520, 52)).name = "Final Stats";
            Button(victory.transform, "PLAY AGAIN", new Vector2(0.5f, 0.31f), new Vector2(390, 62), manager.Restart, true);
            Button(victory.transform, "MAIN MENU", new Vector2(0.5f, 0.16f), new Vector2(390, 58), manager.ReturnToMenu, false);

            ShowMenu();
        }

        private void SelectExplorer(bool female)
        {
            manager.Builder.SelectExplorer(female);
            if (selectedExplorerText != null) selectedExplorerText.text = female ? "SELECTED: FEMALE EXPLORER" : "SELECTED: MALE EXPLORER";
        }
        public void ShowMenu() { Set(menu, true); Set(howTo, false); Set(settings, false); Set(hud, false); Set(pause, false); Set(gameOver, false); Set(victory, false); if (manager != null && manager.Builder != null) SelectExplorer(manager.Builder.IsFemaleExplorerSelected); }
        public void ShowHowTo() { Set(menu, false); Set(howTo, true); Set(settings, false); }
        public void ShowSettings() { Set(menu, false); Set(settings, true); Set(howTo, false); }
        public void ShowHud() { Set(menu, false); Set(howTo, false); Set(settings, false); Set(hud, true); Set(pause, false); Set(gameOver, false); Set(victory, false); }
        public void ShowPause() { Set(hud, false); Set(pause, true); }
        public void ShowGameOver(int coins) { ShowOverlay(gameOver); }
        public void ShowVictory(int coins, int runes)
        {
            var stat = victory.transform.Find("Final Stats");
            if (stat != null) stat.GetComponent<Text>().text = "COINS  " + coins + "        RUNES  " + runes + " / 3";
            ShowOverlay(victory);
        }
        private void ShowOverlay(GameObject overlay) { Set(hud, false); Set(pause, false); Set(overlay, true); }

        public void UpdateHud(GameManager game)
        {
            if (healthText != null) healthText.text = "HEALTH  " + game.Health + " / 5";
            if (coinText != null) coinText.text = "COINS: " + game.Coins;
            if (runeText != null) runeText.text = "RUNES: " + game.Runes + "/3";
            if (distanceText != null && game.Player != null) distanceText.text = "DISTANCE: " + Mathf.Clamp(Mathf.FloorToInt(game.Player.position.z), 0, 300) + " m / 300 m";
        }
        public void UpdateEchoStatus()
        {
            if (echoText == null || GameManager.Instance == null || GameManager.Instance.Player == null) return;
            var pulse = GameManager.Instance.Player.GetComponent<EchoPulse>();
            if (pulse == null) return;
            echoText.text = pulse.IsReady ? "ECHO READY  [Q]" : "ECHO COOLDOWN  " + pulse.CooldownRemaining.ToString("0.0") + "s";
        }
        private void UpdateDistance()
        {
            if (distanceText == null || GameManager.Instance == null || GameManager.Instance.Player == null) return;
            int meters = Mathf.Clamp(Mathf.FloorToInt(GameManager.Instance.Player.position.z), 0, 300);
            distanceText.text = "DISTANCE  " + meters.ToString("N0") + " m  /  300 m";
        }

        private void UpdateFlightAndBoss()
        {
            var game = GameManager.Instance;
            if (game == null || game.Player == null) return;
            var controller = game.Player.GetComponent<PlayerController>();
            flightText.text = controller != null && controller.IsFlying ? "GOLDEN WINGS  " + controller.FlightRemaining.ToString("0.0") + "s" : string.Empty;
            var boss = game.FinalBoss;
            bool showBoss = game.IsPlaying && boss != null && boss.gameObject.activeInHierarchy && !game.FinalBossDefeated;
            bossHealthPanel.SetActive(showBoss);
            if (showBoss)
            {
                UpdateBossHealth(boss);
                bossFill.rectTransform.anchorMax = new Vector2(boss.HealthFraction,1f);
            }
        }
        public void UpdateBossHealth(EnemyController boss)
        {
            if (boss == null || bossText == null || bossFill == null) return;
            bossText.text = "GUARDIAN HEALTH  " + boss.CurrentHealth + " / " + boss.MaxHealth + "     HITS LEFT: " + boss.CurrentHealth;
            bossFill.rectTransform.anchorMax = new Vector2(boss.HealthFraction, 1f);
        }        public void SetPrompt(string prompt) { if (promptText != null) promptText.text = prompt; }
        public void ShowNotification(string message)
        {
            if (notificationText == null) return;
            if (notificationRoutine != null) StopCoroutine(notificationRoutine);
            notificationRoutine = StartCoroutine(Notification(message));
        }
        private IEnumerator Notification(string message)
        {
            notificationText.text = message; notificationText.enabled = true;
            float t = 0;
            while (t < 2.4f) { t += Time.unscaledDeltaTime; yield return null; }
            notificationText.text = string.Empty; notificationRoutine = null;
        }
        private void ToggleSound() { muted = !muted; manager.Audio.Toggle(); ShowNotification(muted ? "Sound muted." : "Sound enabled."); }
        private static void Quit() { Application.Quit(); }

        private void Set(GameObject obj, bool value) { if (obj != null) obj.SetActive(value); }
        private GameObject MakePanel(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = size;
            var image = obj.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return obj;
        }
        private Text Label(Transform parent, string value, int size, Color color, Vector2 anchor, Vector2 dimensions, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var obj = new GameObject(value.Length > 26 ? "Text" : value); obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = anchor; rect.sizeDelta = dimensions;
            var text = obj.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = size; text.color = color; text.alignment = align; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow; text.raycastTarget = false;
            return text;
        }
        private Button Button(Transform parent, string label, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction callback, bool primary)
        {
            var obj = new GameObject(label); obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = size;
            var image = obj.AddComponent<Image>(); image.color = primary ? new Color(0.72f, 0.4f, 0.12f) : new Color(0.12f, 0.25f, 0.29f, 0.97f);
            var button = obj.AddComponent<Button>(); button.targetGraphic = image; var colors = button.colors; colors.highlightedColor = new Color(1f, 0.72f, 0.25f); colors.pressedColor = new Color(0.52f, 0.27f, 0.08f); button.colors = colors; button.onClick.AddListener(callback);
            var text = Label(obj.transform, label, 21, Color.white, new Vector2(0.5f, 0.5f), size - new Vector2(16, 8)); text.raycastTarget = false;
            return button;
        }
    }
}

