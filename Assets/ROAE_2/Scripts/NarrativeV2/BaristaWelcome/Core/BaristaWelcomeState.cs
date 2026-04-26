using UnityEngine;

public static class BaristaWelcomeState
{
    private const string BaristaNpcId = "barista";

    public static bool GetFlag(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static void SetFlag(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value ?? string.Empty);
        PlayerPrefs.Save();
    }

    public static BaristaDrinkType GetHeldDrink()
    {
        return ReadDrink(BaristaWelcomeKeys.HeldDrink);
    }

    public static void SetHeldDrink(BaristaDrinkType drink)
    {
        SetDrinkState(drink, GetPendingDrink());
    }

    public static BaristaDrinkType GetPendingDrink()
    {
        return ReadDrink(BaristaWelcomeKeys.PendingDrink);
    }

    public static void SetPendingDrink(BaristaDrinkType drink)
    {
        SetDrinkState(GetHeldDrink(), drink);
    }

    public static void SetDrinkState(BaristaDrinkType heldDrink, BaristaDrinkType pendingDrink)
    {
        BaristaDrinkType previousPending = GetPendingDrink();
        BaristaDrinkType normalizedHeld = NormalizeDrink(heldDrink);
        BaristaDrinkType normalizedPending = normalizedHeld == BaristaDrinkType.None
            ? NormalizeDrink(pendingDrink)
            : BaristaDrinkType.None;

        SetDrinkRaw(BaristaWelcomeKeys.HeldDrink, normalizedHeld);
        SetDrinkRaw(BaristaWelcomeKeys.PendingDrink, normalizedPending);
        SyncDrinkFlags();

        if (normalizedPending == BaristaDrinkType.None)
            SetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, false);
        else if (previousPending != normalizedPending)
            SetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, false);
    }

    public static bool HasHeldDrink()
    {
        return GetHeldDrink() != BaristaDrinkType.None;
    }

    public static bool IsDrinkDeliveryPending()
    {
        return GetPendingDrink() != BaristaDrinkType.None;
    }

    public static bool HasAlreadyDrink()
    {
        return HasHeldDrink() || GetPendingDrink() != BaristaDrinkType.None;
    }

    public static bool HasAcceptedFirstDrink()
    {
        return GetPendingDrink() != BaristaDrinkType.None;
    }

    public static bool HasAcknowledgedPendingDrink()
    {
        return GetPendingDrink() != BaristaDrinkType.None &&
               GetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged);
    }

    public static void AcknowledgePendingDrink()
    {
        if (GetPendingDrink() == BaristaDrinkType.None)
            return;

        SetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, true);
    }

    public static void SetAcceptedFirstDrink(bool value)
    {
        if (value)
        {
            if (!HasHeldDrink() && GetPendingDrink() == BaristaDrinkType.None)
                SetDrinkState(BaristaDrinkType.None, BaristaDrinkType.PhotosyntheticSap);

            return;
        }

        if (GetPendingDrink() != BaristaDrinkType.None)
            SetDrinkState(GetHeldDrink(), BaristaDrinkType.None);
        else
            SyncDrinkFlags();
    }

    public static BaristaIntroTone GetIntroTone()
    {
        int raw = GetInt(BaristaWelcomeKeys.BaristaIntroTone, 0);
        if (raw < 0 || raw > (int)BaristaIntroTone.Mischievous)
            return BaristaIntroTone.Neutral;
        return (BaristaIntroTone)raw;
    }

    public static void SetIntroTone(BaristaIntroTone tone)
    {
        SetInt(BaristaWelcomeKeys.BaristaIntroTone, (int)tone);
    }

    public static int GetBaristaRelationship()
    {
        int value = NpcRelationshipState.GetRelationshipScore(BaristaNpcId);
        SetInt(BaristaWelcomeKeys.BaristaRelationship, value);
        return value;
    }

    public static void SetBaristaRelationship(int value)
    {
        NpcRelationshipState.SetRelationship(BaristaNpcId, value);
        SetInt(BaristaWelcomeKeys.BaristaRelationship, value);
    }

    public static void AdjustBaristaRelationship(int delta)
    {
        NpcRelationshipState.AdjustRelationship(BaristaNpcId, delta);
        SetInt(BaristaWelcomeKeys.BaristaRelationship, NpcRelationshipState.GetRelationshipScore(BaristaNpcId));
    }

    public static void ApplyNaiveResponseEffects()
    {
        AdjustBaristaRelationship(4);
        ApplyStats(1, 1, 1);
    }

    public static void ApplyGuardedResponseEffects()
    {
        AdjustBaristaRelationship(-2);
        ApplyStats(0, -1, 0);
    }

    public static bool DeliverPendingDrinkIfPossible()
    {
        BaristaDrinkType pending = GetPendingDrink();
        if (pending == BaristaDrinkType.None || HasHeldDrink())
            return false;

        SetDrinkState(pending, BaristaDrinkType.None);
        return true;
    }

    public static void GiveFirstAcceptedDrinkIfPossible()
    {
        DeliverPendingDrinkIfPossible();
    }

    public static bool TryOrderCola()
    {
        if (HasAlreadyDrink())
            return false;

        SetDrinkState(BaristaDrinkType.Cola, BaristaDrinkType.None);
        return true;
    }

    public static bool TryOrderPhotosyntheticSap()
    {
        if (HasAlreadyDrink())
            return false;

        SetDrinkState(BaristaDrinkType.None, BaristaDrinkType.PhotosyntheticSap);
        return true;
    }

    public static bool TryDrinkHeldDrink()
    {
        BaristaDrinkType held = GetHeldDrink();
        if (held == BaristaDrinkType.None)
            return false;

        if (held == BaristaDrinkType.Cola)
            SetFlag(BaristaWelcomeKeys.DrankCola, true);

        if (held == BaristaDrinkType.PhotosyntheticSap)
            SetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink, true);

        SetDrinkState(BaristaDrinkType.None, BaristaDrinkType.None);
        return true;
    }

    public static void DiscardHeldDrink()
    {
        if (!HasHeldDrink())
            return;

        SetDrinkState(BaristaDrinkType.None, GetPendingDrink());
    }

    public static bool IsMomentComplete()
    {
        return GetFlag(BaristaWelcomeKeys.BaristaIntroDone) && GetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink);
    }

    public static void ResetAll()
    {
        SetFlag(BaristaWelcomeKeys.ReadUnknownText01, false);
        SetFlag(BaristaWelcomeKeys.BaristaIntroDone, false);
        SetFlag(BaristaWelcomeKeys.DrankPhotosyntheticDrink, false);
        SetFlag(BaristaWelcomeKeys.DrankCola, false);
        SetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, false);
        SetDrinkState(BaristaDrinkType.None, BaristaDrinkType.None);
        SetIntroTone(BaristaIntroTone.Neutral);
        SetBaristaRelationship(0);
    }

    private static BaristaDrinkType ReadDrink(string key)
    {
        int raw = GetInt(key, 0);
        if (raw < 0 || raw > (int)BaristaDrinkType.PhotosyntheticSap)
            return BaristaDrinkType.None;

        return (BaristaDrinkType)raw;
    }

    private static void SetDrinkRaw(string key, BaristaDrinkType drink)
    {
        SetInt(key, (int)NormalizeDrink(drink));
    }

    private static BaristaDrinkType NormalizeDrink(BaristaDrinkType drink)
    {
        if (drink != BaristaDrinkType.Cola && drink != BaristaDrinkType.PhotosyntheticSap)
            return BaristaDrinkType.None;

        return drink;
    }

    private static void SyncDrinkFlags()
    {
        bool pending = GetPendingDrink() != BaristaDrinkType.None;
        bool held = GetHeldDrink() != BaristaDrinkType.None;
        bool hasAlreadyDrink = pending || held;

        SetFlag(BaristaWelcomeKeys.DrinkDeliveryPending, pending);
        SetFlag(BaristaWelcomeKeys.HasAlreadyDrink, hasAlreadyDrink);
        SetFlag(BaristaWelcomeKeys.AcceptedFirstDrink, pending);
    }

    private static void ApplyStats(int creativityDelta, int empathyDelta, int corruptionDelta)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        if (core == null)
            return;

        CreativeHUD hud = CreativeHUD.Instance;

        if (creativityDelta != 0)
        {
            core.AdjustCreativity(creativityDelta);
            if (hud != null) hud.ShowStatChange("creativity", creativityDelta);
        }

        if (empathyDelta != 0)
        {
            core.AdjustEmpathy(empathyDelta);
            if (hud != null) hud.ShowStatChange("empathy", empathyDelta);
        }

        if (corruptionDelta != 0)
        {
            core.AdjustCorruption(corruptionDelta);
            if (hud != null) hud.ShowStatChange("plantCorruption", corruptionDelta);
        }

        core.PrintStats();
    }
}
