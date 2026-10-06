using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PeakClimber
{
    public class MobileTouchController : MonoBehaviour
    {
        [Header("References")]
        public ScoutClimber scout;

        [Header("Joystick Settings")]
        public RectTransform joystickBackground;
        public RectTransform joystickHandle;
        public float joystickRadius = 80f;

        [Header("Action Buttons")]
        public Button jumpButton;
        public Button climbButton;
        public Button pitonButton;
        public Image climbButtonBg;

        [Header("HUD Elements")]
        public Text altitudeText;
        public Slider mountainProgressSlider;
        public Text badgeCounterText;
        public Text pitonCountText;
        public Button soundButton;
        public Text soundButtonText;

        [Header("Victory Screen")]
        public GameObject victoryPanel;
        public Text victoryStatsText;
        public Button playAgainButton;

        private Vector2 joystickInput = Vector2.zero;
        private bool isTouchingJoystick = false;
        private bool climbHeld = false;

        private void Start()
        {
            if (scout == null)
            {
                scout = FindAnyObjectByType<ScoutClimber>();
            }

            SetupJoystickTouchEvents();
            SetupButtonEvents();
        }

        private void SetupJoystickTouchEvents()
        {
            if (joystickBackground == null) return;

            EventTrigger trigger = joystickBackground.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = joystickBackground.gameObject.AddComponent<EventTrigger>();

            // Pointer Down
            EventTrigger.Entry entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener((data) => { OnJoystickDown((PointerEventData)data); });
            trigger.triggers.Add(entryDown);

            // Drag
            EventTrigger.Entry entryDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            entryDrag.callback.AddListener((data) => { OnJoystickDrag((PointerEventData)data); });
            trigger.triggers.Add(entryDrag);

            // Pointer Up
            EventTrigger.Entry entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener((data) => { OnJoystickUp((PointerEventData)data); });
            trigger.triggers.Add(entryUp);
        }

        private void SetupButtonEvents()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(() =>
                {
                    if (scout != null) scout.PressJump();
                });
            }

            if (climbButton != null)
            {
                climbButton.onClick.AddListener(() =>
                {
                    climbHeld = !climbHeld;
                    if (scout != null) scout.SetClimbHeld(climbHeld);
                    UpdateClimbButtonVisual();
                });
            }

            if (pitonButton != null)
            {
                pitonButton.onClick.AddListener(() =>
                {
                    if (scout != null) scout.PressPiton();
                });
            }

            if (soundButton != null)
            {
                soundButton.onClick.AddListener(() =>
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.ToggleMute();
                        UpdateSoundButtonVisual();
                    }
                });
            }

            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.RestartGame();
                });
            }
        }

        private void OnJoystickDown(PointerEventData eventData)
        {
            isTouchingJoystick = true;
            OnJoystickDrag(eventData);
        }

        private void OnJoystickDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                Vector2 clamped = Vector2.ClampMagnitude(localPoint, joystickRadius);
                if (joystickHandle != null)
                {
                    joystickHandle.anchoredPosition = clamped;
                }
                joystickInput = clamped / joystickRadius;
            }
        }

        private void OnJoystickUp(PointerEventData eventData)
        {
            isTouchingJoystick = false;
            joystickInput = Vector2.zero;
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }

        private void Update()
        {
            if (scout != null)
            {
                // Pass touch joystick input to scout
                if (isTouchingJoystick || joystickInput.sqrMagnitude > 0.01f)
                {
                    scout.SetMoveInput(joystickInput);
                }

                // Update HUD metrics
                float alt = Mathf.Max(0f, scout.transform.position.y);
                if (altitudeText != null)
                {
                    altitudeText.text = $"ALT: {alt:F0}m / 350m";
                }

                if (mountainProgressSlider != null)
                {
                    mountainProgressSlider.value = Mathf.Clamp01(alt / 350f);
                }

                if (pitonCountText != null)
                {
                    pitonCountText.text = $"x{scout.pitonCount}";
                }
            }

            if (GameManager.Instance != null && badgeCounterText != null)
            {
                badgeCounterText.text = $"Badges: {GameManager.Instance.BadgesCollected} / 5";
            }
        }

        private void UpdateClimbButtonVisual()
        {
            if (climbButtonBg != null)
            {
                climbButtonBg.color = climbHeld ? new Color(0.2f, 0.85f, 0.4f, 0.95f) : new Color(0.15f, 0.2f, 0.25f, 0.8f);
            }
        }

        private void UpdateSoundButtonVisual()
        {
            if (soundButtonText != null && SoundManager.Instance != null)
            {
                soundButtonText.text = SoundManager.Instance.IsMuted ? "🔇" : "🔊";
            }
        }

        public void ShowVictoryScreen(int badges, float time, int falls)
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                if (victoryStatsText != null)
                {
                    string rank = badges >= 5 ? "LEGENDARY PEAK SCOUT ⭐⭐⭐" : (badges >= 3 ? "EXPERT MOUNTAINEER ⭐⭐" : "BRAVE SCOUT ⭐");
                    int mins = (int)(time / 60);
                    int secs = (int)(time % 60);
                    victoryStatsText.text = $"SUMMIT REACHED!\n\nRank: {rank}\nTime: {mins:D2}:{secs:D2}\nBadges: {badges}/5\nFalls: {falls}\n\nHelicopter Rescue Confirmed!";
                }
            }
        }
    }
}
