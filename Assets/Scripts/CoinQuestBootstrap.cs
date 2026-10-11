using UnityEngine;

namespace CoinQuest
{
    public static class CoinQuestBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (GameManager.Instance != null) return;
            Camera old = Camera.main;
            if (old != null) { old.enabled = false; var listener = old.GetComponent<AudioListener>(); if (listener != null) listener.enabled = false; }
            var root = new GameObject("Coin Quest Runtime");
            var manager = root.AddComponent<GameManager>();
            var builder = root.AddComponent<IslandLevelBuilder>();
            var ui = root.AddComponent<UIManager>();
            var audio = root.AddComponent<AudioManager>();
            manager.Configure(builder, ui, audio);
            audio.Build();
            builder.BuildWorld(manager);
            ui.Build(manager);
            manager.PrepareMenu();
        }
    }
}
