using System.Collections;
using UnityEngine;

namespace PeakClimber
{
    public enum ScoutState
    {
        Grounded,
        Airborne,
        Climbing,
        ExhaustedFall,
        AnchoredPiton,
        Victory
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class ScoutClimber : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 6.5f;
        public float jumpForce = 12.5f;
        public float wallJumpForce = 11.0f;
        public float climbSpeed = 4.2f;

        [Header("Stamina System (PEAK)")]
        public float maxStamina = 100f;
        public float currentStamina;
        public float climbIdleDrain = 4.5f;
        public float climbMoveDrain = 15f;
        public float staminaRechargeRate = 45f;
        public bool isExhausted = false;

        [Header("Equipment")]
        public int pitonCount = 3;
        public GameObject pitonPrefab;

        [Header("Detection")]
        public LayerMask groundLayer;
        public LayerMask climbableLayer;
        public Transform groundCheck;
        public float groundCheckRadius = 0.25f;
        public Transform wallCheckFront;
        public Transform wallCheckBack;
        public float wallCheckDistance = 0.45f;

        [Header("Visuals")]
        public SpriteRenderer bodyRenderer;
        public Transform visualRoot;
        public Sprite idleSprite;
        public Sprite climbSprite;

        // Current state
        public ScoutState State { get; private set; } = ScoutState.Grounded;
        public ClimbableSurface CurrentClimbSurface { get; private set; }

        private Rigidbody2D rb;
        private Collider2D col;
        private Vector2 moveInput;
        private bool climbInputHeld = false;
        private bool jumpRequested = false;
        private bool pitonRequested = false;

        private float coyoteTimeCounter = 0f;
        private const float CoyoteTime = 0.15f;
        private float jumpBufferCounter = 0f;
        private const float JumpBufferTime = 0.15f;

        private Vector3 checkpointPos;
        private Piton activePiton;
        private float wallDirection = 1f; // 1 = wall is to the right, -1 = to the left

        // Stamina UI reference
        private StaminaWheel staminaWheel;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            currentStamina = maxStamina;
            checkpointPos = transform.position;

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (idleSprite == null) idleSprite = ProceduralSpriteGenerator.CreateScoutSprite(false);
            if (climbSprite == null) climbSprite = ProceduralSpriteGenerator.CreateScoutSprite(true);

            if (bodyRenderer != null && bodyRenderer.sprite == null)
            {
                bodyRenderer.sprite = idleSprite;
            }

            staminaWheel = GetComponentInChildren<StaminaWheel>();
        }

        private void Start()
        {
            if (staminaWheel == null)
            {
                GameObject wheelGo = new GameObject("StaminaWheel");
                wheelGo.transform.SetParent(transform);
                wheelGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                staminaWheel = wheelGo.AddComponent<StaminaWheel>();
            }
        }

        private void Update()
        {
            if (State == ScoutState.Victory) return;

            ReadFallbackKeyboardInput();

            // Jump buffer
            if (jumpRequested)
            {
                jumpBufferCounter = JumpBufferTime;
                jumpRequested = false;
            }
            else if (jumpBufferCounter > 0f)
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // Coyote time
            bool isGrounded = CheckGrounded();
            if (isGrounded)
            {
                coyoteTimeCounter = CoyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // State Machine Updates
            switch (State)
            {
                case ScoutState.Grounded:
                    UpdateGrounded(isGrounded);
                    break;
                case ScoutState.Airborne:
                    UpdateAirborne(isGrounded);
                    break;
                case ScoutState.Climbing:
                    UpdateClimbing();
                    break;
                case ScoutState.ExhaustedFall:
                    UpdateExhaustedFall(isGrounded);
                    break;
                case ScoutState.AnchoredPiton:
                    UpdateAnchoredPiton();
                    break;
            }

            // Check piton placement
            if (pitonRequested)
            {
                pitonRequested = false;
                TryPlacePiton();
            }

            // Check if fallen below world boundary
            if (transform.position.y < -15f)
            {
                RespawnAtCheckpoint();
            }

            UpdateVisuals();
            UpdateAudioAlerts();
        }

        private void FixedUpdate()
        {
            if (State == ScoutState.Victory)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            switch (State)
            {
                case ScoutState.Grounded:
                case ScoutState.Airborne:
                    // Horizontal locomotion
                    float targetVx = moveInput.x * moveSpeed;
                    rb.linearVelocity = new Vector2(targetVx, rb.linearVelocity.y);
                    break;

                case ScoutState.Climbing:
                    // Free 2D rock climbing velocity
                    float mult = CurrentClimbSurface != null ? CurrentClimbSurface.staminaDrainMultiplier : 1f;
                    Vector2 climbVel = moveInput * climbSpeed;
                    if (CurrentClimbSurface != null && CurrentClimbSurface.surfaceType == SurfaceType.Icy)
                    {
                        climbVel.y -= 0.8f; // Icy downward slip
                    }
                    rb.linearVelocity = climbVel;
                    break;

                case ScoutState.AnchoredPiton:
                    rb.linearVelocity = Vector2.zero;
                    break;
            }
        }

        private void ReadFallbackKeyboardInput()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                float h = 0f;
                float v = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;

                if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
                {
                    moveInput = new Vector2(h, v);
                }

                if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                {
                    jumpRequested = true;
                }

                if (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed || kb.cKey.isPressed || kb.zKey.isPressed)
                {
                    climbInputHeld = true;
                }
                else if (kb.leftShiftKey.wasReleasedThisFrame || kb.cKey.wasReleasedThisFrame)
                {
                    climbInputHeld = false;
                }

                if (kb.eKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
                {
                    pitonRequested = true;
                }
            }
#endif
        }

        // Mobile API inputs
        public void SetMoveInput(Vector2 dir) => moveInput = dir;
        public void PressJump() => jumpRequested = true;
        public void SetClimbHeld(bool held) => climbInputHeld = held;
        public void ToggleClimb() => climbInputHeld = !climbInputHeld;
        public void PressPiton() => pitonRequested = true;

        private void UpdateGrounded(bool isGrounded)
        {
            // Restore stamina quickly on ground
            if (currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRechargeRate * Time.deltaTime);
                isExhausted = false;
            }

            // Jump
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                DoJump();
                return;
            }

            if (!isGrounded)
            {
                State = ScoutState.Airborne;
                return;
            }

            // Check if player wants to climb adjacent rock wall
            if (climbInputHeld && CheckClimbableWall(out ClimbableSurface surface, out float dir))
            {
                EnterClimbing(surface, dir);
            }
        }

        private void UpdateAirborne(bool isGrounded)
        {
            if (isGrounded && rb.linearVelocity.y <= 0.1f)
            {
                State = ScoutState.Grounded;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayLand();
                return;
            }

            // Can grab rock wall while airborne if holding climb or touching wall
            if ((climbInputHeld || moveInput.y > 0.2f) && !isExhausted)
            {
                if (CheckClimbableWall(out ClimbableSurface surface, out float dir))
                {
                    EnterClimbing(surface, dir);
                }
            }
        }

        private void UpdateClimbing()
        {
            // Check if player released climb key
            if (!climbInputHeld)
            {
                ExitClimbing();
                return;
            }

            // Check if wall is still in contact
            if (!CheckClimbableWall(out ClimbableSurface surface, out float dir))
            {
                ExitClimbing();
                return;
            }
            CurrentClimbSurface = surface;
            wallDirection = dir;

            // Wall Jump
            if (jumpBufferCounter > 0f)
            {
                DoWallJump(-wallDirection);
                return;
            }

            // Stamina drain
            bool isMoving = moveInput.sqrMagnitude > 0.05f;
            float drainRate = isMoving ? climbMoveDrain : climbIdleDrain;
            if (CurrentClimbSurface != null) drainRate *= CurrentClimbSurface.staminaDrainMultiplier;

            currentStamina -= drainRate * Time.deltaTime;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                TriggerExhaustion();
            }
        }

        private void UpdateExhaustedFall(bool isGrounded)
        {
            // Tumble rotation
            visualRoot.Rotate(0f, 0f, -360f * Time.deltaTime);

            if (isGrounded && rb.linearVelocity.y <= 0.5f)
            {
                // Recover on ground
                visualRoot.rotation = Quaternion.identity;
                isExhausted = false;
                State = ScoutState.Grounded;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayLand();
            }
        }

        private void UpdateAnchoredPiton()
        {
            // Resting on piton restores stamina
            if (currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + (staminaRechargeRate * 0.75f) * Time.deltaTime);
            }

            // Press jump or climb to detach
            if (jumpBufferCounter > 0f || moveInput.sqrMagnitude > 0.5f)
            {
                State = ScoutState.Airborne;
                rb.gravityScale = 2.5f;
                if (activePiton != null) activePiton.isPlayerAttached = false;
                activePiton = null;
                jumpBufferCounter = 0f;
            }
        }

        private void DoJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
            State = ScoutState.Airborne;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayJump();
        }

        private void DoWallJump(float pushDirX)
        {
            ExitClimbing();
            rb.linearVelocity = new Vector2(pushDirX * wallJumpForce * 0.85f, wallJumpForce);
            currentStamina = Mathf.Max(0f, currentStamina - 8f);
            jumpBufferCounter = 0f;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayJump();
        }

        public void EnterClimbing(ClimbableSurface surface, float dir)
        {
            State = ScoutState.Climbing;
            CurrentClimbSurface = surface;
            wallDirection = dir;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            if (surface != null) surface.OnGrabbed(this);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayGrab();
        }

        public void ExitClimbing()
        {
            State = ScoutState.Airborne;
            rb.gravityScale = 2.5f;
            CurrentClimbSurface = null;
        }

        public void ForceReleaseGrip()
        {
            ExitClimbing();
        }

        private void TriggerExhaustion()
        {
            isExhausted = true;
            ExitClimbing();
            State = ScoutState.ExhaustedFall;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, -3f);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayFall();
            if (GameManager.Instance != null) GameManager.Instance.RegisterFall();
        }

        public void TryPlacePiton()
        {
            if (pitonCount <= 0 || State != ScoutState.Climbing) return;

            pitonCount--;
            GameObject pGo;
            if (pitonPrefab != null)
            {
                pGo = Instantiate(pitonPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                pGo = new GameObject("Piton_Instance");
                pGo.transform.position = transform.position;
                pGo.AddComponent<Piton>();
            }

            activePiton = pGo.GetComponent<Piton>();
            if (activePiton != null) activePiton.isPlayerAttached = true;

            State = ScoutState.AnchoredPiton;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayPiton();
        }

        public void RestoreStamina(float amount)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
            isExhausted = false;
        }

        public void Bounce(float force)
        {
            ExitClimbing();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            State = ScoutState.Airborne;
        }

        public void SetCheckpoint(Vector3 pos)
        {
            checkpointPos = pos;
            pitonCount = Mathf.Max(pitonCount, 3);
            currentStamina = maxStamina;
            isExhausted = false;
        }

        public void RespawnAtCheckpoint()
        {
            transform.position = checkpointPos + Vector3.up * 0.5f;
            rb.linearVelocity = Vector2.zero;
            visualRoot.rotation = Quaternion.identity;
            currentStamina = maxStamina;
            isExhausted = false;
            State = ScoutState.Grounded;
            if (GameManager.Instance != null) GameManager.Instance.RegisterFall();
        }

        public void TriggerVictory()
        {
            State = ScoutState.Victory;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            visualRoot.rotation = Quaternion.identity;
        }

        private bool CheckGrounded()
        {
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.6f;
            Collider2D hit = Physics2D.OverlapCircle(origin, groundCheckRadius, groundLayer);
            return hit != null && hit != col;
        }

        private bool CheckClimbableWall(out ClimbableSurface surface, out float dir)
        {
            surface = null;
            dir = 1f;

            Vector2 origin = transform.position;
            // Check right
            RaycastHit2D hitRight = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, climbableLayer);
            if (hitRight.collider != null && hitRight.collider != col)
            {
                surface = hitRight.collider.GetComponent<ClimbableSurface>();
                dir = 1f;
                return true;
            }

            // Check left
            RaycastHit2D hitLeft = Physics2D.Raycast(origin, Vector2.left, wallCheckDistance, climbableLayer);
            if (hitLeft.collider != null && hitLeft.collider != col)
            {
                surface = hitLeft.collider.GetComponent<ClimbableSurface>();
                dir = -1f;
                return true;
            }

            // Check overlap box around scout
            Collider2D overlap = Physics2D.OverlapBox(origin, new Vector2(0.8f, 1.2f), 0f, climbableLayer);
            if (overlap != null && overlap != col)
            {
                surface = overlap.GetComponent<ClimbableSurface>();
                dir = (overlap.bounds.center.x > origin.x) ? 1f : -1f;
                return true;
            }

            return false;
        }

        private void UpdateVisuals()
        {
            if (bodyRenderer == null) return;

            if (State == ScoutState.Climbing || State == ScoutState.AnchoredPiton)
            {
                bodyRenderer.sprite = climbSprite;
            }
            else
            {
                bodyRenderer.sprite = idleSprite;
            }

            // Flip facing
            if (Mathf.Abs(moveInput.x) > 0.05f)
            {
                bodyRenderer.flipX = moveInput.x < 0;
            }
        }

        private void UpdateAudioAlerts()
        {
            if (SoundManager.Instance == null) return;
            bool lowStamina = (currentStamina < 25f && State == ScoutState.Climbing);
            SoundManager.Instance.SetHeartbeat(lowStamina);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Hazard collision (Spikes / Urchins)
            if (collision.gameObject.CompareTag("Hazard") || collision.gameObject.name.Contains("Spike"))
            {
                RespawnAtCheckpoint();
                if (SoundManager.Instance != null) SoundManager.Instance.PlayFall();
            }
        }
    }
}
