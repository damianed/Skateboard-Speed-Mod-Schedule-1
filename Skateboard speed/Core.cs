using MelonLoader;
using HarmonyLib;

using Il2CppScheduleOne.AvatarFramework.Animation;
using Il2CppScheduleOne.Skating;
using Skateboard_speed;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(Skateboard_speed.Core), "Skateboard speed", "1.0.0", "damianed", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Skateboard_speed
{
    public class Core : MelonMod
    {
        private static Skateboard activeBoard;
        private static GameObject speedCanvas;
        private static TextMeshProUGUI speedText;
        private static TextMeshProUGUI topSpeedText;
        private static int topSpeed = 0;
        private static bool isCanvasActive = false;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnUpdate()
        {
            if (activeBoard != null && isCanvasActive)
            {
                UpdateSkateboardSpeedText();
            }

            base.OnUpdate();
        }

        private static void UpdateSkateboardSpeedText()
        {
            if (speedText != null && activeBoard != null)
            {

                float speed = MathF.Abs(activeBoard.VelocityCalculator.Velocity.magnitude * 3.6f * 1.4f);

                speedText.text = MathF.Abs(speed).ToString("0") + " km/h";
            }
        }

        private static void SetupSpeedCanvas()
        {
            if (speedCanvas == null)
            {
                speedCanvas = new GameObject("SkateboardSpeedCanvas");
                Canvas canvas = speedCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                speedCanvas.AddComponent<CanvasScaler>();
                speedCanvas.AddComponent<GraphicRaycaster>();

                GameObject textObj = new GameObject("SpeedText");
                textObj.transform.SetParent(speedCanvas.transform, false);

                RectTransform rectTransform = textObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.86f, 0.08f);
                rectTransform.anchorMax = new Vector2(0.86f, 0.08f);
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.anchoredPosition = new Vector2(0, 0);

                speedText = textObj.AddComponent<TextMeshProUGUI>();
                speedText.alignment = TextAlignmentOptions.Right;
                speedText.fontSize = 28;
                speedText.color = Color.white;

                speedCanvas.SetActive(false);
            }
        }

        public static void OnSkateboardMounted(Skateboard board)
        {
            Melon<Core>.Logger.Msg("Skateboard Mounted");
            activeBoard = board;
            activeBoard.VelocityCalculator.MaxReasonableVelocity = 1000f;

            SetupSpeedCanvas();

            speedCanvas.SetActive(true);
            isCanvasActive = true;
        }

        public static void OnSkateboardDismounted()
        {
            Melon<Core>.Logger.Msg("Skateboard Dismounted");
            activeBoard = null;
            if (speedCanvas != null)
            {
                speedCanvas.SetActive(false);
                isCanvasActive = false;
            }
        }
    }
}

[HarmonyPatch(typeof(AvatarAnimation), "SkateboardMounted", new Type[] { typeof(Skateboard) })]
public static class SkateBoardMountedPatch
{
    private static void Prefix(Skateboard board)
    {
        Core.OnSkateboardMounted(board);
    }
}

[HarmonyPatch(typeof(AvatarAnimation), "SkateboardDismounted")]
public static class SkateboardDismountedPatch
{
    private static void Prefix()
    {
        Core.OnSkateboardDismounted();
    }
}