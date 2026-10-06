using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakClimber
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int BadgesCollected { get; private set; } = 0;
        public int TotalFalls { get; private set; } = 0;
        public float ElapsedTime { get; private set; } = 0f;
        public bool IsGameWon { get; private set; } = false;

        private ScoutClimber scout;
        private MobileTouchController uiController;
        private CampfireCheckpoint lastCheckpoint;
        private List<string> collectedBadgeNames = new List<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            scout = FindAnyObjectByType<ScoutClimber>();
            uiController = FindAnyObjectByType<MobileTouchController>();
        }

        private void Update()
        {
            if (!IsGameWon)
            {
                ElapsedTime += Time.deltaTime;
            }
        }

        public void CollectBadge(string badgeName)
        {
            if (!collectedBadgeNames.Contains(badgeName))
            {
                collectedBadgeNames.Add(badgeName);
                BadgesCollected++;
            }
        }

        public void RegisterFall()
        {
            TotalFalls++;
        }

        public void OnCheckpointReached(CampfireCheckpoint cp)
        {
            lastCheckpoint = cp;
            if (scout != null)
            {
                scout.SetCheckpoint(cp.transform.position);
            }
        }

        public void OnGameWon()
        {
            if (IsGameWon) return;
            IsGameWon = true;

            if (uiController != null)
            {
                uiController.ShowVictoryScreen(BadgesCollected, ElapsedTime, TotalFalls);
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
