using DG.Tweening;
using HarmonyLib;
using Reptile.Phone;
using Reptile;
using System;
using System.Reflection;
using UnityEngine;
using static FastPhone.ReflectionCalls;

namespace FastPhone
{
    internal class PhonePatches : HarmonyPatch
    {
        private static bool waitForRelease = false;

        [HarmonyPatch(typeof(Phone), "InitTurnOnAnimation")]
        public static class Phone_InitTurnOnAnimation_Patch
        {
            public static bool Prefix(Phone __instance, ref Sequence ___m_PhoneAnimation, RectTransform ___m_PhoneOpenTransform, PhoneTransformState ___m_OpenState, RectTransform ___m_Screen, float ___phoneScale, App ___m_CurrentApp)
            {
                ___m_PhoneAnimation = DOTween.Sequence();
                ___m_PhoneAnimation.SetUpdate(UpdateType.Manual);
                ___m_PhoneAnimation.SetAutoKill(false);
                ___m_PhoneAnimation.Pause<Sequence>();

                ___m_PhoneAnimation.InsertCallback(0f, delegate
                {
                    ___m_PhoneOpenTransform.anchoredPosition = ___m_OpenState.Position;

                    Type PhoneState = __instance.GetType().GetNestedType("PhoneState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    SetFieldValue(__instance, "state", Enum.Parse(PhoneState, "ON"));
                    ___m_Screen.gameObject.SetActive(true);
                    ___m_Screen.localScale = new Vector3(___phoneScale, ___phoneScale, 1f);

                    if (___m_CurrentApp != null)
                        ___m_CurrentApp.OnAppEnable();
                });

                return false;
            }
        }

        [HarmonyPatch(typeof(Phone), "InitTurnOffAnimation")]
        public static class Phone_InitTurnOffAnimation_Patch
        {
            public static bool Prefix(Phone __instance, ref Sequence ___m_PhoneTurnOffAnimation, RectTransform ___m_PhoneOpenTransform, PhoneTransformState ___m_ClosedState)
            {
                ___m_PhoneTurnOffAnimation = DOTween.Sequence();
                ___m_PhoneTurnOffAnimation.SetUpdate(UpdateType.Manual);
                ___m_PhoneTurnOffAnimation.SetAutoKill(false);
                ___m_PhoneTurnOffAnimation.Pause<Sequence>();

                ___m_PhoneTurnOffAnimation.AppendCallback(delegate
                {
                    ___m_PhoneOpenTransform.anchoredPosition = ___m_ClosedState.Position;
                    InvokeMethod(__instance, "DisableOS");
                    InvokeMethod(__instance, "SetPhoneUIEnabled", false, false);
                });

                return false;
            }
        }

        [HarmonyPatch(typeof(PhoneScroll), "SelectButton")]
        public static class PhoneScroll_SelectButton_Patch
        {
            public static void Prefix(ref bool skipAnimation)
            {
                skipAnimation = true;
            }
        }

        [HarmonyPatch(typeof(AppHomeScreen), "OpenApp")]
        public static class AppHomeScreen_OpenApp_Patch
        {
            public static bool Prefix(AppHomeScreen __instance, HomescreenButton appToOpen)
            {
                if (appToOpen.AssignedApp.AppType != HomeScreenApp.HomeScreenAppType.PHOTO_ALBUM)
                {
                    Player player = WorldHandler.instance?.GetCurrentPlayer();
                    Phone phone = GetFieldValue<Phone>(player, "phone");

                    if (phone.AppInstances.TryGetValue(appToOpen.AssignedApp.AppName, out App app))
                    {
                        PropertyInfo HandleInput = app.GetType().GetProperty("HandleInput");

                        HandleInput.SetValue(app, false, null);
                        phone?.OpenApp(app.GetType());
                        waitForRelease = true;
                    }

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Phone), "GetInput")]
        public static class Phone_GetInput_Patch
        {
            public static bool Prefix(App app, GameInput ___gameInput)
            {
                if (waitForRelease && (___gameInput.GetButtonNew(21, 0) || ___gameInput.GetButtonHeld(29, 0)) && !___gameInput.GetButtonUp(29, 0))
                    return false;

                if (waitForRelease)
                {
                    PropertyInfo HandleInput = app.GetType().GetProperty("HandleInput");
                    HandleInput.SetValue(app, true, null);

                    waitForRelease = false;

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Phone), "TurnOn")]
        public static class Phone_TurnOn_Patch
        {
            public static bool Prefix(GameInput ___gameInput)
            {
                if (___gameInput.GetButtonNew(57, 0))
                    return false;

                Mapcontroller instance = Mapcontroller.Instance;
                if (instance != null)
                {
                    instance.EnableMapCamera();
                    instance.RestoreDefaultMapControllerState();
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AppHomeScreen), "AwakeAnimation")]
        public static class AppHomeScreen_AwakeAnimation_Patch
        {
            public static bool Prefix(AppHomeScreen __instance, HomescreenScrollView ___m_ScrollView, RectTransform ___m_TopView)
            {
                ___m_ScrollView.OtherElementsParent.gameObject.SetActive(true);
                (___m_ScrollView.SelectedButtton as HomescreenButton).IsSelected = true;
                ___m_TopView.sizeDelta = new Vector2(___m_TopView.sizeDelta.x, 775f);

                Player player = WorldHandler.instance?.GetCurrentPlayer();
                Phone phone = GetFieldValue<Phone>(player, "phone");

                if (phone != null)
                {
                    for (int i = 0; i < ___m_ScrollView.GetScrollRange(); i++)
                    {
                        RectTransform rectTransform = ___m_ScrollView.GetButtonByRelativeIndex(i).RectTransform();
                        Vector2 endValue = ___m_ScrollView.ButtonPos((float)i);
                        rectTransform.anchoredPosition = endValue;
                    }

                    Vector2 anchoredPosition = ___m_ScrollView.Selector.RectTransform().anchoredPosition;
                    anchoredPosition.y = ___m_ScrollView.SelectedButtton.RectTransform().anchoredPosition.y;
                    ___m_ScrollView.Selector.RectTransform().localScale = Vector2.one;
                    ___m_ScrollView.Selector.RectTransform().anchoredPosition = anchoredPosition;
                }

                if (__instance.gameObject.TryGetComponent(out App app))
                {
                    PropertyInfo HandleInput = app.GetType().GetProperty("HandleInput");
                    HandleInput.SetValue(app, true, null);
                }

                return false;
            }
        }
    }
}
