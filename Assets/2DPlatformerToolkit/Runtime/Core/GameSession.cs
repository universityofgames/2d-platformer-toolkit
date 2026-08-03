using System;
using UnityEngine;

namespace PlatformerToolkit.Core
{
    /// <summary>
    /// Lightweight session state shared across scenes, such as the collected
    /// coin total. An instance is created on demand, so no scene setup is
    /// required; add one manually only when you want to configure it.
    /// </summary>
    [AddComponentMenu("2D Platformer Toolkit/Core/Game Session")]
    [DisallowMultipleComponent]
    public sealed class GameSession : MonoBehaviour
    {
        private static GameSession instance;

        /// <summary>
        /// Raised whenever the coin total changes. Carries the new total.
        /// </summary>
        public static event Action<int> CoinsChanged;

        /// <summary>
        /// Raised whenever the key count changes. Carries the new count.
        /// </summary>
        public static event Action<int> KeysChanged;

        /// <summary>
        /// The active session. Created on demand when none exists yet.
        /// </summary>
        public static GameSession Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<GameSession>();
                    if (instance == null)
                    {
                        var sessionObject = new GameObject(nameof(GameSession));
                        instance = sessionObject.AddComponent<GameSession>();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Total coins collected during this session.
        /// </summary>
        public int Coins { get; private set; }

        /// <summary>
        /// Keys currently held.
        /// </summary>
        public int Keys { get; private set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Adds coins to the session total and notifies listeners.
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0)
                return;

            Coins += amount;
            CoinsChanged?.Invoke(Coins);
        }

        /// <summary>
        /// Adds keys and notifies listeners.
        /// </summary>
        public void AddKeys(int amount)
        {
            if (amount <= 0)
                return;

            Keys += amount;
            KeysChanged?.Invoke(Keys);
        }

        /// <summary>
        /// Consumes one key. Returns false when none are held.
        /// </summary>
        public bool TryUseKey()
        {
            if (Keys <= 0)
                return false;

            Keys--;
            KeysChanged?.Invoke(Keys);
            return true;
        }

        /// <summary>
        /// Clears all session state, e.g. when starting a new game.
        /// </summary>
        public void ResetSession()
        {
            Coins = 0;
            Keys = 0;
            CoinsChanged?.Invoke(Coins);
            KeysChanged?.Invoke(Keys);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            CoinsChanged = null;
            KeysChanged = null;
        }
    }
}
