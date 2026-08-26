namespace Expriverse.UI {

    //public class DifficultyUI : MonoBehaviour {

    //    [Header("UI References")]
    //    [SerializeField] private TextMeshProUGUI timeText;
    //    [SerializeField] private TextMeshProUGUI timeDLText;
    //    [SerializeField] private TextMeshProUGUI partyDLText;
    //    [SerializeField] private TextMeshProUGUI finalDLText;
    //    [SerializeField] private TextMeshProUGUI playerCountText;
    //    [SerializeField] private TextMeshProUGUI bossTierText;

    //    [Header("Update Settings")]
    //    [SerializeField] private float updateInterval = 0.5f;

    //    private DifficultyManager difficultyManager;
    //    private float lastUpdateTime;

    //    [Inject]
    //    internal void Inject(DifficultyManager manager) {
    //        difficultyManager = manager;
    //    }

    //    private void Update() {
    //        if (difficultyManager == null) {
    //            return;
    //        }

    //        if (Time.time - lastUpdateTime >= updateInterval) {
    //            UpdateAllUI();
    //            lastUpdateTime = Time.time;
    //        }
    //    }

    //    private void UpdateAllUI() {
    //        UpdateTimeUI();
    //        UpdateDLUI();
    //        UpdatePlayerCountUI();
    //        UpdateBossTierUI();
    //    }

    //    private void UpdateTimeUI() {
    //        if (timeText != null) {
    //            timeText.text = difficultyManager.GetGameTimeFormatted();
    //        }
    //    }

    //    private void UpdateDLUI() {
    //        if (timeDLText != null) {
    //            timeDLText.text = $"Time: {difficultyManager.CurrentTimeDL:F1}";
    //        }

    //        if (partyDLText != null && difficultyManager.Config != null) {
    //            float partyDL = difficultyManager.Config.EvaluatePartyDL(difficultyManager.CurrentPlayerCount);
    //            partyDLText.text = $"Party: {partyDL:F2}";
    //        }

    //        if (finalDLText != null) {
    //            finalDLText.text = $"DL: {difficultyManager.CurrentFinalDL:F2}";
    //        }
    //    }

    //    private void UpdatePlayerCountUI() {
    //        if (playerCountText != null) {
    //            playerCountText.text = $"Players: {difficultyManager.CurrentPlayerCount}";
    //        }
    //    }

    //    private void UpdateBossTierUI() {
    //        if (bossTierText != null) {
    //            float bossHealth = difficultyManager.CurrentBossHealthMultiplier;
    //            bossTierText.text = $"Boss HP: x{bossHealth:F1}";
    //        }
    //    }
    //}
}
