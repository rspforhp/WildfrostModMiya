using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;
using String = Il2CppSystem.String;

namespace WildfrostModMiya;

public static class CardAdder
{
    public static event Action<int> OnAskForAddingCards;

    internal static void LaunchEvent()
    {
        OnAskForAddingCards?.Invoke(0);
    }

    public static Sprite ToSprite(this Texture2D t, Vector2? v = null)
    {
        var vector = v ?? new Vector2(.5f, .5f);
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), vector);
    }

    public static Sprite LoadSpriteFromCardPortraits(string name, Vector2? v = null)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(WildFrostAPIMod.ModsFolder +
                                        (name.EndsWith(".png") ? name : name + ".png")));

        return tex.ToSprite(v);
    }


    public static CardData RegisterCardInApi(this CardData t)
    {
        t.SetCustomData("AddedByApi", true);
        t.original = t;
        WildFrostAPIMod.CardDataAdditions.Add(t);
        return t;
    }

    public static CardData ModifyFields(this CardData t, Func<CardData, CardData> modifyFields)
    {
        t = modifyFields(t);
        return t;
    }

    public enum VanillaStatusEffects
    {
        None,
        AddAttackAndHealthToSummon,
        Block,
        Bombard1,
        Bombard2,
        BonusDamageEqualToDartsInHand,
        BonusDamageEqualToGoldFactor002,
        BonusDamageEqualToJuice,
        BonusDamageEqualToScrapOnBoard,
        BonusDamageEqualToScrap,
        BonusDamageEqualToShell,
        BoostEffects,
        Budge,
        CannotRecall,
        CheckHasJuice,
        Cleanse,
        CombineWhen2Deployed,
        CopyEffects,
        Crush,
        DamageEqualToHealth,
        DamageToFrontAllies,
        Demonize,
        DestroyAfterUse,
        DestroySelfAfterTurn,
        DoubleAllSpiceWhenDestroyed,
        DoubleBlock,
        DoubleInk,
        DoubleJuice,
        DoubleNegativeEffects,
        DoubleOverload,
        DoubleShell,
        DoubleShroom,
        DoubleSpice,
        DoubleVim,
        DrawCards,
        EatHealthAndAttack,
        EatHealthAndAttackAndEffects,
        EatAlliesInRowHealthAndAttack,
        EatRandomAllyHealthAndAttackAndEffects,
        Escape,
        FillBoardFinalBoss,
        FinalBossPhase2,
        Flee,
        FreeAction,
        FrenzyBossPhase2,
        Frost,
        GainFrenzyWhenWildUnitKilled,
        GainGoldRange36,
        GainGold,
        GoatWampusPhase2,
        HaltSpice,
        Haze,
        HealNoPing,
        HealFrontAllyBasedOnDamageDealt,
        HealFullAndGainEqualSpice,
        HealToFrontAllies,
        Heal,
        HighPriorityPosition,
        HitAllCrownEnemies,
        HitAllEnemiesInRow,
        HitAllEnemies,
        HitAllUndamagedEnemies,
        HitFurthestTarget,
        HitRandomTarget,
        ImmuneToFrost,
        ImmuneToSnow,
        ImmuneToSpice,
        ImmuneToVim,
        IncreaseAllSpiceAppliedNoDesc,
        IncreaseAttackAndHealth,
        IncreaseAttackAndLoseHalfHealth,
        IncreaseAttackEffect1,
        IncreaseAttackWhileAlone,
        IncreaseAttackWhileDamaged,
        IncreaseAttack,
        IncreaseEffects,
        IncreaseMaxCounter,
        IncreaseMaxHealth,
        Injury,
        InstantAddScrap,
        InstantApplyAttackToApplier,
        InstantApplyCurrentAttackToAllies,
        InstantApplyCurrentAttackToRandomAlly,
        InstantApplyFrenzyToItemInHand,
        InstantDraw,
        InstantGainAimless,
        InstantGainFury,
        InstantGainNoomlinToCardInHand,
        InstantGainSoulboundToEnemy,
        InstantSummonBloo,
        InstantSummonCopyOfItem,
        InstantSummonCopyOnOtherSideWithXHealth,
        InstantSummonCopy,
        InstantSummonDregg,
        InstantSummonFallow,
        InstantSummonGearhammerInHand,
        InstantSummonJunkInHand,
        InstantSummonSunShardInHand,
        InstantSummonTailsFour,
        InstantSummonTailsOne,
        InstantSummonTailsThree,
        InstantSummonTailsTwo,
        Kill,
        LastStand,
        LoseHalfHealth,
        LoseJuice,
        LoseScrap,
        LowPriorityPosition,
        Lumin,
        MultiHitTemporaryAndNotVisible,
        MultiHit,
        MustHaveJuiceToTrigger,
        Null,
        OnCardPlayedAddFuryToTarget,
        OnCardPlayedAddGearhammerToHand,
        OnCardPlayedAddJunkToHand,
        OnCardPlayedAddScrapToRandomAlly,
        OnCardPlayedAddSoulboundToRandomAlly,
        OnCardPlayedAddSunShardToHand,
        OnCardPlayedApplyAttackToSelf,
        OnCardPlayedApplyBlockToRandomUnit,
        OnCardPlayedApplyFrostToRandomEnemy,
        OnCardPlayedApplyHazeToRandomEnemy,
        OnCardPlayedApplyInkToRandomEnemy,
        OnCardPlayedApplyOverloadToFrontEnemy,
        OnCardPlayedApplyShellToRandomAlly,
        OnCardPlayedApplyShroomToEnemies,
        OnCardPlayedApplySnowToEnemiesInRow,
        OnCardPlayedApplySpiceToRandomAlly,
        OnCardPlayedApplyTeethToRandomAlly,
        OnCardPlayedBoostToRandomAlly,
        OnCardPlayedBoostToRandomEnemy,
        OnCardPlayedDamageFrostedEnemies,
        OnCardPlayedDamageInkedEnemies,
        OnCardPlayedDamageToSelfAndAlliesInRow,
        OnCardPlayedDamageUndamagedEnemies,
        OnCardPlayedDestroyRandomCardInHand,
        OnCardPlayedDestroyRandomJunkInHand,
        OnCardPlayedDestroyRightmostCardInHand,
        OnCardPlayedDoubleVimToSelf,
        OnCardPlayedLose1JuiceToSelfNoDesc,
        OnCardPlayedLoseScrapToSelf,
        OnCardPlayedReduceAttackEffect1ToSelf,
        OnCardPlayedReduceCounterToAllies,
        OnCardPlayedSacrificeRandomAlly,
        OnCardPlayedTakeHealthFromAllies,
        OnCardPlayedTriggerAgainstAllyBehind,
        OnCardPlayedTriggerRandomAlly,
        OnCardPlayedUseRandomItemInHandAgainstRandomEnemy,
        OnCardPlayedVimToAllies,
        OnCardPlayedVimToSelf,
        OnCardPlayedVoidToEnemies,
        OnHitDamageDamagedTarget,
        OnHitDamageFrostedTarget,
        OnHitDamageShelledTarget,
        OnHitDamageSnowedTarget,
        OnHitEqualDamageToInkedTarget,
        OnHitEqualHealToFrontAlly,
        OnHitEqualOverloadToTarget,
        OnHitEqualSnowToTarget,
        OnHitPullTarget,
        OnHitPushTarget,
        OnKillApplyAttackToSelf,
        OnKillApplyBlockToSelf,
        OnKillApplyGoldToSelf,
        OnKillApplyScrapToAllies,
        OnKillApplyScrapToAllyBehind,
        OnKillApplyScrapToRandomAlly,
        OnKillApplyShellToSelf,
        OnKillApplyStealthToSelf,
        OnKillApplyTeethToSelf,
        OnKillApplyVimToAllyBehind,
        OnKillApplyVimToRandomAlly,
        OnKillApplyVimToRandomEnemy,
        OnKillDraw,
        OnKillHealToSelfAndAlliesInRow,
        OnKillHealToSelf,
        OnKillIncreaseHealthToSelfAndAllies,
        OnTurnApplyAttackToSelf,
        OnTurnApplyDemonizeToRandomEnemy,
        OnTurnApplyInkToEnemies,
        OnTurnApplyInkToRandomEnemy,
        OnTurnApplyJuiceToAllyBehind,
        OnTurnApplyOverloadToRandomEnemy,
        OnTurnApplyScrapToAllyAhead,
        OnTurnApplyScrapToAllyBehind,
        OnTurnApplyScrapToRandomAlly,
        OnTurnApplyScrapToSelf,
        OnTurnApplyShellToAllies,
        OnTurnApplyShellToAllyInFrontOf,
        OnTurnApplyShellToSelf,
        OnTurnApplySnowToEnemies,
        OnTurnApplySpiceToAllies,
        OnTurnApplySpiceToAllyBehind,
        OnTurnApplySpiceToAllyInFrontOf,
        OnTurnApplyTeethToSelf,
        OnTurnApplyVimToAllyBehind,
        OnTurnApplyVimToRandomAlly,
        OnTurnApplyVoidToEveryone,
        OnTurnApplyVoidToRandomEnemy,
        OnTurnEatRandomAllyHealthAndAttackAndEffects,
        OnTurnEscapeToSelf,
        OnTurnHealAllies,
        OngoingIncreaseAttack,
        OngoingIncreaseEffectFactor,
        OngoingIncreaseEffects,
        OngoingReduceAttack,
        Overload,
        PreTriggerGainTempMultiHitEqualToJuice1,
        PreTriggerGainTempMultiHitEqualToScrap1,
        PreTurnDestroyAllItemsInHand,
        PreTurnDestroyRandomCardInHand,
        PreTurnEatAlliesInRowHealthAndAttack,
        PreTurnEatRandomAllyHealthAndAttackAndEffects,
        PreTurnGainAttackForEachItemInHandForEachCardDestroyed,
        PreTurnGainTempMultiHitEqualToJuice,
        PreTurnTakeJuiceFromRandomAlly,
        PreTurnTakeScrapFromRandomAlly,
        Pull,
        Push,
        RecycleJunk,
        RedrawCards,
        ReduceAttackEffect1,
        ReduceAttack,
        ReduceCounter,
        ReduceEffects,
        ReduceMaxCounter,
        ReduceMaxHealthMustbeally,
        ReduceMaxHealth,
        ResistShroom,
        ResistSnow,
        ResistSpice,
        SacrificeAlly,
        SacrificeCardInHand,
        SacrificeEnemy,
        Scrap,
        SetHealth,
        SetMaxHealth,
        Shell,
        Shroom,
        Snow,
        SoulboundBossPhase2,
        Spice,
        Split,
        SplitBossPhase2,
        Stealth,
        SummonBeepop,
        SummonBloo,
        SummonBoBo,
        SummonBonzo,
        SummonDregg,
        SummonEnemyLeech,
        SummonEnemyPigeon,
        SummonEnemyPopper,
        SummonFallow,
        SummonGearhammer,
        SummonItem,
        SummonJunk,
        SummonPlep,
        SummonSunShard,
        SummonTailsFive,
        SummonTailsFour,
        SummonTailsOne,
        SummonTailsThree,
        SummonTailsTwo,
        SummonTigris,
        SummonUzu,
        Summoned,
        Take100DamageWhenSoulboundUnitKilled,
        TakeHealth,
        Teeth,
        TemporaryAimless,
        TemporaryBarrage,
        TemporaryFury,
        TemporaryNoomlin,
        TemporaryPigheaded,
        TemporarySoulbound,
        TemporarySummoned,
        TemporaryUnbreakable,
        TemporaryUnmovable,
        TriggerAgainstAndReduceUses,
        TriggerAgainstDontCountAsTrigger,
        TriggerAgainstAllyWhenAllyIsHit,
        TriggerAgainstAttackerWhenHit,
        TriggerAgainstCrownAlliesWhenDiscarded,
        TriggerAgainstCrownAlliesWhenDrawn,
        TriggerAgainstRandomAllyWhenDiscarded,
        TriggerAgainstRandomAllyWhenDrawn,
        TriggerAgainstRandomEnemy,
        TriggerAgainstRandomUnitWhenDiscarded,
        TriggerAgainstRandomUnitWhenDrawn,
        TriggerAgainstWhenAllyAttacks,
        TriggerAgainstWhenFrostApplied,
        TriggerAgainstWhenSnowApplied,
        TriggerAgainstWhenWeaknessApplied,
        TriggerAgainst,
        TriggerWhenAllyAttacks,
        TriggerWhenAllyInRowAttacks,
        TriggerWhenAllyIsHit,
        TriggerWhenDeployed,
        TriggerWhenEnemyIsKilled,
        TriggerWhenJunkDestroyed,
        TriggerWhenRedrawHit,
        Trigger,
        Unmovable,
        Weakness,
        WhenAllyIsHealedApplyEqualSpice,
        WhenAllyIsHealedTriggerToSelf,
        WhenAllyisHitApplyFrostToAttacker,
        WhenAllyisHitApplyShroomToAttacker,
        WhenAllyisHitApplyTeethToSelf,
        WhenAllyisHitApplyVimToTarget,
        WhenAllyisHitHealToTarget,
        WhenAllyisHitIncreaseHealthToSelf,
        WhenAllyIsKilledApplyAttackToSelf,
        WhenAllyIsKilledGainTheirAttack,
        WhenAllyIsKilledLoseHalfHealthAndGainAttack,
        WhenAllyIsKilledTriggerToSelf,
        WhenAllyIsSacrificedGainTheirAttack,
        WhenAllyIsSacrificedTriggerToSelf,
        WhenAnyoneTakesShroomDamageApplyAttackToSelf,
        WhenBuiltAddJunkToHand,
        WhenBuiltApplyVimToSelf,
        WhenCardDestroyedAndGainAttack,
        WhenCardDestroyedAndGainJuice,
        WhenCardDestroyedAndReduceCounterToSelf,
        WhenConsumedAddHealthToAllies,
        WhenConsumedApplyOverloadToEnemies,
        WhenDeployedAddJunkToHand,
        WhenDeployedApplyBlockToSelf,
        WhenDeployedApplyFrenzyToSelf,
        WhenDeployedApplyInkToAllies,
        WhenDeployedApplyInkToEnemiesInRow,
        WhenDeployedCopyEffectsOfRandomEnemy,
        WhenDeployedFillBoardFinalBoss,
        WhenDeployedSummonWowee,
        WhenDestroyedApplyDamageToAlliesInRow,
        WhenDestroyedApplyDamageToAttacker,
        WhenDestroyedApplyDamageToEnemiesEqualToJuice,
        WhenDestroyedApplyDamageToEnemiesInRow,
        WhenDestroyedApplyFrenzyToRandomAlly,
        WhenDestroyedApplyHazeToAttacker,
        WhenDestroyedApplyOverloadToAttacker,
        WhenDestroyedApplySpiceToAllies,
        WhenDestroyedApplyStealthToAlliesInRow,
        WhenDestroyedSummonDregg,
        WhenDestroyedTriggerToAllies,
        WhenDrawnApplySnowToAllies,
        WhenEnemiesAttackApplyDemonizeToAttacker,
        WhenEnemyShroomedIsKilledApplyTheirShroomToRandomEnemy,
        WhenEnemyDeployedCopyEffectsOfTarget,
        WhenEnemyIsKilledApplyGoldToSelf,
        WhenEnemyIsKilledApplyShellToAttacker,
        WhenHealedApplyAttackToSelf,
        WhenHealedTriggerToSelf,
        WhenHealthLostApplyEqualAttackToSelfAndAllies,
        WhenHealthLostApplyEqualFrostToSelf,
        WhenHealthLostApplyEqualSpiceToSelf,
        WhenHitAddFrenzyToSelf,
        WhenHitAddGearhammerToHand,
        WhenHitAddHealthLostToAttacker,
        WhenHitAddHealthLostToRandomAlly,
        WhenHitAddJunkToHand,
        WhenHitApplyBlockToRandomAlly,
        WhenHitApplyDemonizeToAttacker,
        WhenHitApplyFrostToEnemies,
        WhenHitApplyFrostToRandomEnemy,
        WhenHitApplyGoldToAttackerNoPing,
        WhenHitApplyInkToAttacker,
        WhenHitApplyInkToRandomEnemy,
        WhenHitApplyInkToSelf,
        WhenHitApplyOverloadToAttacker,
        WhenHitApplyShellToAllies,
        WhenHitApplyShellToAllyBehind,
        WhenHitApplyShellToSelf,
        WhenHitApplyShroomToAttacker,
        WhenHitApplyShroomToRandomEnemy,
        WhenHitApplySnowToAttacker,
        WhenHitApplySnowToEnemies,
        WhenHitApplySnowToRandomEnemy,
        WhenHitApplySpiceToAlliesAndEnemiesAndSelf,
        WhenHitApplySpiceToAllies,
        WhenHitApplySpiceToAlliesInRow,
        WhenHitApplySpiceToSelf,
        WhenHitApplyStealthToSelf,
        WhenHitApplyVimToSelf,
        WhenHitApplyVoidToAttacker,
        WhenHitApplyWeaknessToAttacker,
        WhenHitDamageToEnemies,
        WhenHitDamageToEnemiesInRow,
        WhenHitDraw,
        WhenHitEqualDamageToAttacker,
        WhenHitGainAttackToSelfNoPing,
        WhenHitGainTeethToSelf,
        WhenHitIncreaseAttackEffect1ToSelf,
        WhenHitIncreaseAttackToRandomAlly,
        WhenHitIncreaseHealthToRandomAlly,
        WhenHitReduceAttackToAttacker,
        WhenHitReduceAttackToSelf,
        WhenHitReduceCounterToSelf,
        WhenHitTriggerToSelf,
        WhenHitWithJunkAddFrenzyToSelf,
        WhenJuiceAppliedToSelfGainFrenzy,
        WhenSacrificedSummonTailsFour,
        WhenSacrificedSummonTailsOne,
        WhenSacrificedSummonTailsThree,
        WhenSacrificedSummonTailsTwo,
        WhenShellAppliedToSelfGainSpiceInstead,
        WhenShroomAppliedToAnythingDoubleAmountAndLoseScrap,
        WhenShroomDamageTakenTriggerToSelf,
        WhenSnowAppliedToAnythingGainAttackToSelf,
        WhenSnowAppliedToAnythingGainEqualAttackToSelf,
        WhenSnowAppliedToSelfApplyDemonizeToEnemies,
        WhenSnowAppliedToSelfGainEqualAttack,
        WhenSpiceXAppliedToSelfTriggerToSelf,
        WhenVimAppliedToAnythingDoubleAmount,
        WhenXHealthLostSplit,
        WhileActiveAddEqualAttackToJunkInHand,
        WhileActiveAimlessToEnemies,
        WhileActiveBarrageToAllies,
        WhileActiveBarrageToAlliesInRow,
        WhileActiveBarrageToEnemies,
        WhileActiveFrenzyToAllies,
        WhileActiveFrenzyToCrownAllies,
        WhileActiveHaltSpiceToAllies,
        WhileActiveIncreaseAllSpiceApplied,
        WhileActiveIncreaseAttackbyCurrentToAllies,
        WhileActiveIncreaseAttackbyCurrentToSummonedAllies,
        WhileActiveIncreaseAttackToAlliesAndEnemies,
        WhileActiveIncreaseAttackToAlliesNoDesc,
        WhileActiveIncreaseAttackToAllies,
        WhileActiveIncreaseAttackToAlliesInRow,
        WhileActiveIncreaseAttackToItemsInHand,
        WhileActiveIncreaseAttackToJunkInHand,
        WhileActiveIncreaseEffectsToAlliesAndEnemies,
        WhileActiveIncreaseEffectsToFrontAlly,
        WhileActiveIncreaseEffectsToHand,
        WhileActivePigheadedToEnemies,
        WhileActiveReduceAttackToEnemiesNoPingAndNoDesc,
        WhileActiveSnowImmuneToAllies,
        WhileActiveTeethToAllies,
        WhileActiveUnmovableToEnemies,
        WhileInHandReduceAttackToAllies,
        WhileLastInHandDoubleEffectsToSelf
    }

    internal static readonly Dictionary<VanillaStatusEffects, string>
        VanillaStatusEffectsNamesLookUp =
            new()
            {
                [VanillaStatusEffects.AddAttackAndHealthToSummon] = "Add Attack & Health To Summon",
                [VanillaStatusEffects.Block] = "Block",
                [VanillaStatusEffects.Bombard1] = "Bombard 1",
                [VanillaStatusEffects.Bombard2] = "Bombard 2",
                [VanillaStatusEffects.BonusDamageEqualToDartsInHand] = "Bonus Damage Equal To Darts In Hand",
                [VanillaStatusEffects.BonusDamageEqualToGoldFactor002] = "Bonus Damage Equal To Gold Factor 0.02",
                [VanillaStatusEffects.BonusDamageEqualToJuice] = "Bonus Damage Equal To Juice",
                [VanillaStatusEffects.BonusDamageEqualToScrapOnBoard] = "Bonus Damage Equal To Scrap On Board",
                [VanillaStatusEffects.BonusDamageEqualToScrap] = "Bonus Damage Equal To Scrap",
                [VanillaStatusEffects.BonusDamageEqualToShell] = "Bonus Damage Equal To Shell",
                [VanillaStatusEffects.BoostEffects] = "Boost Effects",
                [VanillaStatusEffects.Budge] = "Budge",
                [VanillaStatusEffects.CannotRecall] = "Cannot Recall",
                [VanillaStatusEffects.CheckHasJuice] = "Check Has Juice",
                [VanillaStatusEffects.Cleanse] = "Cleanse",
                [VanillaStatusEffects.CombineWhen2Deployed] = "Combine When 2 Deployed",
                [VanillaStatusEffects.CopyEffects] = "Copy Effects",
                [VanillaStatusEffects.Crush] = "Crush",
                [VanillaStatusEffects.DamageEqualToHealth] = "Damage Equal To Health",
                [VanillaStatusEffects.DamageToFrontAllies] = "Damage To Front Allies",
                [VanillaStatusEffects.Demonize] = "Demonize",
                [VanillaStatusEffects.DestroyAfterUse] = "Destroy After Use",
                [VanillaStatusEffects.DestroySelfAfterTurn] = "Destroy Self After Turn",
                [VanillaStatusEffects.DoubleAllSpiceWhenDestroyed] = "Double All Spice When Destroyed",
                [VanillaStatusEffects.DoubleBlock] = "Double Block",
                [VanillaStatusEffects.DoubleInk] = "Double Ink",
                [VanillaStatusEffects.DoubleJuice] = "Double Juice",
                [VanillaStatusEffects.DoubleNegativeEffects] = "Double Negative Effects",
                [VanillaStatusEffects.DoubleOverload] = "Double Overload",
                [VanillaStatusEffects.DoubleShell] = "Double Shell",
                [VanillaStatusEffects.DoubleShroom] = "Double Shroom",
                [VanillaStatusEffects.DoubleSpice] = "Double Spice",
                [VanillaStatusEffects.DoubleVim] = "Double Vim",
                [VanillaStatusEffects.DrawCards] = "Draw Cards",
                [VanillaStatusEffects.EatHealthAndAttack] = "Eat (Health & Attack)",
                [VanillaStatusEffects.EatHealthAndAttackAndEffects] = "Eat (Health, Attack & Effects)",
                [VanillaStatusEffects.EatAlliesInRowHealthAndAttack] = "Eat Allies In Row (Health & Attack)",
                [VanillaStatusEffects.EatRandomAllyHealthAndAttackAndEffects] =
                    "Eat Random Ally (Health, Attack & Effects)",
                [VanillaStatusEffects.Escape] = "Escape",
                [VanillaStatusEffects.FillBoardFinalBoss] = "Fill Board (Final Boss)",
                [VanillaStatusEffects.FinalBossPhase2] = "FinalBossPhase2",
                [VanillaStatusEffects.Flee] = "Flee",
                [VanillaStatusEffects.FreeAction] = "Free Action",
                [VanillaStatusEffects.FrenzyBossPhase2] = "FrenzyBossPhase2",
                [VanillaStatusEffects.Frost] = "Frost",
                [VanillaStatusEffects.GainFrenzyWhenWildUnitKilled] = "Gain Frenzy When Wild Unit Killed",
                [VanillaStatusEffects.GainGoldRange36] = "Gain Gold Range (3-6)",
                [VanillaStatusEffects.GainGold] = "Gain Gold",
                [VanillaStatusEffects.GoatWampusPhase2] = "GoatWampusPhase2",
                [VanillaStatusEffects.HaltSpice] = "Halt Spice",
                [VanillaStatusEffects.Haze] = "Haze",
                [VanillaStatusEffects.HealNoPing] = "Heal (No Ping)",
                [VanillaStatusEffects.HealFrontAllyBasedOnDamageDealt] = "Heal Front Ally Based On Damage Dealt",
                [VanillaStatusEffects.HealFullAndGainEqualSpice] = "Heal Full, Gain Equal Spice",
                [VanillaStatusEffects.HealToFrontAllies] = "Heal To Front Allies",
                [VanillaStatusEffects.Heal] = "Heal",
                [VanillaStatusEffects.HighPriorityPosition] = "High Priority Position",
                [VanillaStatusEffects.HitAllCrownEnemies] = "Hit All Crown Enemies",
                [VanillaStatusEffects.HitAllEnemiesInRow] = "Hit All Enemies In Row",
                [VanillaStatusEffects.HitAllEnemies] = "Hit All Enemies",
                [VanillaStatusEffects.HitAllUndamagedEnemies] = "Hit All Undamaged Enemies",
                [VanillaStatusEffects.HitFurthestTarget] = "Hit Furthest Target",
                [VanillaStatusEffects.HitRandomTarget] = "Hit Random Target",
                [VanillaStatusEffects.ImmuneToFrost] = "ImmuneToFrost",
                [VanillaStatusEffects.ImmuneToSnow] = "ImmuneToSnow",
                [VanillaStatusEffects.ImmuneToSpice] = "ImmuneToSpice",
                [VanillaStatusEffects.ImmuneToVim] = "ImmuneToVim",
                [VanillaStatusEffects.IncreaseAllSpiceAppliedNoDesc] = "Increase All Spice Applied (No Desc)",
                [VanillaStatusEffects.IncreaseAttackAndHealth] = "Increase Attack & Health",
                [VanillaStatusEffects.IncreaseAttackAndLoseHalfHealth] = "Increase Attack & Lose Half Health",
                [VanillaStatusEffects.IncreaseAttackEffect1] = "Increase Attack Effect 1",
                [VanillaStatusEffects.IncreaseAttackWhileAlone] = "Increase Attack While Alone",
                [VanillaStatusEffects.IncreaseAttackWhileDamaged] = "Increase Attack While Damaged",
                [VanillaStatusEffects.IncreaseAttack] = "Increase Attack",
                [VanillaStatusEffects.IncreaseEffects] = "Increase Effects",
                [VanillaStatusEffects.IncreaseMaxCounter] = "Increase Max Counter",
                [VanillaStatusEffects.IncreaseMaxHealth] = "Increase Max Health",
                [VanillaStatusEffects.Injury] = "Injury",
                [VanillaStatusEffects.InstantAddScrap] = "Instant Add Scrap",
                [VanillaStatusEffects.InstantApplyAttackToApplier] = "Instant Apply Attack To Applier",
                [VanillaStatusEffects.InstantApplyCurrentAttackToAllies] = "Instant Apply Current Attack To Allies",
                [VanillaStatusEffects.InstantApplyCurrentAttackToRandomAlly] =
                    "Instant Apply Current Attack To Random Ally",
                [VanillaStatusEffects.InstantApplyFrenzyToItemInHand] = "Instant Apply Frenzy (To Item In Hand)",
                [VanillaStatusEffects.InstantDraw] = "Instant Draw",
                [VanillaStatusEffects.InstantGainAimless] = "Instant Gain Aimless",
                [VanillaStatusEffects.InstantGainFury] = "Instant Gain Fury",
                [VanillaStatusEffects.InstantGainNoomlinToCardInHand] = "Instant Gain Noomlin (To Card In Hand)",
                [VanillaStatusEffects.InstantGainSoulboundToEnemy] = "Instant Gain Soulbound (To Enemy)",
                [VanillaStatusEffects.InstantSummonBloo] = "Instant Summon Bloo",
                [VanillaStatusEffects.InstantSummonCopyOfItem] = "Instant Summon Copy Of Item",
                [VanillaStatusEffects.InstantSummonCopyOnOtherSideWithXHealth] =
                    "Instant Summon Copy On Other Side With X Health",
                [VanillaStatusEffects.InstantSummonCopy] = "Instant Summon Copy",
                [VanillaStatusEffects.InstantSummonDregg] = "Instant Summon Dregg",
                [VanillaStatusEffects.InstantSummonFallow] = "Instant Summon Fallow",
                [VanillaStatusEffects.InstantSummonGearhammerInHand] = "Instant Summon Gearhammer In Hand",
                [VanillaStatusEffects.InstantSummonJunkInHand] = "Instant Summon Junk In Hand",
                [VanillaStatusEffects.InstantSummonSunShardInHand] = "Instant Summon SunShard In Hand",
                [VanillaStatusEffects.InstantSummonTailsFour] = "Instant Summon TailsFour",
                [VanillaStatusEffects.InstantSummonTailsOne] = "Instant Summon TailsOne",
                [VanillaStatusEffects.InstantSummonTailsThree] = "Instant Summon TailsThree",
                [VanillaStatusEffects.InstantSummonTailsTwo] = "Instant Summon TailsTwo",
                [VanillaStatusEffects.Kill] = "Kill",
                [VanillaStatusEffects.LastStand] = "Last Stand",
                [VanillaStatusEffects.LoseHalfHealth] = "Lose Half Health",
                [VanillaStatusEffects.LoseJuice] = "Lose Juice",
                [VanillaStatusEffects.LoseScrap] = "Lose Scrap",
                [VanillaStatusEffects.LowPriorityPosition] = "Low Priority Position",
                [VanillaStatusEffects.Lumin] = "Lumin",
                [VanillaStatusEffects.MultiHitTemporaryAndNotVisible] = "MultiHit (Temporary, Not Visible)",
                [VanillaStatusEffects.MultiHit] = "MultiHit",
                [VanillaStatusEffects.MustHaveJuiceToTrigger] = "Must Have Juice To Trigger",
                [VanillaStatusEffects.Null] = "Null",
                [VanillaStatusEffects.OnCardPlayedAddFuryToTarget] = "On Card Played Add Fury To Target",
                [VanillaStatusEffects.OnCardPlayedAddGearhammerToHand] = "On Card Played Add Gearhammer To Hand",
                [VanillaStatusEffects.OnCardPlayedAddJunkToHand] = "On Card Played Add Junk To Hand",
                [VanillaStatusEffects.OnCardPlayedAddScrapToRandomAlly] = "On Card Played Add Scrap To RandomAlly",
                [VanillaStatusEffects.OnCardPlayedAddSoulboundToRandomAlly] =
                    "On Card Played Add Soulbound To RandomAlly",
                [VanillaStatusEffects.OnCardPlayedAddSunShardToHand] = "On Card Played Add SunShard To Hand",
                [VanillaStatusEffects.OnCardPlayedApplyAttackToSelf] = "On Card Played Apply Attack To Self",
                [VanillaStatusEffects.OnCardPlayedApplyBlockToRandomUnit] =
                    "On Card Played Apply Block To RandomUnit",
                [VanillaStatusEffects.OnCardPlayedApplyFrostToRandomEnemy] =
                    "On Card Played Apply Frost To RandomEnemy",
                [VanillaStatusEffects.OnCardPlayedApplyHazeToRandomEnemy] =
                    "On Card Played Apply Haze To RandomEnemy",
                [VanillaStatusEffects.OnCardPlayedApplyInkToRandomEnemy] = "On Card Played Apply Ink To RandomEnemy",
                [VanillaStatusEffects.OnCardPlayedApplyOverloadToFrontEnemy] =
                    "On Card Played Apply Overload To FrontEnemy",
                [VanillaStatusEffects.OnCardPlayedApplyShellToRandomAlly] =
                    "On Card Played Apply Shell To RandomAlly",
                [VanillaStatusEffects.OnCardPlayedApplyShroomToEnemies] = "On Card Played Apply Shroom To Enemies",
                [VanillaStatusEffects.OnCardPlayedApplySnowToEnemiesInRow] =
                    "On Card Played Apply Snow To EnemiesInRow",
                [VanillaStatusEffects.OnCardPlayedApplySpiceToRandomAlly] =
                    "On Card Played Apply Spice To RandomAlly",
                [VanillaStatusEffects.OnCardPlayedApplyTeethToRandomAlly] =
                    "On Card Played Apply Teeth To RandomAlly",
                [VanillaStatusEffects.OnCardPlayedBoostToRandomAlly] = "On Card Played Boost To RandomAlly",
                [VanillaStatusEffects.OnCardPlayedBoostToRandomEnemy] = "On Card Played Boost To RandomEnemy",
                [VanillaStatusEffects.OnCardPlayedDamageFrostedEnemies] = "On Card Played Damage Frosted Enemies",
                [VanillaStatusEffects.OnCardPlayedDamageInkedEnemies] = "On Card Played Damage Inked Enemies",
                [VanillaStatusEffects.OnCardPlayedDamageToSelfAndAlliesInRow] =
                    "On Card Played Damage To Self & AlliesInRow",
                [VanillaStatusEffects.OnCardPlayedDamageUndamagedEnemies] = "On Card Played Damage Undamaged Enemies",
                [VanillaStatusEffects.OnCardPlayedDestroyRandomCardInHand] =
                    "On Card Played Destroy Random Card In Hand",
                [VanillaStatusEffects.OnCardPlayedDestroyRandomJunkInHand] =
                    "On Card Played Destroy Random Junk In Hand",
                [VanillaStatusEffects.OnCardPlayedDestroyRightmostCardInHand] =
                    "On Card Played Destroy Rightmost Card In Hand",
                [VanillaStatusEffects.OnCardPlayedDoubleVimToSelf] = "On Card Played Double Vim To Self",
                [VanillaStatusEffects.OnCardPlayedLose1JuiceToSelfNoDesc] =
                    "On Card Played Lose 1 Juice To Self (No Desc)",
                [VanillaStatusEffects.OnCardPlayedLoseScrapToSelf] = "On Card Played Lose Scrap To Self",
                [VanillaStatusEffects.OnCardPlayedReduceAttackEffect1ToSelf] =
                    "On Card Played Reduce Attack Effect 1 To Self",
                [VanillaStatusEffects.OnCardPlayedReduceCounterToAllies] = "On Card Played Reduce Counter To Allies",
                [VanillaStatusEffects.OnCardPlayedSacrificeRandomAlly] = "On Card Played Sacrifice RandomAlly",
                [VanillaStatusEffects.OnCardPlayedTakeHealthFromAllies] = "On Card Played Take Health From Allies",
                [VanillaStatusEffects.OnCardPlayedTriggerAgainstAllyBehind] =
                    "On Card Played Trigger Against AllyBehind",
                [VanillaStatusEffects.OnCardPlayedTriggerRandomAlly] = "On Card Played Trigger RandomAlly",
                [VanillaStatusEffects.OnCardPlayedUseRandomItemInHandAgainstRandomEnemy] =
                    "On Card Played Use Random Item In Hand Against Random Enemy",
                [VanillaStatusEffects.OnCardPlayedVimToAllies] = "On Card Played Vim To Allies",
                [VanillaStatusEffects.OnCardPlayedVimToSelf] = "On Card Played Vim To Self",
                [VanillaStatusEffects.OnCardPlayedVoidToEnemies] = "On Card Played Void To Enemies",
                [VanillaStatusEffects.OnHitDamageDamagedTarget] = "On Hit Damage Damaged Target",
                [VanillaStatusEffects.OnHitDamageFrostedTarget] = "On Hit Damage Frosted Target",
                [VanillaStatusEffects.OnHitDamageShelledTarget] = "On Hit Damage Shelled Target",
                [VanillaStatusEffects.OnHitDamageSnowedTarget] = "On Hit Damage Snowed Target",
                [VanillaStatusEffects.OnHitEqualDamageToInkedTarget] = "On Hit Equal Damage To Inked Target",
                [VanillaStatusEffects.OnHitEqualHealToFrontAlly] = "On Hit Equal Heal To FrontAlly",
                [VanillaStatusEffects.OnHitEqualOverloadToTarget] = "On Hit Equal Overload To Target",
                [VanillaStatusEffects.OnHitEqualSnowToTarget] = "On Hit Equal Snow To Target",
                [VanillaStatusEffects.OnHitPullTarget] = "On Hit Pull Target",
                [VanillaStatusEffects.OnHitPushTarget] = "On Hit Push Target",
                [VanillaStatusEffects.OnKillApplyAttackToSelf] = "On Kill Apply Attack To Self",
                [VanillaStatusEffects.OnKillApplyBlockToSelf] = "On Kill Apply Block To Self",
                [VanillaStatusEffects.OnKillApplyGoldToSelf] = "On Kill Apply Gold To Self",
                [VanillaStatusEffects.OnKillApplyScrapToAllies] = "On Kill Apply Scrap To Allies",
                [VanillaStatusEffects.OnKillApplyScrapToAllyBehind] = "On Kill Apply Scrap To AllyBehind",
                [VanillaStatusEffects.OnKillApplyScrapToRandomAlly] = "On Kill Apply Scrap To RandomAlly",
                [VanillaStatusEffects.OnKillApplyShellToSelf] = "On Kill Apply Shell To Self",
                [VanillaStatusEffects.OnKillApplyStealthToSelf] = "On Kill Apply Stealth To Self",
                [VanillaStatusEffects.OnKillApplyTeethToSelf] = "On Kill Apply Teeth To Self",
                [VanillaStatusEffects.OnKillApplyVimToAllyBehind] = "On Kill Apply Vim To AllyBehind",
                [VanillaStatusEffects.OnKillApplyVimToRandomAlly] = "On Kill Apply Vim To RandomAlly",
                [VanillaStatusEffects.OnKillApplyVimToRandomEnemy] = "On Kill Apply Vim To RandomEnemy",
                [VanillaStatusEffects.OnKillDraw] = "On Kill Draw",
                [VanillaStatusEffects.OnKillHealToSelfAndAlliesInRow] = "On Kill Heal To Self & AlliesInRow",
                [VanillaStatusEffects.OnKillHealToSelf] = "On Kill Heal To Self",
                [VanillaStatusEffects.OnKillIncreaseHealthToSelfAndAllies] =
                    "On Kill Increase Health To Self & Allies",
                [VanillaStatusEffects.OnTurnApplyAttackToSelf] = "On Turn Apply Attack To Self",
                [VanillaStatusEffects.OnTurnApplyDemonizeToRandomEnemy] = "On Turn Apply Demonize To RandomEnemy",
                [VanillaStatusEffects.OnTurnApplyInkToEnemies] = "On Turn Apply Ink To Enemies",
                [VanillaStatusEffects.OnTurnApplyInkToRandomEnemy] = "On Turn Apply Ink To RandomEnemy",
                [VanillaStatusEffects.OnTurnApplyJuiceToAllyBehind] = "On Turn Apply Juice To AllyBehind",
                [VanillaStatusEffects.OnTurnApplyOverloadToRandomEnemy] = "On Turn Apply Overload To RandomEnemy",
                [VanillaStatusEffects.OnTurnApplyScrapToAllyAhead] = "On Turn Apply Scrap To AllyAhead",
                [VanillaStatusEffects.OnTurnApplyScrapToAllyBehind] = "On Turn Apply Scrap To AllyBehind",
                [VanillaStatusEffects.OnTurnApplyScrapToRandomAlly] = "On Turn Apply Scrap To RandomAlly",
                [VanillaStatusEffects.OnTurnApplyScrapToSelf] = "On Turn Apply Scrap To Self",
                [VanillaStatusEffects.OnTurnApplyShellToAllies] = "On Turn Apply Shell To Allies",
                [VanillaStatusEffects.OnTurnApplyShellToAllyInFrontOf] = "On Turn Apply Shell To AllyInFrontOf",
                [VanillaStatusEffects.OnTurnApplyShellToSelf] = "On Turn Apply Shell To Self",
                [VanillaStatusEffects.OnTurnApplySnowToEnemies] = "On Turn Apply Snow To Enemies",
                [VanillaStatusEffects.OnTurnApplySpiceToAllies] = "On Turn Apply Spice To Allies",
                [VanillaStatusEffects.OnTurnApplySpiceToAllyBehind] = "On Turn Apply Spice To AllyBehind",
                [VanillaStatusEffects.OnTurnApplySpiceToAllyInFrontOf] = "On Turn Apply Spice To AllyInFrontOf",
                [VanillaStatusEffects.OnTurnApplyTeethToSelf] = "On Turn Apply Teeth To Self",
                [VanillaStatusEffects.OnTurnApplyVimToAllyBehind] = "On Turn Apply Vim To AllyBehind",
                [VanillaStatusEffects.OnTurnApplyVimToRandomAlly] = "On Turn Apply Vim To RandomAlly",
                [VanillaStatusEffects.OnTurnApplyVoidToEveryone] = "On Turn Apply Void To Everyone",
                [VanillaStatusEffects.OnTurnApplyVoidToRandomEnemy] = "On Turn Apply Void To RandomEnemy",
                [VanillaStatusEffects.OnTurnEatRandomAllyHealthAndAttackAndEffects] =
                    "On Turn Eat Random Ally (Health, Attack & Effects)",
                [VanillaStatusEffects.OnTurnEscapeToSelf] = "On Turn Escape To Self",
                [VanillaStatusEffects.OnTurnHealAllies] = "On Turn Heal Allies",
                [VanillaStatusEffects.OngoingIncreaseAttack] = "Ongoing Increase Attack",
                [VanillaStatusEffects.OngoingIncreaseEffectFactor] = "Ongoing Increase Effect Factor",
                [VanillaStatusEffects.OngoingIncreaseEffects] = "Ongoing Increase Effects",
                [VanillaStatusEffects.OngoingReduceAttack] = "Ongoing Reduce Attack",
                [VanillaStatusEffects.Overload] = "Overload",
                [VanillaStatusEffects.PreTriggerGainTempMultiHitEqualToJuice1] =
                    "Pre Trigger Gain Temp MultiHit Equal To Juice - 1",
                [VanillaStatusEffects.PreTriggerGainTempMultiHitEqualToScrap1] =
                    "Pre Trigger Gain Temp MultiHit Equal To Scrap - 1",
                [VanillaStatusEffects.PreTurnDestroyAllItemsInHand] = "Pre Turn Destroy All Items In Hand",
                [VanillaStatusEffects.PreTurnDestroyRandomCardInHand] = "Pre Turn Destroy Random Card In Hand",
                [VanillaStatusEffects.PreTurnEatAlliesInRowHealthAndAttack] =
                    "Pre Turn Eat Allies In Row (Health & Attack)",
                [VanillaStatusEffects.PreTurnEatRandomAllyHealthAndAttackAndEffects] =
                    "Pre Turn Eat Random Ally (Health, Attack & Effects)",
                [VanillaStatusEffects.PreTurnGainAttackForEachItemInHandForEachCardDestroyed] =
                    "Pre Turn Gain Attack For Each Item In Hand (For Each Card Destroyed)",
                [VanillaStatusEffects.PreTurnGainTempMultiHitEqualToJuice] =
                    "Pre Turn Gain Temp MultiHit Equal To Juice",
                [VanillaStatusEffects.PreTurnTakeJuiceFromRandomAlly] = "Pre Turn Take Juice From RandomAlly",
                [VanillaStatusEffects.PreTurnTakeScrapFromRandomAlly] = "Pre Turn Take Scrap From RandomAlly",
                [VanillaStatusEffects.Pull] = "Pull",
                [VanillaStatusEffects.Push] = "Push",
                [VanillaStatusEffects.RecycleJunk] = "Recycle Junk",
                [VanillaStatusEffects.RedrawCards] = "Redraw Cards",
                [VanillaStatusEffects.ReduceAttackEffect1] = "Reduce Attack Effect 1",
                [VanillaStatusEffects.ReduceAttack] = "Reduce Attack",
                [VanillaStatusEffects.ReduceCounter] = "Reduce Counter",
                [VanillaStatusEffects.ReduceEffects] = "Reduce Effects",
                [VanillaStatusEffects.ReduceMaxCounter] = "Reduce Max Counter",
                [VanillaStatusEffects.ReduceMaxHealthMustbeally] = "Reduce Max Health (Must be ally)",
                [VanillaStatusEffects.ReduceMaxHealth] = "Reduce Max Health",
                [VanillaStatusEffects.ResistShroom] = "ResistShroom",
                [VanillaStatusEffects.ResistSnow] = "ResistSnow",
                [VanillaStatusEffects.ResistSpice] = "ResistSpice",
                [VanillaStatusEffects.SacrificeAlly] = "Sacrifice Ally",
                [VanillaStatusEffects.SacrificeCardInHand] = "Sacrifice Card In Hand",
                [VanillaStatusEffects.SacrificeEnemy] = "Sacrifice Enemy",
                [VanillaStatusEffects.Scrap] = "Scrap",
                [VanillaStatusEffects.SetHealth] = "Set Health",
                [VanillaStatusEffects.SetMaxHealth] = "Set Max Health",
                [VanillaStatusEffects.Shell] = "Shell",
                [VanillaStatusEffects.Shroom] = "Shroom",
                [VanillaStatusEffects.Snow] = "Snow",
                [VanillaStatusEffects.SoulboundBossPhase2] = "SoulboundBossPhase2",
                [VanillaStatusEffects.Spice] = "Spice",
                [VanillaStatusEffects.Split] = "Split",
                [VanillaStatusEffects.SplitBossPhase2] = "SplitBossPhase2",
                [VanillaStatusEffects.Stealth] = "Stealth",
                [VanillaStatusEffects.SummonBeepop] = "Summon Beepop",
                [VanillaStatusEffects.SummonBloo] = "Summon Bloo",
                [VanillaStatusEffects.SummonBoBo] = "Summon BoBo",
                [VanillaStatusEffects.SummonBonzo] = "Summon Bonzo",
                [VanillaStatusEffects.SummonDregg] = "Summon Dregg",
                [VanillaStatusEffects.SummonEnemyLeech] = "Summon Enemy Leech",
                [VanillaStatusEffects.SummonEnemyPigeon] = "Summon Enemy Pigeon",
                [VanillaStatusEffects.SummonEnemyPopper] = "Summon Enemy Popper",
                [VanillaStatusEffects.SummonFallow] = "Summon Fallow",
                [VanillaStatusEffects.SummonGearhammer] = "Summon Gearhammer",
                [VanillaStatusEffects.SummonItem] = "Summon Item",
                [VanillaStatusEffects.SummonJunk] = "Summon Junk",
                [VanillaStatusEffects.SummonPlep] = "Summon Plep",
                [VanillaStatusEffects.SummonSunShard] = "Summon SunShard",
                [VanillaStatusEffects.SummonTailsFive] = "Summon TailsFive",
                [VanillaStatusEffects.SummonTailsFour] = "Summon TailsFour",
                [VanillaStatusEffects.SummonTailsOne] = "Summon TailsOne",
                [VanillaStatusEffects.SummonTailsThree] = "Summon TailsThree",
                [VanillaStatusEffects.SummonTailsTwo] = "Summon TailsTwo",
                [VanillaStatusEffects.SummonTigris] = "Summon Tigris",
                [VanillaStatusEffects.SummonUzu] = "Summon Uzu",
                [VanillaStatusEffects.Summoned] = "Summoned",
                [VanillaStatusEffects.Take100DamageWhenSoulboundUnitKilled] =
                    "Take 100 Damage When Soulbound Unit Killed",
                [VanillaStatusEffects.TakeHealth] = "Take Health",
                [VanillaStatusEffects.Teeth] = "Teeth",
                [VanillaStatusEffects.TemporaryAimless] = "Temporary Aimless",
                [VanillaStatusEffects.TemporaryBarrage] = "Temporary Barrage",
                [VanillaStatusEffects.TemporaryFury] = "Temporary Fury",
                [VanillaStatusEffects.TemporaryNoomlin] = "Temporary Noomlin",
                [VanillaStatusEffects.TemporaryPigheaded] = "Temporary Pigheaded",
                [VanillaStatusEffects.TemporarySoulbound] = "Temporary Soulbound",
                [VanillaStatusEffects.TemporarySummoned] = "Temporary Summoned",
                [VanillaStatusEffects.TemporaryUnbreakable] = "Temporary Unbreakable",
                [VanillaStatusEffects.TemporaryUnmovable] = "Temporary Unmovable",
                [VanillaStatusEffects.TriggerAgainstAndReduceUses] = "Trigger Against & Reduce Uses",
                [VanillaStatusEffects.TriggerAgainstDontCountAsTrigger] = "Trigger Against (Don't Count As Trigger)",
                [VanillaStatusEffects.TriggerAgainstAllyWhenAllyIsHit] = "Trigger Against Ally When Ally Is Hit",
                [VanillaStatusEffects.TriggerAgainstAttackerWhenHit] = "Trigger Against Attacker When Hit",
                [VanillaStatusEffects.TriggerAgainstCrownAlliesWhenDiscarded] =
                    "Trigger Against Crown Allies When Discarded",
                [VanillaStatusEffects.TriggerAgainstCrownAlliesWhenDrawn] = "Trigger Against Crown Allies When Drawn",
                [VanillaStatusEffects.TriggerAgainstRandomAllyWhenDiscarded] =
                    "Trigger Against Random Ally When Discarded",
                [VanillaStatusEffects.TriggerAgainstRandomAllyWhenDrawn] = "Trigger Against Random Ally When Drawn",
                [VanillaStatusEffects.TriggerAgainstRandomEnemy] = "Trigger Against Random Enemy",
                [VanillaStatusEffects.TriggerAgainstRandomUnitWhenDiscarded] =
                    "Trigger Against Random Unit When Discarded",
                [VanillaStatusEffects.TriggerAgainstRandomUnitWhenDrawn] = "Trigger Against Random Unit When Drawn",
                [VanillaStatusEffects.TriggerAgainstWhenAllyAttacks] = "Trigger Against When Ally Attacks",
                [VanillaStatusEffects.TriggerAgainstWhenFrostApplied] = "Trigger Against When Frost Applied",
                [VanillaStatusEffects.TriggerAgainstWhenSnowApplied] = "Trigger Against When Snow Applied",
                [VanillaStatusEffects.TriggerAgainstWhenWeaknessApplied] = "Trigger Against When Weakness Applied",
                [VanillaStatusEffects.TriggerAgainst] = "Trigger Against",
                [VanillaStatusEffects.TriggerWhenAllyAttacks] = "Trigger When Ally Attacks",
                [VanillaStatusEffects.TriggerWhenAllyInRowAttacks] = "Trigger When Ally In Row Attacks",
                [VanillaStatusEffects.TriggerWhenAllyIsHit] = "Trigger When Ally Is Hit",
                [VanillaStatusEffects.TriggerWhenDeployed] = "Trigger When Deployed",
                [VanillaStatusEffects.TriggerWhenEnemyIsKilled] = "Trigger When Enemy Is Killed",
                [VanillaStatusEffects.TriggerWhenJunkDestroyed] = "Trigger When Junk Destroyed",
                [VanillaStatusEffects.TriggerWhenRedrawHit] = "Trigger When Redraw Hit",
                [VanillaStatusEffects.Trigger] = "Trigger",
                [VanillaStatusEffects.Unmovable] = "Unmovable",
                [VanillaStatusEffects.Weakness] = "Weakness",
                [VanillaStatusEffects.WhenAllyIsHealedApplyEqualSpice] = "When Ally Is Healed Apply Equal Spice",
                [VanillaStatusEffects.WhenAllyIsHealedTriggerToSelf] = "When Ally Is Healed Trigger To Self",
                [VanillaStatusEffects.WhenAllyisHitApplyFrostToAttacker] = "When Ally is Hit Apply Frost To Attacker",
                [VanillaStatusEffects.WhenAllyisHitApplyShroomToAttacker] =
                    "When Ally is Hit Apply Shroom To Attacker",
                [VanillaStatusEffects.WhenAllyisHitApplyTeethToSelf] = "When Ally is Hit Apply Teeth To Self",
                [VanillaStatusEffects.WhenAllyisHitApplyVimToTarget] = "When Ally is Hit Apply Vim To Target",
                [VanillaStatusEffects.WhenAllyisHitHealToTarget] = "When Ally is Hit Heal To Target",
                [VanillaStatusEffects.WhenAllyisHitIncreaseHealthToSelf] = "When Ally is Hit Increase Health To Self",
                [VanillaStatusEffects.WhenAllyIsKilledApplyAttackToSelf] = "When Ally Is Killed Apply Attack To Self",
                [VanillaStatusEffects.WhenAllyIsKilledGainTheirAttack] = "When Ally Is Killed Gain Their Attack",
                [VanillaStatusEffects.WhenAllyIsKilledLoseHalfHealthAndGainAttack] =
                    "When Ally Is Killed Lose Half Health & Gain Attack",
                [VanillaStatusEffects.WhenAllyIsKilledTriggerToSelf] = "When Ally Is Killed Trigger To Self",
                [VanillaStatusEffects.WhenAllyIsSacrificedGainTheirAttack] =
                    "When Ally Is Sacrificed Gain Their Attack",
                [VanillaStatusEffects.WhenAllyIsSacrificedTriggerToSelf] = "When Ally Is Sacrificed Trigger To Self",
                [VanillaStatusEffects.WhenAnyoneTakesShroomDamageApplyAttackToSelf] =
                    "When Anyone Takes Shroom Damage Apply Attack To Self",
                [VanillaStatusEffects.WhenBuiltAddJunkToHand] = "When Built Add Junk To Hand",
                [VanillaStatusEffects.WhenBuiltApplyVimToSelf] = "When Built Apply Vim To Self",
                [VanillaStatusEffects.WhenCardDestroyedAndGainAttack] = "When Card Destroyed, Gain Attack",
                [VanillaStatusEffects.WhenCardDestroyedAndGainJuice] = "When Card Destroyed, Gain Juice",
                [VanillaStatusEffects.WhenCardDestroyedAndReduceCounterToSelf] =
                    "When Card Destroyed, Reduce Counter To Self",
                [VanillaStatusEffects.WhenConsumedAddHealthToAllies] = "When Consumed Add Health To Allies",
                [VanillaStatusEffects.WhenConsumedApplyOverloadToEnemies] = "When Consumed Apply Overload To Enemies",
                [VanillaStatusEffects.WhenDeployedAddJunkToHand] = "When Deployed Add Junk To Hand",
                [VanillaStatusEffects.WhenDeployedApplyBlockToSelf] = "When Deployed Apply Block To Self",
                [VanillaStatusEffects.WhenDeployedApplyFrenzyToSelf] = "When Deployed Apply Frenzy To Self",
                [VanillaStatusEffects.WhenDeployedApplyInkToAllies] = "When Deployed Apply Ink To Allies",
                [VanillaStatusEffects.WhenDeployedApplyInkToEnemiesInRow] = "When Deployed Apply Ink To EnemiesInRow",
                [VanillaStatusEffects.WhenDeployedCopyEffectsOfRandomEnemy] =
                    "When Deployed Copy Effects Of RandomEnemy",
                [VanillaStatusEffects.WhenDeployedFillBoardFinalBoss] = "When Deployed Fill Board (Final Boss)",
                [VanillaStatusEffects.WhenDeployedSummonWowee] = "When Deployed Summon Wowee",
                [VanillaStatusEffects.WhenDestroyedApplyDamageToAlliesInRow] =
                    "When Destroyed Apply Damage To AlliesInRow",
                [VanillaStatusEffects.WhenDestroyedApplyDamageToAttacker] = "When Destroyed Apply Damage To Attacker",
                [VanillaStatusEffects.WhenDestroyedApplyDamageToEnemiesEqualToJuice] =
                    "When Destroyed Apply Damage To Enemies Equal To Juice",
                [VanillaStatusEffects.WhenDestroyedApplyDamageToEnemiesInRow] =
                    "When Destroyed Apply Damage To EnemiesInRow",
                [VanillaStatusEffects.WhenDestroyedApplyFrenzyToRandomAlly] =
                    "When Destroyed Apply Frenzy To RandomAlly",
                [VanillaStatusEffects.WhenDestroyedApplyHazeToAttacker] = "When Destroyed Apply Haze To Attacker",
                [VanillaStatusEffects.WhenDestroyedApplyOverloadToAttacker] =
                    "When Destroyed Apply Overload To Attacker",
                [VanillaStatusEffects.WhenDestroyedApplySpiceToAllies] = "When Destroyed Apply Spice To Allies",
                [VanillaStatusEffects.WhenDestroyedApplyStealthToAlliesInRow] =
                    "When Destroyed Apply Stealth To AlliesInRow",
                [VanillaStatusEffects.WhenDestroyedSummonDregg] = "When Destroyed Summon Dregg",
                [VanillaStatusEffects.WhenDestroyedTriggerToAllies] = "When Destroyed Trigger To Allies",
                [VanillaStatusEffects.WhenDrawnApplySnowToAllies] = "When Drawn Apply Snow To Allies",
                [VanillaStatusEffects.WhenEnemiesAttackApplyDemonizeToAttacker] =
                    "When Enemies Attack Apply Demonize To Attacker",
                [VanillaStatusEffects.WhenEnemyShroomedIsKilledApplyTheirShroomToRandomEnemy] =
                    "When Enemy (Shroomed) Is Killed Apply Their Shroom To RandomEnemy",
                [VanillaStatusEffects.WhenEnemyDeployedCopyEffectsOfTarget] =
                    "When Enemy Deployed Copy Effects Of Target",
                [VanillaStatusEffects.WhenEnemyIsKilledApplyGoldToSelf] = "When Enemy Is Killed Apply Gold To Self",
                [VanillaStatusEffects.WhenEnemyIsKilledApplyShellToAttacker] =
                    "When Enemy Is Killed Apply Shell To Attacker",
                [VanillaStatusEffects.WhenHealedApplyAttackToSelf] = "When Healed Apply Attack To Self",
                [VanillaStatusEffects.WhenHealedTriggerToSelf] = "When Healed Trigger To Self",
                [VanillaStatusEffects.WhenHealthLostApplyEqualAttackToSelfAndAllies] =
                    "When Health Lost Apply Equal Attack To Self And Allies",
                [VanillaStatusEffects.WhenHealthLostApplyEqualFrostToSelf] =
                    "When Health Lost Apply Equal Frost To Self",
                [VanillaStatusEffects.WhenHealthLostApplyEqualSpiceToSelf] =
                    "When Health Lost Apply Equal Spice To Self",
                [VanillaStatusEffects.WhenHitAddFrenzyToSelf] = "When Hit Add Frenzy To Self",
                [VanillaStatusEffects.WhenHitAddGearhammerToHand] = "When Hit Add Gearhammer To Hand",
                [VanillaStatusEffects.WhenHitAddHealthLostToAttacker] = "When Hit Add Health Lost To Attacker",
                [VanillaStatusEffects.WhenHitAddHealthLostToRandomAlly] = "When Hit Add Health Lost To RandomAlly",
                [VanillaStatusEffects.WhenHitAddJunkToHand] = "When Hit Add Junk To Hand",
                [VanillaStatusEffects.WhenHitApplyBlockToRandomAlly] = "When Hit Apply Block To RandomAlly",
                [VanillaStatusEffects.WhenHitApplyDemonizeToAttacker] = "When Hit Apply Demonize To Attacker",
                [VanillaStatusEffects.WhenHitApplyFrostToEnemies] = "When Hit Apply Frost To Enemies",
                [VanillaStatusEffects.WhenHitApplyFrostToRandomEnemy] = "When Hit Apply Frost To RandomEnemy",
                [VanillaStatusEffects.WhenHitApplyGoldToAttackerNoPing] = "When Hit Apply Gold To Attacker (No Ping)",
                [VanillaStatusEffects.WhenHitApplyInkToAttacker] = "When Hit Apply Ink To Attacker",
                [VanillaStatusEffects.WhenHitApplyInkToRandomEnemy] = "When Hit Apply Ink To RandomEnemy",
                [VanillaStatusEffects.WhenHitApplyInkToSelf] = "When Hit Apply Ink To Self",
                [VanillaStatusEffects.WhenHitApplyOverloadToAttacker] = "When Hit Apply Overload To Attacker",
                [VanillaStatusEffects.WhenHitApplyShellToAllies] = "When Hit Apply Shell To Allies",
                [VanillaStatusEffects.WhenHitApplyShellToAllyBehind] = "When Hit Apply Shell To AllyBehind",
                [VanillaStatusEffects.WhenHitApplyShellToSelf] = "When Hit Apply Shell To Self",
                [VanillaStatusEffects.WhenHitApplyShroomToAttacker] = "When Hit Apply Shroom To Attacker",
                [VanillaStatusEffects.WhenHitApplyShroomToRandomEnemy] = "When Hit Apply Shroom To RandomEnemy",
                [VanillaStatusEffects.WhenHitApplySnowToAttacker] = "When Hit Apply Snow To Attacker",
                [VanillaStatusEffects.WhenHitApplySnowToEnemies] = "When Hit Apply Snow To Enemies",
                [VanillaStatusEffects.WhenHitApplySnowToRandomEnemy] = "When Hit Apply Snow To RandomEnemy",
                [VanillaStatusEffects.WhenHitApplySpiceToAlliesAndEnemiesAndSelf] =
                    "When Hit Apply Spice To Allies & Enemies & Self",
                [VanillaStatusEffects.WhenHitApplySpiceToAllies] = "When Hit Apply Spice To Allies",
                [VanillaStatusEffects.WhenHitApplySpiceToAlliesInRow] = "When Hit Apply Spice To AlliesInRow",
                [VanillaStatusEffects.WhenHitApplySpiceToSelf] = "When Hit Apply Spice To Self",
                [VanillaStatusEffects.WhenHitApplyStealthToSelf] = "When Hit Apply Stealth To Self",
                [VanillaStatusEffects.WhenHitApplyVimToSelf] = "When Hit Apply Vim To Self",
                [VanillaStatusEffects.WhenHitApplyVoidToAttacker] = "When Hit Apply Void To Attacker",
                [VanillaStatusEffects.WhenHitApplyWeaknessToAttacker] = "When Hit Apply Weakness To Attacker",
                [VanillaStatusEffects.WhenHitDamageToEnemies] = "When Hit Damage To Enemies",
                [VanillaStatusEffects.WhenHitDamageToEnemiesInRow] = "When Hit Damage To EnemiesInRow",
                [VanillaStatusEffects.WhenHitDraw] = "When Hit Draw",
                [VanillaStatusEffects.WhenHitEqualDamageToAttacker] = "When Hit Equal Damage To Attacker",
                [VanillaStatusEffects.WhenHitGainAttackToSelfNoPing] = "When Hit Gain Attack To Self (No Ping)",
                [VanillaStatusEffects.WhenHitGainTeethToSelf] = "When Hit Gain Teeth To Self",
                [VanillaStatusEffects.WhenHitIncreaseAttackEffect1ToSelf] =
                    "When Hit Increase Attack Effect 1 To Self",
                [VanillaStatusEffects.WhenHitIncreaseAttackToRandomAlly] = "When Hit Increase Attack To RandomAlly",
                [VanillaStatusEffects.WhenHitIncreaseHealthToRandomAlly] = "When Hit Increase Health To RandomAlly",
                [VanillaStatusEffects.WhenHitReduceAttackToAttacker] = "When Hit Reduce Attack To Attacker",
                [VanillaStatusEffects.WhenHitReduceAttackToSelf] = "When Hit Reduce Attack To Self",
                [VanillaStatusEffects.WhenHitReduceCounterToSelf] = "When Hit Reduce Counter To Self",
                [VanillaStatusEffects.WhenHitTriggerToSelf] = "When Hit Trigger To Self",
                [VanillaStatusEffects.WhenHitWithJunkAddFrenzyToSelf] = "When Hit With Junk Add Frenzy To Self",
                [VanillaStatusEffects.WhenJuiceAppliedToSelfGainFrenzy] = "When Juice Applied To Self Gain Frenzy",
                [VanillaStatusEffects.WhenSacrificedSummonTailsFour] = "When Sacrificed Summon TailsFour",
                [VanillaStatusEffects.WhenSacrificedSummonTailsOne] = "When Sacrificed Summon TailsOne",
                [VanillaStatusEffects.WhenSacrificedSummonTailsThree] = "When Sacrificed Summon TailsThree",
                [VanillaStatusEffects.WhenSacrificedSummonTailsTwo] = "When Sacrificed Summon TailsTwo",
                [VanillaStatusEffects.WhenShellAppliedToSelfGainSpiceInstead] =
                    "When Shell Applied To Self Gain Spice Instead",
                [VanillaStatusEffects.WhenShroomAppliedToAnythingDoubleAmountAndLoseScrap] =
                    "When Shroom Applied To Anything Double Amount And Lose Scrap",
                [VanillaStatusEffects.WhenShroomDamageTakenTriggerToSelf] =
                    "When Shroom Damage Taken Trigger To Self",
                [VanillaStatusEffects.WhenSnowAppliedToAnythingGainAttackToSelf] =
                    "When Snow Applied To Anything Gain Attack To Self",
                [VanillaStatusEffects.WhenSnowAppliedToAnythingGainEqualAttackToSelf] =
                    "When Snow Applied To Anything Gain Equal Attack To Self",
                [VanillaStatusEffects.WhenSnowAppliedToSelfApplyDemonizeToEnemies] =
                    "When Snow Applied To Self Apply Demonize To Enemies",
                [VanillaStatusEffects.WhenSnowAppliedToSelfGainEqualAttack] =
                    "When Snow Applied To Self Gain Equal Attack",
                [VanillaStatusEffects.WhenSpiceXAppliedToSelfTriggerToSelf] =
                    "When Spice X Applied To Self Trigger To Self",
                [VanillaStatusEffects.WhenVimAppliedToAnythingDoubleAmount] =
                    "When Vim Applied To Anything Double Amount",
                [VanillaStatusEffects.WhenXHealthLostSplit] = "When X Health Lost Split",
                [VanillaStatusEffects.WhileActiveAddEqualAttackToJunkInHand] =
                    "While Active Add Equal Attack To Junk In Hand",
                [VanillaStatusEffects.WhileActiveAimlessToEnemies] = "While Active Aimless To Enemies",
                [VanillaStatusEffects.WhileActiveBarrageToAllies] = "While Active Barrage To Allies",
                [VanillaStatusEffects.WhileActiveBarrageToAlliesInRow] = "While Active Barrage To AlliesInRow",
                [VanillaStatusEffects.WhileActiveBarrageToEnemies] = "While Active Barrage To Enemies",
                [VanillaStatusEffects.WhileActiveFrenzyToAllies] = "While Active Frenzy To Allies",
                [VanillaStatusEffects.WhileActiveFrenzyToCrownAllies] = "While Active Frenzy To Crown Allies",
                [VanillaStatusEffects.WhileActiveHaltSpiceToAllies] = "While Active Halt Spice To Allies",
                [VanillaStatusEffects.WhileActiveIncreaseAllSpiceApplied] = "While Active Increase All Spice Applied",
                [VanillaStatusEffects.WhileActiveIncreaseAttackbyCurrentToAllies] =
                    "While Active Increase Attack by Current To Allies",
                [VanillaStatusEffects.WhileActiveIncreaseAttackbyCurrentToSummonedAllies] =
                    "While Active Increase Attack by Current To Summoned Allies",
                [VanillaStatusEffects.WhileActiveIncreaseAttackToAlliesAndEnemies] =
                    "While Active Increase Attack To Allies & Enemies",
                [VanillaStatusEffects.WhileActiveIncreaseAttackToAlliesNoDesc] =
                    "While Active Increase Attack To Allies (No Desc)",
                [VanillaStatusEffects.WhileActiveIncreaseAttackToAllies] = "While Active Increase Attack To Allies",
                [VanillaStatusEffects.WhileActiveIncreaseAttackToAlliesInRow] =
                    "While Active Increase Attack To AlliesInRow",
                [VanillaStatusEffects.WhileActiveIncreaseAttackToItemsInHand] =
                    "While Active Increase Attack To Items In Hand",
                [VanillaStatusEffects.WhileActiveIncreaseAttackToJunkInHand] =
                    "While Active Increase Attack To Junk In Hand",
                [VanillaStatusEffects.WhileActiveIncreaseEffectsToAlliesAndEnemies] =
                    "While Active Increase Effects To Allies & Enemies",
                [VanillaStatusEffects.WhileActiveIncreaseEffectsToFrontAlly] =
                    "While Active Increase Effects To FrontAlly",
                [VanillaStatusEffects.WhileActiveIncreaseEffectsToHand] = "While Active Increase Effects To Hand",
                [VanillaStatusEffects.WhileActivePigheadedToEnemies] = "While Active Pigheaded To Enemies",
                [VanillaStatusEffects.WhileActiveReduceAttackToEnemiesNoPingAndNoDesc] =
                    "While Active Reduce Attack To Enemies (No Ping, No Desc)",
                [VanillaStatusEffects.WhileActiveSnowImmuneToAllies] = "While Active Snow Immune To Allies",
                [VanillaStatusEffects.WhileActiveTeethToAllies] = "While Active Teeth To Allies",
                [VanillaStatusEffects.WhileActiveUnmovableToEnemies] = "While Active Unmovable To Enemies",
                [VanillaStatusEffects.WhileInHandReduceAttackToAllies] = "While In Hand Reduce Attack To Allies",
                [VanillaStatusEffects.WhileLastInHandDoubleEffectsToSelf] =
                    "While Last In Hand Double Effects To Self"
            };

    public static CardData SetSprites(this CardData t, Sprite mainSprite, Sprite backgroundSprite)
    {
        t.mainSprite = mainSprite;
        t.backgroundSprite = backgroundSprite;
        return t;
    }

    public static StatusEffectData StatusEffectData(this VanillaStatusEffects effect)
    {
        return VanillaStatusEffectsNamesLookUp[effect].StatusEffectData();
    }

    public static CardData.StatusEffectStacks StatusEffectStack(this VanillaStatusEffects effect, int amount)
    {
        return VanillaStatusEffectsNamesLookUp[effect].StatusEffectStack(amount);
    }

    public static StatusEffectData StatusEffectData(this string name)
    {
        return AddressableLoader.groups["StatusEffectData"].lookup[name].Cast<StatusEffectData>();
    }

    public static CardData.StatusEffectStacks StatusEffectStack(this string name, int amount)
    {
        return new CardData.StatusEffectStacks()
        {
            data = AddressableLoader.groups["StatusEffectData"].lookup[name].Cast<StatusEffectData>(), count = amount
        };
    }

    public enum VanillaTraits
    {
        None,
        Aimless,
        Backline,
        Barrage,
        Bombard1,
        Bombard2,
        Combo,
        Consume,
        Crush,
        Draw,
        Effigy,
        Explode,
        Frontline,
        Fury,
        Greed,
        Hellbent,
        Knockback,
        Longshot,
        Noomlin,
        Pigheaded,
        Pull,
        Recycle,
        Smackback,
        Soulbound,
        Spark,
        Summoned,
        Trash,
        Unmovable,
        Wild,
    }

    internal static readonly Dictionary<VanillaTraits, string>
        VanillaTraitsNamesLookUp =
            new()
            {
                [VanillaTraits.Aimless] = "Aimless",
                [VanillaTraits.Backline] = "Backline",
                [VanillaTraits.Barrage] = "Barrage",
                [VanillaTraits.Bombard1] = "Bombard 1",
                [VanillaTraits.Bombard2] = "Bombard 2",
                [VanillaTraits.Combo] = "Combo",
                [VanillaTraits.Consume] = "Consume",
                [VanillaTraits.Crush] = "Crush",
                [VanillaTraits.Draw] = "Draw",
                [VanillaTraits.Effigy] = "Effigy",
                [VanillaTraits.Explode] = "Explode",
                [VanillaTraits.Frontline] = "Frontline",
                [VanillaTraits.Fury] = "Fury",
                [VanillaTraits.Greed] = "Greed",
                [VanillaTraits.Hellbent] = "Hellbent",
                [VanillaTraits.Knockback] = "Knockback",
                [VanillaTraits.Longshot] = "Longshot",
                [VanillaTraits.Noomlin] = "Noomlin",
                [VanillaTraits.Pigheaded] = "Pigheaded",
                [VanillaTraits.Pull] = "Pull",
                [VanillaTraits.Recycle] = "Recycle",
                [VanillaTraits.Smackback] = "Smackback",
                [VanillaTraits.Soulbound] = "Soulbound",
                [VanillaTraits.Spark] = "Spark",
                [VanillaTraits.Summoned] = "Summoned",
                [VanillaTraits.Trash] = "Trash",
                [VanillaTraits.Unmovable] = "Unmovable",
                [VanillaTraits.Wild] = "Wild",
            };

    public static CardData.TraitStacks TraitStack(this VanillaTraits trait, int amount)
    {
        return VanillaTraitsNamesLookUp[trait].TraitStack(amount);
    }


    public static CardData.TraitStacks TraitStack(this string name, int amount)
    {
        return new CardData.TraitStacks()
        {
            data = AddressableLoader.groups["TraitData"].lookup[name].Cast<TraitData>(), count = amount
        };
    }


    public enum VanillaRewardPools
    {
        None,
        BasicItemPool,
        BasicUnitPool,
        BasicCharmPool,
        GeneralItemPool,
        GeneralUnitPool,
        GeneralCharmPool,
        MagicItemPool,
        MagicUnitPool,
        MagicCharmPool,
        ClunkItemPool,
        ClunkUnitPool,
        ClunkCharmPool
    }

    public static CardData AddToPool(this CardData t, params VanillaRewardPools[] rewardPools)
    {
        List<string> names = new();
        foreach (var p in rewardPools)
        {
            names.Add(p.ToString().Replace("VanillaRewardPools.", ""));
        }

        t = t.AddToPool(names.ToArray());
        return t;
    }

    public static CardData AddToPool(this CardData t, params string[] rewardPools)
    {
        var allPools = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<RewardPool>());
        foreach (var poolName in rewardPools)
        {
            var pool = allPools.ToList().Find(a => a.name == poolName).Cast<RewardPool>();
            pool?.list?.Add(t);
        }

        return t;
    }


    public enum VanillaCardAnimationProfiles
    {
        None,
        HeartbeatAnimationProfile,
        FloatAnimationProfile,
        FloatSquishAnimationProfile,
        FlyAnimationProfile,
        GiantAnimationProfile,
        HangAnimationProfile,
        Heartbeat2AnimationProfile,
        PingAnimationProfile,
        PulseAnimationProfile,
        ShakeAnimationProfile,
        SquishAnimationProfile,
        SwayAnimationProfile,
        GoopAnimationProfile,
    }

    public static CardData SetIdleAnimationProfile(this CardData t, VanillaCardAnimationProfiles profile)
    {
        return t.SetIdleAnimationProfile(profile.ToString().Replace("VanillaCardAnimationProfiles.", ""));
    }

    public static CardData SetIdleAnimationProfile(this CardData t, CardAnimationProfile profile)
    {
        t.idleAnimationProfile = profile;
        return t;
    }

    public static CardData SetIdleAnimationProfile(this CardData t, string animationProfileName)
    {
        t.idleAnimationProfile =
            WildFrostAPIMod.VanillaAnimationProfiles?.Find(a =>
                a != null && a.name.Equals(animationProfileName, StringComparison.OrdinalIgnoreCase));
        if (t.idleAnimationProfile == null)
            throw new Exception($"Animation profile with name {animationProfileName} not found!");
        return t;
    }

    public enum VanillaBloodProfiles
    {
        None,
        BloodProfileBerry,
        BloodProfileBlack,
        BloodProfileBlueDouble,
        BloodProfileFungus,
        BloodProfileNormal,
        BloodProfileSnow,
        BloodProfilePinkWisp,
        BloodProfileHusk,
    }

    internal static readonly Dictionary<VanillaBloodProfiles, string>
        VanillaBloodProfilesNamesLookUp =
            new()
            {
                [VanillaBloodProfiles.BloodProfileBerry] = "Blood Profile Berry",
                [VanillaBloodProfiles.BloodProfileBlack] = "Blood Profile Black",
                [VanillaBloodProfiles.BloodProfileBlueDouble] = "Blood Profile Blue (x2)",
                [VanillaBloodProfiles.BloodProfileFungus] = "Blood Profile Fungus",
                [VanillaBloodProfiles.BloodProfileNormal] = "Blood Profile Normal",
                [VanillaBloodProfiles.BloodProfileSnow] = "Blood Profile Snow",
                [VanillaBloodProfiles.BloodProfilePinkWisp] = "Blood Profile Pink Wisp",
                [VanillaBloodProfiles.BloodProfileHusk] = "Blood Profile Husk",
            };

    public static CardData SetBloodProfile(this CardData t, VanillaBloodProfiles bloodProfile)
    {
        return t.SetBloodProfile(VanillaBloodProfilesNamesLookUp[bloodProfile]);
    }

    public static CardData SetBloodProfile(this CardData t, BloodProfile bloodProfile)
    {
        t.bloodProfile = bloodProfile;
        return t;
    }

    public static CardData SetBloodProfile(this CardData t, string bloodProfileName)
    {
        t.bloodProfile =
            WildFrostAPIMod.VanillaBloodProfiles?.Find(a =>
                a != null && a.name.Equals(bloodProfileName, StringComparison.OrdinalIgnoreCase));
        if (t.bloodProfile == null)
            throw new Exception($"Blood profile with name {bloodProfileName} not found!");
        return t;
    }


    public enum VanillaTargetModes
    {
        None,
        TargetModeAll,
        TargetModeAllUndamaged,
        TargetModeBack,
        TargetModeBasic,
        TargetModeRandom,
        TargetModeRow,
        TargetModeCrowns,
    }

    public static CardData SetTargetMode(this CardData t, VanillaTargetModes vanillaTargetMode)
    {
        return t.SetTargetMode(vanillaTargetMode.ToString().Replace("VanillaTargetModes.", ""));
        ;
    }

    public static CardData SetTargetMode(this CardData t, TargetMode targetMode)
    {
        t.targetMode = targetMode;
        return t;
    }

    public static CardData SetTargetMode(this CardData t, string targetModeName)
    {
        t.targetMode = WildFrostAPIMod.VanillaTargetModes?.Find(a => a != null && a.name == targetModeName);
        return t;
    }

    public static Il2CppSystem.Collections.Generic.List<T> Dictinct<T>(
        this Il2CppSystem.Collections.Generic.List<T> list)
    {
        Il2CppSystem.Collections.Generic.List<T> distinctNames = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var name in list)
        {
            if (!distinctNames.Contains(name))
            {
                distinctNames.Add(name);
            }
        }

        return distinctNames;
    }

    public static CardData AddToPets(this CardData t)
    {
        var unlocks = SaveSystem.LoadProgressData<Il2CppSystem.Collections.Generic.List<string>>("petHutUnlocks",
            new Il2CppSystem.Collections.Generic.List<string>());
        unlocks.Add(t.name);
        SaveSystem.SaveProgressData<Il2CppSystem.Collections.Generic.List<string>>("petHutUnlocks", unlocks.Dictinct());
        var pets = MetaprogressionSystem.data["pets"].Cast<Il2CppStringArray>().ToList();
        pets.Add(t.name);
        MetaprogressionSystem.data["pets"] = pets.Dictinct().ToArray().Cast<Il2CppSystem.Object>();
        var selectStartingPet = UnityEngine.Object.FindObjectOfType<SelectStartingPet>();
        if (selectStartingPet != null)
        {
            selectStartingPet.group.Clear();
            CoroutineManager.Start(selectStartingPet.SetUp());
        }

        return t;
    }

    public static CardData SetStartWithEffects(this CardData t, params CardData.StatusEffectStacks[] effect)
    {
        t.startWithEffects = effect;
        return t;
    }

    public enum VanillaCardUpgrades
    {
        None,
        CardUpgradeAcorn,
        CardUpgradeAttackAndHealth,
        CardUpgradeAttackConsume,
        CardUpgradeAttackIncreaseCounter,
        CardUpgradeAttackRemoveEffects,
        CardUpgradeBalanced,
        CardUpgradeBarrage,
        CardUpgradeBattle,
        CardUpgradeBling,
        CardUpgradeBlock,
        CardUpgradeBom,
        CardUpgradeBombskull,
        CardUpgradeBoost,
        CardUpgradeCake,
        CardUpgradeCloudberry,
        CardUpgradeConsume,
        CardUpgradeConsumeAddHealth,
        CardUpgradeConsumeOverload,
        CardUpgradeCritical,
        CardUpgradeCrush,
        CardUpgradeDemonize,
        CardUpgradeDraw,
        CardUpgradeEffigy,
        CardUpgradeFrenzyConsume,
        CardUpgradeFrenzyReduceAttack,
        CardUpgradeFrosthand,
        CardUpgradeFury,
        CardUpgradeGreed,
        CardUpgradeHeart,
        CardUpgradeHook,
        CardUpgradeInk,
        CardUpgradeNoomlin,
        CardUpgradeOverload,
        CardUpgradePig,
        CardUpgradePunchfist,
        CardUpgradeRemoveCharmLimit,
        CardUpgradeScrap,
        CardUpgradeShellBecomesSpice,
        CardUpgradeShellOnKill,
        CardUpgradeShroom,
        CardUpgradeShroomReduceHealth,
        CardUpgradeSnowball,
        CardUpgradeSnowImmune,
        CardUpgradeSpark,
        CardUpgradeSpice,
        CardUpgradeSpiky,
        CardUpgradeSun,
        CardUpgradeTeethWhenHit,
        CardUpgradeTrash,
        CardUpgradeWeakness,
        CardUpgradeWildcard,
        Crown,
    }

    public static CardData SetUpgrades(this CardData t, Il2CppSystem.Collections.Generic.List<string> upgrade)
    {
        Il2CppSystem.Collections.Generic.List<CardUpgradeData> upgrades =
            new Il2CppSystem.Collections.Generic.List<CardUpgradeData>();
        foreach (var u in upgrade)
        {
            upgrades.Add(AddressableLoader.groups["CardUpgradeData"].lookup[u].Cast<CardUpgradeData>());
        }

        return t;
    }

    public static CardData SetUpgrades(this CardData t,
        Il2CppSystem.Collections.Generic.List<VanillaCardUpgrades> upgrade)
    {
        Il2CppSystem.Collections.Generic.List<CardUpgradeData> upgrades =
            new Il2CppSystem.Collections.Generic.List<CardUpgradeData>();
        foreach (var u in upgrade)
        {
            upgrades.Add(AddressableLoader.groups["CardUpgradeData"]
                .lookup[u.ToString().Replace("VanillaCardUpgrades.", "")].Cast<CardUpgradeData>());
        }

        return t;
    }

    public static CardData SetUpgrades(this CardData t, Il2CppSystem.Collections.Generic.List<CardUpgradeData> upgrade)
    {
        t.upgrades = upgrade;
        return t;
    }

    public static CardData SetAttackEffects(this CardData t, params CardData.StatusEffectStacks[] effect)
    {
        t.attackEffects = effect;
        return t;
    }

    public static CardData SetTraits(this CardData t, params CardData.TraitStacks[] traits)
    {
        var list = new Il2CppSystem.Collections.Generic.List<CardData.TraitStacks>();
        foreach (var trait in traits)
        {
            list.Add(trait);
        }

        t.traits = list;
        return t;
    }

    public static CardData SetStats(this CardData t, int? health = null, int? damage = null, int counter = 0)
    {
        return t.SetHealth(health).SetDamage(damage).SetCounter(counter);
    }

    public static CardData SetCounter(this CardData t, int counter)
    {
        t.counter = counter;
        return t;
    }

    public static CardData SetDamage(this CardData t, int? damage)
    {
        if (damage.HasValue)
        {
            t.hasAttack = true;
            t.damage = damage.Value;
        }

        return t;
    }

    public static CardData SetHealth(this CardData t, int? health)
    {
        if (health.HasValue)
        {
            t.hasHealth = true;
            t.hp = health.Value;
        }

        return t;
    }

    public static CardData SetSprites(this CardData t, string mainSprite, string backgroundSprite)
    {
        t.mainSprite = LoadSpriteFromCardPortraits(mainSprite);
        t.backgroundSprite = LoadSpriteFromCardPortraits(backgroundSprite);
        return t;
    }

    public static CardData SetIsUnit(this CardData t)
    {
        t.canBeHit = true;
        t.playType = Card.PlayType.Place;
        t.canPlayOnBoard = true;
        return t;
    }

    public enum VanillaCardTypes
    {
        None,
        Boss,
        BossSmall,
        Clunker,
        Enemy,
        Friendly,
        Item,
        Leader,
        Miniboss,
        Summoned,
    }

    public static CardData SetCardType(this CardData t, VanillaCardTypes cardType)
    {
        t.cardType = AddressableLoader.GetGroup<CardType>("CardType").Find(delegate(CardType type)
        {
            return type.name == cardType.ToString().Replace("VanillaCardTypes.", "");
        });
        return t;
    }

    public static CardData SetCardType(this CardData t, string cardTypeName)
    {
        t.cardType = AddressableLoader.GetGroup<CardType>("CardType").Find(delegate(CardType type)
        {
            return type.name == cardTypeName;
        });
        return t;
    }

    [Flags]
    public enum CanPlay
    {
        None,
        CanPlayOnBoard = 0b1,
        CanPlayOnEnemy = 0b10,
        CanPlayOnFriendly = 0b100,
        CanPlayOnHand = 0b1000,
    }

    public static CardData SetCanPlay(this CardData t, CanPlay canPlayFlags)
    {
        t.canPlayOnBoard = canPlayFlags.HasFlag(CanPlay.CanPlayOnBoard);
        t.canPlayOnEnemy = canPlayFlags.HasFlag(CanPlay.CanPlayOnEnemy);
        t.canPlayOnFriendly = canPlayFlags.HasFlag(CanPlay.CanPlayOnFriendly);
        t.canPlayOnHand = canPlayFlags.HasFlag(CanPlay.CanPlayOnHand);
        return t;
    }

    public static CardData SetItemUses(this CardData t, int amount)
    {
        t.uses = amount;
        return t;
    }

    public static CardData SetIsItem(this CardData t)
    {
        t.uses = 1;
        t.canBeHit = false;
        t.playType = Card.PlayType.Play;
        t = t.SetCardType(VanillaCardTypes.Item);
        return t;
    }

    public static CardData SetTitle(this CardData t, string name)
    {
        t.titleKey = LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name + ".Title", name));
        return t;
    }

    public static CardData SetText(this CardData t, string text)
    {
        t.textKey = LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name + ".Text", text));
        return t;
    }

    public static CardData SetFlavour(this CardData t, string flavour)
    {
        t.flavourKey =
            LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name + ".Flavour", flavour));
        return t;
    }

    public static CardData CreateCardData(string modName, string cardName)
    {
        string oldName = cardName;
        cardName=cardName.StartsWith(modName) ? cardName : $"{modName}.{cardName}";
        if (modName == "") cardName= oldName;
        CardData newData = null;
        var cardWithSameName = AddressableLoader.GetGroup<CardData>("CardData").ToArray().ToList()
            .Find(c => c.name == cardName);
        if (cardWithSameName != null)
        {
            newData = cardWithSameName;
        }
        else  newData = ScriptableObject.CreateInstance<CardData>();
        newData.titleKey = new LocalizedString();
        newData.flavourKey = new LocalizedString();
        newData.textKey = new LocalizedString();
        newData.injuries = new Il2CppSystem.Collections.Generic.List<CardData.StatusEffectStacks>();
        newData.upgrades = new Il2CppSystem.Collections.Generic.List<CardUpgradeData>();
        newData.attackEffects = new Il2CppReferenceArray<CardData.StatusEffectStacks>(0);
        newData.startWithEffects = new Il2CppReferenceArray<CardData.StatusEffectStacks>(0);
        newData.traits = new Il2CppSystem.Collections.Generic.List<CardData.TraitStacks>();
        newData.createScripts = new Il2CppReferenceArray<CardScript>(0);
        newData = newData.SetTargetMode(VanillaTargetModes.TargetModeBasic);
        newData.name =cardName;
        newData.cardType = AddressableLoader.GetGroup<CardType>("CardType").Find(delegate(CardType type)
        {
            return type.name == "Friendly";
        });
        newData.backgroundSprite = LoadSpriteFromCardPortraits("CardPortraits\\FALLBACKBACKGROUNDSPRITE.png");
        newData.mainSprite = LoadSpriteFromCardPortraits("CardPortraits\\FALLBACKMAINSPRITE.png");
        return newData;
    }
}