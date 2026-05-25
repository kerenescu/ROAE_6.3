using UnityEngine;

public static class BaristaWelcomeState
{
    private const string BaristaNpcId = "barista";
    private const BaristaDrinkType DefaultDrinkType = BaristaDrinkType.Cola;

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
        if (!HasDrink())
            return BaristaDrinkType.None;

        BaristaDrinkType storedDrink = ReadDrink(BaristaWelcomeKeys.HeldDrink);
        return storedDrink == BaristaDrinkType.None ? DefaultDrinkType : storedDrink;
    }

    public static void SetHeldDrink(BaristaDrinkType drink)
    {
        SetDrinkState(drink, GetPendingDrink());
    }

    public static BaristaDrinkType GetPendingDrink()
    {
        return BaristaDrinkType.None;
    }

    public static void SetPendingDrink(BaristaDrinkType drink)
    {
        SetDrinkState(GetHeldDrink(), drink);
    }

    public static void SetDrinkState(BaristaDrinkType heldDrink, BaristaDrinkType pendingDrink)
    {
        BaristaDrinkType normalizedHeld = NormalizeDrink(heldDrink);
        BaristaDrinkType normalizedPending = NormalizeDrink(pendingDrink);
        BaristaDrinkType canonicalDrink = normalizedHeld != BaristaDrinkType.None
            ? normalizedHeld
            : normalizedPending;

        SetDrinkRaw(BaristaWelcomeKeys.HeldDrink, canonicalDrink);
        SetDrinkRaw(BaristaWelcomeKeys.PendingDrink, BaristaDrinkType.None);
        SyncDrinkFlags(canonicalDrink != BaristaDrinkType.None);
    }

    public static bool HasHeldDrink()
    {
        return HasDrink();
    }

    public static bool IsDrinkDeliveryPending()
    {
        return false;
    }

    public static bool HasAlreadyDrink()
    {
        return HasDrink();
    }

    public static bool HasDrink()
    {
        return GetFlag(BaristaWelcomeKeys.HasAlreadyDrink);
    }

    public static void SetHasDrink(bool value)
    {
        if (!value)
        {
            SetDrinkRaw(BaristaWelcomeKeys.HeldDrink, BaristaDrinkType.None);
            SetDrinkRaw(BaristaWelcomeKeys.PendingDrink, BaristaDrinkType.None);
            SyncDrinkFlags(false);
            return;
        }

        BaristaDrinkType currentDrink = GetHeldDrink();
        if (currentDrink == BaristaDrinkType.None)
            currentDrink = DefaultDrinkType;

        SetDrinkRaw(BaristaWelcomeKeys.HeldDrink, currentDrink);
        SetDrinkRaw(BaristaWelcomeKeys.PendingDrink, BaristaDrinkType.None);
        SyncDrinkFlags(true);
    }

    public static bool HasAcceptedFirstDrink()
    {
        return HasDrink();
    }

    public static bool HasAcknowledgedPendingDrink()
    {
        return false;
    }

    public static void AcknowledgePendingDrink()
    {
        SetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, false);
    }

    public static void SetAcceptedFirstDrink(bool value)
    {
        if (value)
            SetDrinkState(BaristaDrinkType.PhotosyntheticSap, BaristaDrinkType.None);
        else
            SetHasDrink(false);
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
        if (HasDrink())
            return false;

        SetHasDrink(true);
        return true;
    }

    public static void GiveFirstAcceptedDrinkIfPossible()
    {
        DeliverPendingDrinkIfPossible();
    }

    public static bool TryOrderCola()
    {
        if (HasDrink())
            return false;

        SetDrinkState(BaristaDrinkType.Cola, BaristaDrinkType.None);
        return true;
    }

    public static bool TryOrderPhotosyntheticSap()
    {
        if (HasDrink())
            return false;

        SetDrinkState(BaristaDrinkType.PhotosyntheticSap, BaristaDrinkType.None);
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
        if (!HasDrink())
            return;

        SetHasDrink(false);
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
        SetHasDrink(false);
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

    private static void SyncDrinkFlags(bool hasDrink)
    {
        SetFlag(BaristaWelcomeKeys.DrinkDeliveryPending, false);
        SetFlag(BaristaWelcomeKeys.DrinkDeliveryAcknowledged, false);
        SetFlag(BaristaWelcomeKeys.HasAlreadyDrink, hasDrink);
        SetFlag(BaristaWelcomeKeys.AcceptedFirstDrink, hasDrink);
    }

    private static void ApplyStats(int creativityDelta, int empathyDelta, int corruptionDelta)
    {
        CreativeCore core = CreativeCore.Instance ?? Object.FindFirstObjectByType<CreativeCore>();
        if (core == null)
            return;

        CreativeHUD hud = CreativeHUD.Instance;
        int appliedEmpathyDelta = CreativeStatScale.ConvertLegacyEmpathyDelta(empathyDelta);
        int appliedCorruptionDelta = CreativeStatScale.ConvertLegacyCorruptionDelta(corruptionDelta);

        if (creativityDelta != 0)
        {
            core.AdjustCreativity(creativityDelta);
            if (hud != null) hud.ShowStatChange("creativity", creativityDelta);
        }

        if (empathyDelta != 0)
        {
            core.AdjustEmpathy(appliedEmpathyDelta);
            if (hud != null) hud.ShowStatChange("empathy", appliedEmpathyDelta);
        }

        if (corruptionDelta != 0)
        {
            core.AdjustCorruption(appliedCorruptionDelta);
            if (hud != null) hud.ShowStatChange("plantCorruption", appliedCorruptionDelta);
        }

        core.PrintStats();
    }
}
