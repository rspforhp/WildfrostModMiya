using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using WildfrostModMiya;
using Console = System.Console;
using File = Il2CppSystem.IO.File;
using Object = UnityEngine.Object; // The namespace of your mod class
// ...
[assembly: MelonInfo(typeof(Mod), "WildFrost Mod", "1", "Kopie_Miya")]
[assembly: MelonGame("Deadpan Games", "Wildfrost")]

namespace WildfrostModMiya
{
    
        public static class JSONParser
    {
        [ThreadStatic] static Stack<List<string>> splitArrayPool;
        [ThreadStatic] static StringBuilder stringBuilder;
        [ThreadStatic] static Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache;
        [ThreadStatic] static Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache;

        public static T FromJson<T>(this string json)
        {
            // Initialize, if needed, the ThreadStatic variables
            if (propertyInfoCache == null) propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            if (fieldInfoCache == null) fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
            if (stringBuilder == null) stringBuilder = new StringBuilder();
            if (splitArrayPool == null) splitArrayPool = new Stack<List<string>>();

            //Remove all whitespace not within strings to make parsing simpler
            stringBuilder.Length = 0;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '"')
                {
                    i = AppendUntilStringEnd(true, i, json);
                    continue;
                }
                if (char.IsWhiteSpace(c))
                    continue;

                stringBuilder.Append(c);
            }

            //Parse the thing!
            return (T)ParseValue(typeof(T), stringBuilder.ToString());
        }

        static int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            stringBuilder.Append(json[startIdx]);
            for (int i = startIdx + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    if (appendEscapeCharacter)
                        stringBuilder.Append(json[i]);
                    stringBuilder.Append(json[i + 1]);
                    i++;//Skip next character as it is escaped
                }
                else if (json[i] == '"')
                {
                    stringBuilder.Append(json[i]);
                    return i;
                }
                else
                    stringBuilder.Append(json[i]);
            }
            return json.Length - 1;
        }

        //Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings
        static List<string> Split(string json)
        {
            List<string> splitArray = splitArrayPool.Count > 0 ? splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();
            if (json.Length == 2)
                return splitArray;
            int parseDepth = 0;
            stringBuilder.Length = 0;
            for (int i = 1; i < json.Length - 1; i++)
            {
                switch (json[i])
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '"':
                        i = AppendUntilStringEnd(true, i, json);
                        continue;
                    case ',':
                    case ':':
                        if (parseDepth == 0)
                        {
                            splitArray.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }

                stringBuilder.Append(json[i]);
            }

            splitArray.Add(stringBuilder.ToString());

            return splitArray;
        }

        internal static object ParseValue(Type type, string json)
        {
            if (type == typeof(string))
            {
                if (json.Length <= 2)
                    return string.Empty;
                StringBuilder parseStringBuilder = new StringBuilder(json.Length);
                for (int i = 1; i < json.Length - 1; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length - 1)
                    {
                        int j = "\"\\nrtbf/".IndexOf(json[i + 1]);
                        if (j >= 0)
                        {
                            parseStringBuilder.Append("\"\\\n\r\t\b\f/"[j]);
                            ++i;
                            continue;
                        }
                        if (json[i + 1] == 'u' && i + 5 < json.Length - 1)
                        {
                            UInt32 c = 0;
                            if (UInt32.TryParse(json.Substring(i + 2, 4), System.Globalization.NumberStyles.AllowHexSpecifier, null, out c))
                            {
                                parseStringBuilder.Append((char)c);
                                i += 5;
                                continue;
                            }
                        }
                    }
                    parseStringBuilder.Append(json[i]);
                }
                return parseStringBuilder.ToString();
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var result = Convert.ChangeType(json, type.GetGenericArguments().First(), System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
            if (type.IsPrimitive)
            {
                var result = Convert.ChangeType(json, type, System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
            if (type == typeof(decimal))
            {
                decimal result;
                decimal.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                return result;
            }
            if (type == typeof(DateTime))
            {
                DateTime result;
                DateTime.TryParse(json.Replace("\"",""), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result);
                return result;
            }
            if (json == "null")
            {
                return null;
            }
            if (type.IsEnum)
            {
                if (json[0] == '"')
                    json = json.Substring(1, json.Length - 2);
                try
                {
                    return Enum.Parse(type, json, false);
                }
                catch
                {
                    return 0;
                }
            }
            if (type.IsArray)
            {
                Type arrayType = type.GetElementType();
                if (json[0] != '[' || json[json.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json);
                Array newArray = Array.CreateInstance(arrayType, elems.Count);
                for (int i = 0; i < elems.Count; i++)
                    newArray.SetValue(ParseValue(arrayType, elems[i]), i);
                splitArrayPool.Push(elems);
                return newArray;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listType = type.GetGenericArguments()[0];
                if (json[0] != '[' || json[json.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json);
                var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });
                for (int i = 0; i < elems.Count; i++)
                    list.Add(ParseValue(listType, elems[i]));
                splitArrayPool.Push(elems);
                return list;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType, valueType;
                {
                    Type[] args = type.GetGenericArguments();
                    keyType = args[0];
                    valueType = args[1];
                }

                //Refuse to parse dictionary keys that aren't of type string
                if (keyType != typeof(string))
                    return null;
                //Must be a valid dictionary element
                if (json[0] != '{' || json[json.Length - 1] != '}')
                    return null;
                //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                    return null;

                var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });
                for (int i = 0; i < elems.Count; i += 2)
                {
                    if (elems[i].Length <= 2)
                        continue;
                    string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                    object val = ParseValue(valueType, elems[i + 1]);
                    dictionary[keyValue] = val;
                }
                return dictionary;
            }
            if (type == typeof(object))
            {
                return ParseAnonymousValue(json);
            }
            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                return ParseObject(type, json);
            }

            return null;
        }

        static object ParseAnonymousValue(string json)
        {
            if (json.Length == 0)
                return null;
            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                    return null;
                var dict = new Dictionary<string, object>(elems.Count / 2);
                for (int i = 0; i < elems.Count; i += 2)
                    dict[elems[i].Substring(1, elems[i].Length - 2)] = ParseAnonymousValue(elems[i + 1]);
                return dict;
            }
            if (json[0] == '[' && json[json.Length - 1] == ']')
            {
                List<string> items = Split(json);
                var finalList = new List<object>(items.Count);
                for (int i = 0; i < items.Count; i++)
                    finalList.Add(ParseAnonymousValue(items[i]));
                return finalList;
            }
            if (json[0] == '"' && json[json.Length - 1] == '"')
            {
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\", string.Empty);
            }
            if (char.IsDigit(json[0]) || json[0] == '-')
            {
                if (json.Contains("."))
                {
                    double result;
                    double.TryParse(json, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
                    return result;
                }
                else
                {
                    int result;
                    int.TryParse(json, out result);
                    return result;
                }
            }
            if (json == "true")
                return true;
            if (json == "false")
                return false;
            // handles json == "null" as well as invalid JSON
            return null;
        }

        static Dictionary<string, T> CreateMemberNameDictionary<T>(T[] members) where T : MemberInfo
        {
            Dictionary<string, T> nameToMember = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < members.Length; i++)
            {
                T member = members[i];
                if (member.IsDefined(typeof(IgnoreDataMemberAttribute), true))
                    continue;

                string name = member.Name;
                if (member.IsDefined(typeof(DataMemberAttribute), true))
                {
                    DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
                    if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                        name = dataMemberAttribute.Name;
                }

                nameToMember.Add(name, member);
            }

            return nameToMember;
        }

        static object ParseObject(Type type, string json)
        {
            object instance = FormatterServices.GetUninitializedObject(type);

            //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            List<string> elems = Split(json);
            if (elems.Count % 2 != 0)
                return instance;

            Dictionary<string, FieldInfo> nameToField;
            Dictionary<string, PropertyInfo> nameToProperty;
            if (!fieldInfoCache.TryGetValue(type, out nameToField))
            {
                nameToField = CreateMemberNameDictionary(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                fieldInfoCache.Add(type, nameToField);
            }
            if (!propertyInfoCache.TryGetValue(type, out nameToProperty))
            {
                nameToProperty = CreateMemberNameDictionary(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy));
                propertyInfoCache.Add(type, nameToProperty);
            }

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                    continue;
                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                FieldInfo fieldInfo;
                PropertyInfo propertyInfo;
                if (nameToField.TryGetValue(key, out fieldInfo))
                    fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                else if (nameToProperty.TryGetValue(key, out propertyInfo))
                    propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
            }

            return instance;
        }
    }
    public static class Extensions
    {
        public static string VanillaEffectName(this WildFrostAPI.CardBuilder.VanillaEffects effect) =>
            WildFrostAPI.CardBuilder.VanillaEffectsNamesLookUp[effect];

        public static string VanillaTraitName(this WildFrostAPI.CardBuilder.VanillaTraits traits) =>
            WildFrostAPI.CardBuilder.VanillaTraitsNamesLookUp[traits];
    }

    public static class WildFrostAPI
    {
        public class CardBuilder
        {
            public CardBuilder(string fallbackCard = "Naked Gnome")
            {
                CardData woodheadCard = null;
                foreach (var cardO in AddressableLoader.groups["CardData"].list)
                {
                    var card = cardO.Cast<CardData>();
                    if (card.title.Equals(fallbackCard))
                    {
                        woodheadCard = card;
                        break;
                    }
                }

                data = woodheadCard.Clone();
                data.original = data;
            }

            public CardData data;

            public CardBuilder SetTitle(string title = "")
            {
                // data.name = title; THIS IS SO THE GAME DOESNT ERRORS
                data.titleFallback = title;
                data.forceTitle = title;
                return this;
            }

            public CardBuilder SetFlavour(string flavour = "")
            {
                data.flavour = flavour;
                data.flavourKey = new LocalizedString() { };
                return this;
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

            public enum VanillaEffects
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
                WhileLastInHandDoubleEffectsToSelf,
            }

            internal static readonly System.Collections.Generic.Dictionary<VanillaTraits, string>
                VanillaTraitsNamesLookUp =
                    new System.Collections.Generic.Dictionary<VanillaTraits, string>()
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

            internal static readonly System.Collections.Generic.Dictionary<VanillaEffects, string>
                VanillaEffectsNamesLookUp =
                    new System.Collections.Generic.Dictionary<VanillaEffects, string>()
                    {
                        [VanillaEffects.AddAttackAndHealthToSummon] = "Add Attack & Health To Summon",
                        [VanillaEffects.Block] = "Block",
                        [VanillaEffects.Bombard1] = "Bombard 1",
                        [VanillaEffects.Bombard2] = "Bombard 2",
                        [VanillaEffects.BonusDamageEqualToDartsInHand] = "Bonus Damage Equal To Darts In Hand",
                        [VanillaEffects.BonusDamageEqualToGoldFactor002] = "Bonus Damage Equal To Gold Factor 0.02",
                        [VanillaEffects.BonusDamageEqualToJuice] = "Bonus Damage Equal To Juice",
                        [VanillaEffects.BonusDamageEqualToScrapOnBoard] = "Bonus Damage Equal To Scrap On Board",
                        [VanillaEffects.BonusDamageEqualToScrap] = "Bonus Damage Equal To Scrap",
                        [VanillaEffects.BonusDamageEqualToShell] = "Bonus Damage Equal To Shell",
                        [VanillaEffects.BoostEffects] = "Boost Effects",
                        [VanillaEffects.Budge] = "Budge",
                        [VanillaEffects.CannotRecall] = "Cannot Recall",
                        [VanillaEffects.CheckHasJuice] = "Check Has Juice",
                        [VanillaEffects.Cleanse] = "Cleanse",
                        [VanillaEffects.CombineWhen2Deployed] = "Combine When 2 Deployed",
                        [VanillaEffects.CopyEffects] = "Copy Effects",
                        [VanillaEffects.Crush] = "Crush",
                        [VanillaEffects.DamageEqualToHealth] = "Damage Equal To Health",
                        [VanillaEffects.DamageToFrontAllies] = "Damage To Front Allies",
                        [VanillaEffects.Demonize] = "Demonize",
                        [VanillaEffects.DestroyAfterUse] = "Destroy After Use",
                        [VanillaEffects.DestroySelfAfterTurn] = "Destroy Self After Turn",
                        [VanillaEffects.DoubleAllSpiceWhenDestroyed] = "Double All Spice When Destroyed",
                        [VanillaEffects.DoubleBlock] = "Double Block",
                        [VanillaEffects.DoubleInk] = "Double Ink",
                        [VanillaEffects.DoubleJuice] = "Double Juice",
                        [VanillaEffects.DoubleNegativeEffects] = "Double Negative Effects",
                        [VanillaEffects.DoubleOverload] = "Double Overload",
                        [VanillaEffects.DoubleShell] = "Double Shell",
                        [VanillaEffects.DoubleShroom] = "Double Shroom",
                        [VanillaEffects.DoubleSpice] = "Double Spice",
                        [VanillaEffects.DoubleVim] = "Double Vim",
                        [VanillaEffects.DrawCards] = "Draw Cards",
                        [VanillaEffects.EatHealthAndAttack] = "Eat (Health & Attack)",
                        [VanillaEffects.EatHealthAndAttackAndEffects] = "Eat (Health, Attack & Effects)",
                        [VanillaEffects.EatAlliesInRowHealthAndAttack] = "Eat Allies In Row (Health & Attack)",
                        [VanillaEffects.EatRandomAllyHealthAndAttackAndEffects] =
                            "Eat Random Ally (Health, Attack & Effects)",
                        [VanillaEffects.Escape] = "Escape",
                        [VanillaEffects.FillBoardFinalBoss] = "Fill Board (Final Boss)",
                        [VanillaEffects.FinalBossPhase2] = "FinalBossPhase2",
                        [VanillaEffects.Flee] = "Flee",
                        [VanillaEffects.FreeAction] = "Free Action",
                        [VanillaEffects.FrenzyBossPhase2] = "FrenzyBossPhase2",
                        [VanillaEffects.Frost] = "Frost",
                        [VanillaEffects.GainFrenzyWhenWildUnitKilled] = "Gain Frenzy When Wild Unit Killed",
                        [VanillaEffects.GainGoldRange36] = "Gain Gold Range (3-6)",
                        [VanillaEffects.GainGold] = "Gain Gold",
                        [VanillaEffects.GoatWampusPhase2] = "GoatWampusPhase2",
                        [VanillaEffects.HaltSpice] = "Halt Spice",
                        [VanillaEffects.Haze] = "Haze",
                        [VanillaEffects.HealNoPing] = "Heal (No Ping)",
                        [VanillaEffects.HealFrontAllyBasedOnDamageDealt] = "Heal Front Ally Based On Damage Dealt",
                        [VanillaEffects.HealFullAndGainEqualSpice] = "Heal Full, Gain Equal Spice",
                        [VanillaEffects.HealToFrontAllies] = "Heal To Front Allies",
                        [VanillaEffects.Heal] = "Heal",
                        [VanillaEffects.HighPriorityPosition] = "High Priority Position",
                        [VanillaEffects.HitAllCrownEnemies] = "Hit All Crown Enemies",
                        [VanillaEffects.HitAllEnemiesInRow] = "Hit All Enemies In Row",
                        [VanillaEffects.HitAllEnemies] = "Hit All Enemies",
                        [VanillaEffects.HitAllUndamagedEnemies] = "Hit All Undamaged Enemies",
                        [VanillaEffects.HitFurthestTarget] = "Hit Furthest Target",
                        [VanillaEffects.HitRandomTarget] = "Hit Random Target",
                        [VanillaEffects.ImmuneToFrost] = "ImmuneToFrost",
                        [VanillaEffects.ImmuneToSnow] = "ImmuneToSnow",
                        [VanillaEffects.ImmuneToSpice] = "ImmuneToSpice",
                        [VanillaEffects.ImmuneToVim] = "ImmuneToVim",
                        [VanillaEffects.IncreaseAllSpiceAppliedNoDesc] = "Increase All Spice Applied (No Desc)",
                        [VanillaEffects.IncreaseAttackAndHealth] = "Increase Attack & Health",
                        [VanillaEffects.IncreaseAttackAndLoseHalfHealth] = "Increase Attack & Lose Half Health",
                        [VanillaEffects.IncreaseAttackEffect1] = "Increase Attack Effect 1",
                        [VanillaEffects.IncreaseAttackWhileAlone] = "Increase Attack While Alone",
                        [VanillaEffects.IncreaseAttackWhileDamaged] = "Increase Attack While Damaged",
                        [VanillaEffects.IncreaseAttack] = "Increase Attack",
                        [VanillaEffects.IncreaseEffects] = "Increase Effects",
                        [VanillaEffects.IncreaseMaxCounter] = "Increase Max Counter",
                        [VanillaEffects.IncreaseMaxHealth] = "Increase Max Health",
                        [VanillaEffects.Injury] = "Injury",
                        [VanillaEffects.InstantAddScrap] = "Instant Add Scrap",
                        [VanillaEffects.InstantApplyAttackToApplier] = "Instant Apply Attack To Applier",
                        [VanillaEffects.InstantApplyCurrentAttackToAllies] = "Instant Apply Current Attack To Allies",
                        [VanillaEffects.InstantApplyCurrentAttackToRandomAlly] =
                            "Instant Apply Current Attack To Random Ally",
                        [VanillaEffects.InstantApplyFrenzyToItemInHand] = "Instant Apply Frenzy (To Item In Hand)",
                        [VanillaEffects.InstantDraw] = "Instant Draw",
                        [VanillaEffects.InstantGainAimless] = "Instant Gain Aimless",
                        [VanillaEffects.InstantGainFury] = "Instant Gain Fury",
                        [VanillaEffects.InstantGainNoomlinToCardInHand] = "Instant Gain Noomlin (To Card In Hand)",
                        [VanillaEffects.InstantGainSoulboundToEnemy] = "Instant Gain Soulbound (To Enemy)",
                        [VanillaEffects.InstantSummonBloo] = "Instant Summon Bloo",
                        [VanillaEffects.InstantSummonCopyOfItem] = "Instant Summon Copy Of Item",
                        [VanillaEffects.InstantSummonCopyOnOtherSideWithXHealth] =
                            "Instant Summon Copy On Other Side With X Health",
                        [VanillaEffects.InstantSummonCopy] = "Instant Summon Copy",
                        [VanillaEffects.InstantSummonDregg] = "Instant Summon Dregg",
                        [VanillaEffects.InstantSummonFallow] = "Instant Summon Fallow",
                        [VanillaEffects.InstantSummonGearhammerInHand] = "Instant Summon Gearhammer In Hand",
                        [VanillaEffects.InstantSummonJunkInHand] = "Instant Summon Junk In Hand",
                        [VanillaEffects.InstantSummonSunShardInHand] = "Instant Summon SunShard In Hand",
                        [VanillaEffects.InstantSummonTailsFour] = "Instant Summon TailsFour",
                        [VanillaEffects.InstantSummonTailsOne] = "Instant Summon TailsOne",
                        [VanillaEffects.InstantSummonTailsThree] = "Instant Summon TailsThree",
                        [VanillaEffects.InstantSummonTailsTwo] = "Instant Summon TailsTwo",
                        [VanillaEffects.Kill] = "Kill",
                        [VanillaEffects.LastStand] = "Last Stand",
                        [VanillaEffects.LoseHalfHealth] = "Lose Half Health",
                        [VanillaEffects.LoseJuice] = "Lose Juice",
                        [VanillaEffects.LoseScrap] = "Lose Scrap",
                        [VanillaEffects.LowPriorityPosition] = "Low Priority Position",
                        [VanillaEffects.Lumin] = "Lumin",
                        [VanillaEffects.MultiHitTemporaryAndNotVisible] = "MultiHit (Temporary, Not Visible)",
                        [VanillaEffects.MultiHit] = "MultiHit",
                        [VanillaEffects.MustHaveJuiceToTrigger] = "Must Have Juice To Trigger",
                        [VanillaEffects.Null] = "Null",
                        [VanillaEffects.OnCardPlayedAddFuryToTarget] = "On Card Played Add Fury To Target",
                        [VanillaEffects.OnCardPlayedAddGearhammerToHand] = "On Card Played Add Gearhammer To Hand",
                        [VanillaEffects.OnCardPlayedAddJunkToHand] = "On Card Played Add Junk To Hand",
                        [VanillaEffects.OnCardPlayedAddScrapToRandomAlly] = "On Card Played Add Scrap To RandomAlly",
                        [VanillaEffects.OnCardPlayedAddSoulboundToRandomAlly] =
                            "On Card Played Add Soulbound To RandomAlly",
                        [VanillaEffects.OnCardPlayedAddSunShardToHand] = "On Card Played Add SunShard To Hand",
                        [VanillaEffects.OnCardPlayedApplyAttackToSelf] = "On Card Played Apply Attack To Self",
                        [VanillaEffects.OnCardPlayedApplyBlockToRandomUnit] =
                            "On Card Played Apply Block To RandomUnit",
                        [VanillaEffects.OnCardPlayedApplyFrostToRandomEnemy] =
                            "On Card Played Apply Frost To RandomEnemy",
                        [VanillaEffects.OnCardPlayedApplyHazeToRandomEnemy] =
                            "On Card Played Apply Haze To RandomEnemy",
                        [VanillaEffects.OnCardPlayedApplyInkToRandomEnemy] = "On Card Played Apply Ink To RandomEnemy",
                        [VanillaEffects.OnCardPlayedApplyOverloadToFrontEnemy] =
                            "On Card Played Apply Overload To FrontEnemy",
                        [VanillaEffects.OnCardPlayedApplyShellToRandomAlly] =
                            "On Card Played Apply Shell To RandomAlly",
                        [VanillaEffects.OnCardPlayedApplyShroomToEnemies] = "On Card Played Apply Shroom To Enemies",
                        [VanillaEffects.OnCardPlayedApplySnowToEnemiesInRow] =
                            "On Card Played Apply Snow To EnemiesInRow",
                        [VanillaEffects.OnCardPlayedApplySpiceToRandomAlly] =
                            "On Card Played Apply Spice To RandomAlly",
                        [VanillaEffects.OnCardPlayedApplyTeethToRandomAlly] =
                            "On Card Played Apply Teeth To RandomAlly",
                        [VanillaEffects.OnCardPlayedBoostToRandomAlly] = "On Card Played Boost To RandomAlly",
                        [VanillaEffects.OnCardPlayedBoostToRandomEnemy] = "On Card Played Boost To RandomEnemy",
                        [VanillaEffects.OnCardPlayedDamageFrostedEnemies] = "On Card Played Damage Frosted Enemies",
                        [VanillaEffects.OnCardPlayedDamageInkedEnemies] = "On Card Played Damage Inked Enemies",
                        [VanillaEffects.OnCardPlayedDamageToSelfAndAlliesInRow] =
                            "On Card Played Damage To Self & AlliesInRow",
                        [VanillaEffects.OnCardPlayedDamageUndamagedEnemies] = "On Card Played Damage Undamaged Enemies",
                        [VanillaEffects.OnCardPlayedDestroyRandomCardInHand] =
                            "On Card Played Destroy Random Card In Hand",
                        [VanillaEffects.OnCardPlayedDestroyRandomJunkInHand] =
                            "On Card Played Destroy Random Junk In Hand",
                        [VanillaEffects.OnCardPlayedDestroyRightmostCardInHand] =
                            "On Card Played Destroy Rightmost Card In Hand",
                        [VanillaEffects.OnCardPlayedDoubleVimToSelf] = "On Card Played Double Vim To Self",
                        [VanillaEffects.OnCardPlayedLose1JuiceToSelfNoDesc] =
                            "On Card Played Lose 1 Juice To Self (No Desc)",
                        [VanillaEffects.OnCardPlayedLoseScrapToSelf] = "On Card Played Lose Scrap To Self",
                        [VanillaEffects.OnCardPlayedReduceAttackEffect1ToSelf] =
                            "On Card Played Reduce Attack Effect 1 To Self",
                        [VanillaEffects.OnCardPlayedReduceCounterToAllies] = "On Card Played Reduce Counter To Allies",
                        [VanillaEffects.OnCardPlayedSacrificeRandomAlly] = "On Card Played Sacrifice RandomAlly",
                        [VanillaEffects.OnCardPlayedTakeHealthFromAllies] = "On Card Played Take Health From Allies",
                        [VanillaEffects.OnCardPlayedTriggerAgainstAllyBehind] =
                            "On Card Played Trigger Against AllyBehind",
                        [VanillaEffects.OnCardPlayedTriggerRandomAlly] = "On Card Played Trigger RandomAlly",
                        [VanillaEffects.OnCardPlayedUseRandomItemInHandAgainstRandomEnemy] =
                            "On Card Played Use Random Item In Hand Against Random Enemy",
                        [VanillaEffects.OnCardPlayedVimToAllies] = "On Card Played Vim To Allies",
                        [VanillaEffects.OnCardPlayedVimToSelf] = "On Card Played Vim To Self",
                        [VanillaEffects.OnCardPlayedVoidToEnemies] = "On Card Played Void To Enemies",
                        [VanillaEffects.OnHitDamageDamagedTarget] = "On Hit Damage Damaged Target",
                        [VanillaEffects.OnHitDamageFrostedTarget] = "On Hit Damage Frosted Target",
                        [VanillaEffects.OnHitDamageShelledTarget] = "On Hit Damage Shelled Target",
                        [VanillaEffects.OnHitDamageSnowedTarget] = "On Hit Damage Snowed Target",
                        [VanillaEffects.OnHitEqualDamageToInkedTarget] = "On Hit Equal Damage To Inked Target",
                        [VanillaEffects.OnHitEqualHealToFrontAlly] = "On Hit Equal Heal To FrontAlly",
                        [VanillaEffects.OnHitEqualOverloadToTarget] = "On Hit Equal Overload To Target",
                        [VanillaEffects.OnHitEqualSnowToTarget] = "On Hit Equal Snow To Target",
                        [VanillaEffects.OnHitPullTarget] = "On Hit Pull Target",
                        [VanillaEffects.OnHitPushTarget] = "On Hit Push Target",
                        [VanillaEffects.OnKillApplyAttackToSelf] = "On Kill Apply Attack To Self",
                        [VanillaEffects.OnKillApplyBlockToSelf] = "On Kill Apply Block To Self",
                        [VanillaEffects.OnKillApplyGoldToSelf] = "On Kill Apply Gold To Self",
                        [VanillaEffects.OnKillApplyScrapToAllies] = "On Kill Apply Scrap To Allies",
                        [VanillaEffects.OnKillApplyScrapToAllyBehind] = "On Kill Apply Scrap To AllyBehind",
                        [VanillaEffects.OnKillApplyScrapToRandomAlly] = "On Kill Apply Scrap To RandomAlly",
                        [VanillaEffects.OnKillApplyShellToSelf] = "On Kill Apply Shell To Self",
                        [VanillaEffects.OnKillApplyStealthToSelf] = "On Kill Apply Stealth To Self",
                        [VanillaEffects.OnKillApplyTeethToSelf] = "On Kill Apply Teeth To Self",
                        [VanillaEffects.OnKillApplyVimToAllyBehind] = "On Kill Apply Vim To AllyBehind",
                        [VanillaEffects.OnKillApplyVimToRandomAlly] = "On Kill Apply Vim To RandomAlly",
                        [VanillaEffects.OnKillApplyVimToRandomEnemy] = "On Kill Apply Vim To RandomEnemy",
                        [VanillaEffects.OnKillDraw] = "On Kill Draw",
                        [VanillaEffects.OnKillHealToSelfAndAlliesInRow] = "On Kill Heal To Self & AlliesInRow",
                        [VanillaEffects.OnKillHealToSelf] = "On Kill Heal To Self",
                        [VanillaEffects.OnKillIncreaseHealthToSelfAndAllies] =
                            "On Kill Increase Health To Self & Allies",
                        [VanillaEffects.OnTurnApplyAttackToSelf] = "On Turn Apply Attack To Self",
                        [VanillaEffects.OnTurnApplyDemonizeToRandomEnemy] = "On Turn Apply Demonize To RandomEnemy",
                        [VanillaEffects.OnTurnApplyInkToEnemies] = "On Turn Apply Ink To Enemies",
                        [VanillaEffects.OnTurnApplyInkToRandomEnemy] = "On Turn Apply Ink To RandomEnemy",
                        [VanillaEffects.OnTurnApplyJuiceToAllyBehind] = "On Turn Apply Juice To AllyBehind",
                        [VanillaEffects.OnTurnApplyOverloadToRandomEnemy] = "On Turn Apply Overload To RandomEnemy",
                        [VanillaEffects.OnTurnApplyScrapToAllyAhead] = "On Turn Apply Scrap To AllyAhead",
                        [VanillaEffects.OnTurnApplyScrapToAllyBehind] = "On Turn Apply Scrap To AllyBehind",
                        [VanillaEffects.OnTurnApplyScrapToRandomAlly] = "On Turn Apply Scrap To RandomAlly",
                        [VanillaEffects.OnTurnApplyScrapToSelf] = "On Turn Apply Scrap To Self",
                        [VanillaEffects.OnTurnApplyShellToAllies] = "On Turn Apply Shell To Allies",
                        [VanillaEffects.OnTurnApplyShellToAllyInFrontOf] = "On Turn Apply Shell To AllyInFrontOf",
                        [VanillaEffects.OnTurnApplyShellToSelf] = "On Turn Apply Shell To Self",
                        [VanillaEffects.OnTurnApplySnowToEnemies] = "On Turn Apply Snow To Enemies",
                        [VanillaEffects.OnTurnApplySpiceToAllies] = "On Turn Apply Spice To Allies",
                        [VanillaEffects.OnTurnApplySpiceToAllyBehind] = "On Turn Apply Spice To AllyBehind",
                        [VanillaEffects.OnTurnApplySpiceToAllyInFrontOf] = "On Turn Apply Spice To AllyInFrontOf",
                        [VanillaEffects.OnTurnApplyTeethToSelf] = "On Turn Apply Teeth To Self",
                        [VanillaEffects.OnTurnApplyVimToAllyBehind] = "On Turn Apply Vim To AllyBehind",
                        [VanillaEffects.OnTurnApplyVimToRandomAlly] = "On Turn Apply Vim To RandomAlly",
                        [VanillaEffects.OnTurnApplyVoidToEveryone] = "On Turn Apply Void To Everyone",
                        [VanillaEffects.OnTurnApplyVoidToRandomEnemy] = "On Turn Apply Void To RandomEnemy",
                        [VanillaEffects.OnTurnEatRandomAllyHealthAndAttackAndEffects] =
                            "On Turn Eat Random Ally (Health, Attack & Effects)",
                        [VanillaEffects.OnTurnEscapeToSelf] = "On Turn Escape To Self",
                        [VanillaEffects.OnTurnHealAllies] = "On Turn Heal Allies",
                        [VanillaEffects.OngoingIncreaseAttack] = "Ongoing Increase Attack",
                        [VanillaEffects.OngoingIncreaseEffectFactor] = "Ongoing Increase Effect Factor",
                        [VanillaEffects.OngoingIncreaseEffects] = "Ongoing Increase Effects",
                        [VanillaEffects.OngoingReduceAttack] = "Ongoing Reduce Attack",
                        [VanillaEffects.Overload] = "Overload",
                        [VanillaEffects.PreTriggerGainTempMultiHitEqualToJuice1] =
                            "Pre Trigger Gain Temp MultiHit Equal To Juice - 1",
                        [VanillaEffects.PreTriggerGainTempMultiHitEqualToScrap1] =
                            "Pre Trigger Gain Temp MultiHit Equal To Scrap - 1",
                        [VanillaEffects.PreTurnDestroyAllItemsInHand] = "Pre Turn Destroy All Items In Hand",
                        [VanillaEffects.PreTurnDestroyRandomCardInHand] = "Pre Turn Destroy Random Card In Hand",
                        [VanillaEffects.PreTurnEatAlliesInRowHealthAndAttack] =
                            "Pre Turn Eat Allies In Row (Health & Attack)",
                        [VanillaEffects.PreTurnEatRandomAllyHealthAndAttackAndEffects] =
                            "Pre Turn Eat Random Ally (Health, Attack & Effects)",
                        [VanillaEffects.PreTurnGainAttackForEachItemInHandForEachCardDestroyed] =
                            "Pre Turn Gain Attack For Each Item In Hand (For Each Card Destroyed)",
                        [VanillaEffects.PreTurnGainTempMultiHitEqualToJuice] =
                            "Pre Turn Gain Temp MultiHit Equal To Juice",
                        [VanillaEffects.PreTurnTakeJuiceFromRandomAlly] = "Pre Turn Take Juice From RandomAlly",
                        [VanillaEffects.PreTurnTakeScrapFromRandomAlly] = "Pre Turn Take Scrap From RandomAlly",
                        [VanillaEffects.Pull] = "Pull",
                        [VanillaEffects.Push] = "Push",
                        [VanillaEffects.RecycleJunk] = "Recycle Junk",
                        [VanillaEffects.RedrawCards] = "Redraw Cards",
                        [VanillaEffects.ReduceAttackEffect1] = "Reduce Attack Effect 1",
                        [VanillaEffects.ReduceAttack] = "Reduce Attack",
                        [VanillaEffects.ReduceCounter] = "Reduce Counter",
                        [VanillaEffects.ReduceEffects] = "Reduce Effects",
                        [VanillaEffects.ReduceMaxCounter] = "Reduce Max Counter",
                        [VanillaEffects.ReduceMaxHealthMustbeally] = "Reduce Max Health (Must be ally)",
                        [VanillaEffects.ReduceMaxHealth] = "Reduce Max Health",
                        [VanillaEffects.ResistShroom] = "ResistShroom",
                        [VanillaEffects.ResistSnow] = "ResistSnow",
                        [VanillaEffects.ResistSpice] = "ResistSpice",
                        [VanillaEffects.SacrificeAlly] = "Sacrifice Ally",
                        [VanillaEffects.SacrificeCardInHand] = "Sacrifice Card In Hand",
                        [VanillaEffects.SacrificeEnemy] = "Sacrifice Enemy",
                        [VanillaEffects.Scrap] = "Scrap",
                        [VanillaEffects.SetHealth] = "Set Health",
                        [VanillaEffects.SetMaxHealth] = "Set Max Health",
                        [VanillaEffects.Shell] = "Shell",
                        [VanillaEffects.Shroom] = "Shroom",
                        [VanillaEffects.Snow] = "Snow",
                        [VanillaEffects.SoulboundBossPhase2] = "SoulboundBossPhase2",
                        [VanillaEffects.Spice] = "Spice",
                        [VanillaEffects.Split] = "Split",
                        [VanillaEffects.SplitBossPhase2] = "SplitBossPhase2",
                        [VanillaEffects.Stealth] = "Stealth",
                        [VanillaEffects.SummonBeepop] = "Summon Beepop",
                        [VanillaEffects.SummonBloo] = "Summon Bloo",
                        [VanillaEffects.SummonBoBo] = "Summon BoBo",
                        [VanillaEffects.SummonBonzo] = "Summon Bonzo",
                        [VanillaEffects.SummonDregg] = "Summon Dregg",
                        [VanillaEffects.SummonEnemyLeech] = "Summon Enemy Leech",
                        [VanillaEffects.SummonEnemyPigeon] = "Summon Enemy Pigeon",
                        [VanillaEffects.SummonEnemyPopper] = "Summon Enemy Popper",
                        [VanillaEffects.SummonFallow] = "Summon Fallow",
                        [VanillaEffects.SummonGearhammer] = "Summon Gearhammer",
                        [VanillaEffects.SummonItem] = "Summon Item",
                        [VanillaEffects.SummonJunk] = "Summon Junk",
                        [VanillaEffects.SummonPlep] = "Summon Plep",
                        [VanillaEffects.SummonSunShard] = "Summon SunShard",
                        [VanillaEffects.SummonTailsFive] = "Summon TailsFive",
                        [VanillaEffects.SummonTailsFour] = "Summon TailsFour",
                        [VanillaEffects.SummonTailsOne] = "Summon TailsOne",
                        [VanillaEffects.SummonTailsThree] = "Summon TailsThree",
                        [VanillaEffects.SummonTailsTwo] = "Summon TailsTwo",
                        [VanillaEffects.SummonTigris] = "Summon Tigris",
                        [VanillaEffects.SummonUzu] = "Summon Uzu",
                        [VanillaEffects.Summoned] = "Summoned",
                        [VanillaEffects.Take100DamageWhenSoulboundUnitKilled] =
                            "Take 100 Damage When Soulbound Unit Killed",
                        [VanillaEffects.TakeHealth] = "Take Health",
                        [VanillaEffects.Teeth] = "Teeth",
                        [VanillaEffects.TemporaryAimless] = "Temporary Aimless",
                        [VanillaEffects.TemporaryBarrage] = "Temporary Barrage",
                        [VanillaEffects.TemporaryFury] = "Temporary Fury",
                        [VanillaEffects.TemporaryNoomlin] = "Temporary Noomlin",
                        [VanillaEffects.TemporaryPigheaded] = "Temporary Pigheaded",
                        [VanillaEffects.TemporarySoulbound] = "Temporary Soulbound",
                        [VanillaEffects.TemporarySummoned] = "Temporary Summoned",
                        [VanillaEffects.TemporaryUnbreakable] = "Temporary Unbreakable",
                        [VanillaEffects.TemporaryUnmovable] = "Temporary Unmovable",
                        [VanillaEffects.TriggerAgainstAndReduceUses] = "Trigger Against & Reduce Uses",
                        [VanillaEffects.TriggerAgainstDontCountAsTrigger] = "Trigger Against (Don't Count As Trigger)",
                        [VanillaEffects.TriggerAgainstAllyWhenAllyIsHit] = "Trigger Against Ally When Ally Is Hit",
                        [VanillaEffects.TriggerAgainstAttackerWhenHit] = "Trigger Against Attacker When Hit",
                        [VanillaEffects.TriggerAgainstCrownAlliesWhenDiscarded] =
                            "Trigger Against Crown Allies When Discarded",
                        [VanillaEffects.TriggerAgainstCrownAlliesWhenDrawn] = "Trigger Against Crown Allies When Drawn",
                        [VanillaEffects.TriggerAgainstRandomAllyWhenDiscarded] =
                            "Trigger Against Random Ally When Discarded",
                        [VanillaEffects.TriggerAgainstRandomAllyWhenDrawn] = "Trigger Against Random Ally When Drawn",
                        [VanillaEffects.TriggerAgainstRandomEnemy] = "Trigger Against Random Enemy",
                        [VanillaEffects.TriggerAgainstRandomUnitWhenDiscarded] =
                            "Trigger Against Random Unit When Discarded",
                        [VanillaEffects.TriggerAgainstRandomUnitWhenDrawn] = "Trigger Against Random Unit When Drawn",
                        [VanillaEffects.TriggerAgainstWhenAllyAttacks] = "Trigger Against When Ally Attacks",
                        [VanillaEffects.TriggerAgainstWhenFrostApplied] = "Trigger Against When Frost Applied",
                        [VanillaEffects.TriggerAgainstWhenSnowApplied] = "Trigger Against When Snow Applied",
                        [VanillaEffects.TriggerAgainstWhenWeaknessApplied] = "Trigger Against When Weakness Applied",
                        [VanillaEffects.TriggerAgainst] = "Trigger Against",
                        [VanillaEffects.TriggerWhenAllyAttacks] = "Trigger When Ally Attacks",
                        [VanillaEffects.TriggerWhenAllyInRowAttacks] = "Trigger When Ally In Row Attacks",
                        [VanillaEffects.TriggerWhenAllyIsHit] = "Trigger When Ally Is Hit",
                        [VanillaEffects.TriggerWhenDeployed] = "Trigger When Deployed",
                        [VanillaEffects.TriggerWhenEnemyIsKilled] = "Trigger When Enemy Is Killed",
                        [VanillaEffects.TriggerWhenJunkDestroyed] = "Trigger When Junk Destroyed",
                        [VanillaEffects.TriggerWhenRedrawHit] = "Trigger When Redraw Hit",
                        [VanillaEffects.Trigger] = "Trigger",
                        [VanillaEffects.Unmovable] = "Unmovable",
                        [VanillaEffects.Weakness] = "Weakness",
                        [VanillaEffects.WhenAllyIsHealedApplyEqualSpice] = "When Ally Is Healed Apply Equal Spice",
                        [VanillaEffects.WhenAllyIsHealedTriggerToSelf] = "When Ally Is Healed Trigger To Self",
                        [VanillaEffects.WhenAllyisHitApplyFrostToAttacker] = "When Ally is Hit Apply Frost To Attacker",
                        [VanillaEffects.WhenAllyisHitApplyShroomToAttacker] =
                            "When Ally is Hit Apply Shroom To Attacker",
                        [VanillaEffects.WhenAllyisHitApplyTeethToSelf] = "When Ally is Hit Apply Teeth To Self",
                        [VanillaEffects.WhenAllyisHitApplyVimToTarget] = "When Ally is Hit Apply Vim To Target",
                        [VanillaEffects.WhenAllyisHitHealToTarget] = "When Ally is Hit Heal To Target",
                        [VanillaEffects.WhenAllyisHitIncreaseHealthToSelf] = "When Ally is Hit Increase Health To Self",
                        [VanillaEffects.WhenAllyIsKilledApplyAttackToSelf] = "When Ally Is Killed Apply Attack To Self",
                        [VanillaEffects.WhenAllyIsKilledGainTheirAttack] = "When Ally Is Killed Gain Their Attack",
                        [VanillaEffects.WhenAllyIsKilledLoseHalfHealthAndGainAttack] =
                            "When Ally Is Killed Lose Half Health & Gain Attack",
                        [VanillaEffects.WhenAllyIsKilledTriggerToSelf] = "When Ally Is Killed Trigger To Self",
                        [VanillaEffects.WhenAllyIsSacrificedGainTheirAttack] =
                            "When Ally Is Sacrificed Gain Their Attack",
                        [VanillaEffects.WhenAllyIsSacrificedTriggerToSelf] = "When Ally Is Sacrificed Trigger To Self",
                        [VanillaEffects.WhenAnyoneTakesShroomDamageApplyAttackToSelf] =
                            "When Anyone Takes Shroom Damage Apply Attack To Self",
                        [VanillaEffects.WhenBuiltAddJunkToHand] = "When Built Add Junk To Hand",
                        [VanillaEffects.WhenBuiltApplyVimToSelf] = "When Built Apply Vim To Self",
                        [VanillaEffects.WhenCardDestroyedAndGainAttack] = "When Card Destroyed, Gain Attack",
                        [VanillaEffects.WhenCardDestroyedAndGainJuice] = "When Card Destroyed, Gain Juice",
                        [VanillaEffects.WhenCardDestroyedAndReduceCounterToSelf] =
                            "When Card Destroyed, Reduce Counter To Self",
                        [VanillaEffects.WhenConsumedAddHealthToAllies] = "When Consumed Add Health To Allies",
                        [VanillaEffects.WhenConsumedApplyOverloadToEnemies] = "When Consumed Apply Overload To Enemies",
                        [VanillaEffects.WhenDeployedAddJunkToHand] = "When Deployed Add Junk To Hand",
                        [VanillaEffects.WhenDeployedApplyBlockToSelf] = "When Deployed Apply Block To Self",
                        [VanillaEffects.WhenDeployedApplyFrenzyToSelf] = "When Deployed Apply Frenzy To Self",
                        [VanillaEffects.WhenDeployedApplyInkToAllies] = "When Deployed Apply Ink To Allies",
                        [VanillaEffects.WhenDeployedApplyInkToEnemiesInRow] = "When Deployed Apply Ink To EnemiesInRow",
                        [VanillaEffects.WhenDeployedCopyEffectsOfRandomEnemy] =
                            "When Deployed Copy Effects Of RandomEnemy",
                        [VanillaEffects.WhenDeployedFillBoardFinalBoss] = "When Deployed Fill Board (Final Boss)",
                        [VanillaEffects.WhenDeployedSummonWowee] = "When Deployed Summon Wowee",
                        [VanillaEffects.WhenDestroyedApplyDamageToAlliesInRow] =
                            "When Destroyed Apply Damage To AlliesInRow",
                        [VanillaEffects.WhenDestroyedApplyDamageToAttacker] = "When Destroyed Apply Damage To Attacker",
                        [VanillaEffects.WhenDestroyedApplyDamageToEnemiesEqualToJuice] =
                            "When Destroyed Apply Damage To Enemies Equal To Juice",
                        [VanillaEffects.WhenDestroyedApplyDamageToEnemiesInRow] =
                            "When Destroyed Apply Damage To EnemiesInRow",
                        [VanillaEffects.WhenDestroyedApplyFrenzyToRandomAlly] =
                            "When Destroyed Apply Frenzy To RandomAlly",
                        [VanillaEffects.WhenDestroyedApplyHazeToAttacker] = "When Destroyed Apply Haze To Attacker",
                        [VanillaEffects.WhenDestroyedApplyOverloadToAttacker] =
                            "When Destroyed Apply Overload To Attacker",
                        [VanillaEffects.WhenDestroyedApplySpiceToAllies] = "When Destroyed Apply Spice To Allies",
                        [VanillaEffects.WhenDestroyedApplyStealthToAlliesInRow] =
                            "When Destroyed Apply Stealth To AlliesInRow",
                        [VanillaEffects.WhenDestroyedSummonDregg] = "When Destroyed Summon Dregg",
                        [VanillaEffects.WhenDestroyedTriggerToAllies] = "When Destroyed Trigger To Allies",
                        [VanillaEffects.WhenDrawnApplySnowToAllies] = "When Drawn Apply Snow To Allies",
                        [VanillaEffects.WhenEnemiesAttackApplyDemonizeToAttacker] =
                            "When Enemies Attack Apply Demonize To Attacker",
                        [VanillaEffects.WhenEnemyShroomedIsKilledApplyTheirShroomToRandomEnemy] =
                            "When Enemy (Shroomed) Is Killed Apply Their Shroom To RandomEnemy",
                        [VanillaEffects.WhenEnemyDeployedCopyEffectsOfTarget] =
                            "When Enemy Deployed Copy Effects Of Target",
                        [VanillaEffects.WhenEnemyIsKilledApplyGoldToSelf] = "When Enemy Is Killed Apply Gold To Self",
                        [VanillaEffects.WhenEnemyIsKilledApplyShellToAttacker] =
                            "When Enemy Is Killed Apply Shell To Attacker",
                        [VanillaEffects.WhenHealedApplyAttackToSelf] = "When Healed Apply Attack To Self",
                        [VanillaEffects.WhenHealedTriggerToSelf] = "When Healed Trigger To Self",
                        [VanillaEffects.WhenHealthLostApplyEqualAttackToSelfAndAllies] =
                            "When Health Lost Apply Equal Attack To Self And Allies",
                        [VanillaEffects.WhenHealthLostApplyEqualFrostToSelf] =
                            "When Health Lost Apply Equal Frost To Self",
                        [VanillaEffects.WhenHealthLostApplyEqualSpiceToSelf] =
                            "When Health Lost Apply Equal Spice To Self",
                        [VanillaEffects.WhenHitAddFrenzyToSelf] = "When Hit Add Frenzy To Self",
                        [VanillaEffects.WhenHitAddGearhammerToHand] = "When Hit Add Gearhammer To Hand",
                        [VanillaEffects.WhenHitAddHealthLostToAttacker] = "When Hit Add Health Lost To Attacker",
                        [VanillaEffects.WhenHitAddHealthLostToRandomAlly] = "When Hit Add Health Lost To RandomAlly",
                        [VanillaEffects.WhenHitAddJunkToHand] = "When Hit Add Junk To Hand",
                        [VanillaEffects.WhenHitApplyBlockToRandomAlly] = "When Hit Apply Block To RandomAlly",
                        [VanillaEffects.WhenHitApplyDemonizeToAttacker] = "When Hit Apply Demonize To Attacker",
                        [VanillaEffects.WhenHitApplyFrostToEnemies] = "When Hit Apply Frost To Enemies",
                        [VanillaEffects.WhenHitApplyFrostToRandomEnemy] = "When Hit Apply Frost To RandomEnemy",
                        [VanillaEffects.WhenHitApplyGoldToAttackerNoPing] = "When Hit Apply Gold To Attacker (No Ping)",
                        [VanillaEffects.WhenHitApplyInkToAttacker] = "When Hit Apply Ink To Attacker",
                        [VanillaEffects.WhenHitApplyInkToRandomEnemy] = "When Hit Apply Ink To RandomEnemy",
                        [VanillaEffects.WhenHitApplyInkToSelf] = "When Hit Apply Ink To Self",
                        [VanillaEffects.WhenHitApplyOverloadToAttacker] = "When Hit Apply Overload To Attacker",
                        [VanillaEffects.WhenHitApplyShellToAllies] = "When Hit Apply Shell To Allies",
                        [VanillaEffects.WhenHitApplyShellToAllyBehind] = "When Hit Apply Shell To AllyBehind",
                        [VanillaEffects.WhenHitApplyShellToSelf] = "When Hit Apply Shell To Self",
                        [VanillaEffects.WhenHitApplyShroomToAttacker] = "When Hit Apply Shroom To Attacker",
                        [VanillaEffects.WhenHitApplyShroomToRandomEnemy] = "When Hit Apply Shroom To RandomEnemy",
                        [VanillaEffects.WhenHitApplySnowToAttacker] = "When Hit Apply Snow To Attacker",
                        [VanillaEffects.WhenHitApplySnowToEnemies] = "When Hit Apply Snow To Enemies",
                        [VanillaEffects.WhenHitApplySnowToRandomEnemy] = "When Hit Apply Snow To RandomEnemy",
                        [VanillaEffects.WhenHitApplySpiceToAlliesAndEnemiesAndSelf] =
                            "When Hit Apply Spice To Allies & Enemies & Self",
                        [VanillaEffects.WhenHitApplySpiceToAllies] = "When Hit Apply Spice To Allies",
                        [VanillaEffects.WhenHitApplySpiceToAlliesInRow] = "When Hit Apply Spice To AlliesInRow",
                        [VanillaEffects.WhenHitApplySpiceToSelf] = "When Hit Apply Spice To Self",
                        [VanillaEffects.WhenHitApplyStealthToSelf] = "When Hit Apply Stealth To Self",
                        [VanillaEffects.WhenHitApplyVimToSelf] = "When Hit Apply Vim To Self",
                        [VanillaEffects.WhenHitApplyVoidToAttacker] = "When Hit Apply Void To Attacker",
                        [VanillaEffects.WhenHitApplyWeaknessToAttacker] = "When Hit Apply Weakness To Attacker",
                        [VanillaEffects.WhenHitDamageToEnemies] = "When Hit Damage To Enemies",
                        [VanillaEffects.WhenHitDamageToEnemiesInRow] = "When Hit Damage To EnemiesInRow",
                        [VanillaEffects.WhenHitDraw] = "When Hit Draw",
                        [VanillaEffects.WhenHitEqualDamageToAttacker] = "When Hit Equal Damage To Attacker",
                        [VanillaEffects.WhenHitGainAttackToSelfNoPing] = "When Hit Gain Attack To Self (No Ping)",
                        [VanillaEffects.WhenHitGainTeethToSelf] = "When Hit Gain Teeth To Self",
                        [VanillaEffects.WhenHitIncreaseAttackEffect1ToSelf] =
                            "When Hit Increase Attack Effect 1 To Self",
                        [VanillaEffects.WhenHitIncreaseAttackToRandomAlly] = "When Hit Increase Attack To RandomAlly",
                        [VanillaEffects.WhenHitIncreaseHealthToRandomAlly] = "When Hit Increase Health To RandomAlly",
                        [VanillaEffects.WhenHitReduceAttackToAttacker] = "When Hit Reduce Attack To Attacker",
                        [VanillaEffects.WhenHitReduceAttackToSelf] = "When Hit Reduce Attack To Self",
                        [VanillaEffects.WhenHitReduceCounterToSelf] = "When Hit Reduce Counter To Self",
                        [VanillaEffects.WhenHitTriggerToSelf] = "When Hit Trigger To Self",
                        [VanillaEffects.WhenHitWithJunkAddFrenzyToSelf] = "When Hit With Junk Add Frenzy To Self",
                        [VanillaEffects.WhenJuiceAppliedToSelfGainFrenzy] = "When Juice Applied To Self Gain Frenzy",
                        [VanillaEffects.WhenSacrificedSummonTailsFour] = "When Sacrificed Summon TailsFour",
                        [VanillaEffects.WhenSacrificedSummonTailsOne] = "When Sacrificed Summon TailsOne",
                        [VanillaEffects.WhenSacrificedSummonTailsThree] = "When Sacrificed Summon TailsThree",
                        [VanillaEffects.WhenSacrificedSummonTailsTwo] = "When Sacrificed Summon TailsTwo",
                        [VanillaEffects.WhenShellAppliedToSelfGainSpiceInstead] =
                            "When Shell Applied To Self Gain Spice Instead",
                        [VanillaEffects.WhenShroomAppliedToAnythingDoubleAmountAndLoseScrap] =
                            "When Shroom Applied To Anything Double Amount And Lose Scrap",
                        [VanillaEffects.WhenShroomDamageTakenTriggerToSelf] =
                            "When Shroom Damage Taken Trigger To Self",
                        [VanillaEffects.WhenSnowAppliedToAnythingGainAttackToSelf] =
                            "When Snow Applied To Anything Gain Attack To Self",
                        [VanillaEffects.WhenSnowAppliedToAnythingGainEqualAttackToSelf] =
                            "When Snow Applied To Anything Gain Equal Attack To Self",
                        [VanillaEffects.WhenSnowAppliedToSelfApplyDemonizeToEnemies] =
                            "When Snow Applied To Self Apply Demonize To Enemies",
                        [VanillaEffects.WhenSnowAppliedToSelfGainEqualAttack] =
                            "When Snow Applied To Self Gain Equal Attack",
                        [VanillaEffects.WhenSpiceXAppliedToSelfTriggerToSelf] =
                            "When Spice X Applied To Self Trigger To Self",
                        [VanillaEffects.WhenVimAppliedToAnythingDoubleAmount] =
                            "When Vim Applied To Anything Double Amount",
                        [VanillaEffects.WhenXHealthLostSplit] = "When X Health Lost Split",
                        [VanillaEffects.WhileActiveAddEqualAttackToJunkInHand] =
                            "While Active Add Equal Attack To Junk In Hand",
                        [VanillaEffects.WhileActiveAimlessToEnemies] = "While Active Aimless To Enemies",
                        [VanillaEffects.WhileActiveBarrageToAllies] = "While Active Barrage To Allies",
                        [VanillaEffects.WhileActiveBarrageToAlliesInRow] = "While Active Barrage To AlliesInRow",
                        [VanillaEffects.WhileActiveBarrageToEnemies] = "While Active Barrage To Enemies",
                        [VanillaEffects.WhileActiveFrenzyToAllies] = "While Active Frenzy To Allies",
                        [VanillaEffects.WhileActiveFrenzyToCrownAllies] = "While Active Frenzy To Crown Allies",
                        [VanillaEffects.WhileActiveHaltSpiceToAllies] = "While Active Halt Spice To Allies",
                        [VanillaEffects.WhileActiveIncreaseAllSpiceApplied] = "While Active Increase All Spice Applied",
                        [VanillaEffects.WhileActiveIncreaseAttackbyCurrentToAllies] =
                            "While Active Increase Attack by Current To Allies",
                        [VanillaEffects.WhileActiveIncreaseAttackbyCurrentToSummonedAllies] =
                            "While Active Increase Attack by Current To Summoned Allies",
                        [VanillaEffects.WhileActiveIncreaseAttackToAlliesAndEnemies] =
                            "While Active Increase Attack To Allies & Enemies",
                        [VanillaEffects.WhileActiveIncreaseAttackToAlliesNoDesc] =
                            "While Active Increase Attack To Allies (No Desc)",
                        [VanillaEffects.WhileActiveIncreaseAttackToAllies] = "While Active Increase Attack To Allies",
                        [VanillaEffects.WhileActiveIncreaseAttackToAlliesInRow] =
                            "While Active Increase Attack To AlliesInRow",
                        [VanillaEffects.WhileActiveIncreaseAttackToItemsInHand] =
                            "While Active Increase Attack To Items In Hand",
                        [VanillaEffects.WhileActiveIncreaseAttackToJunkInHand] =
                            "While Active Increase Attack To Junk In Hand",
                        [VanillaEffects.WhileActiveIncreaseEffectsToAlliesAndEnemies] =
                            "While Active Increase Effects To Allies & Enemies",
                        [VanillaEffects.WhileActiveIncreaseEffectsToFrontAlly] =
                            "While Active Increase Effects To FrontAlly",
                        [VanillaEffects.WhileActiveIncreaseEffectsToHand] = "While Active Increase Effects To Hand",
                        [VanillaEffects.WhileActivePigheadedToEnemies] = "While Active Pigheaded To Enemies",
                        [VanillaEffects.WhileActiveReduceAttackToEnemiesNoPingAndNoDesc] =
                            "While Active Reduce Attack To Enemies (No Ping, No Desc)",
                        [VanillaEffects.WhileActiveSnowImmuneToAllies] = "While Active Snow Immune To Allies",
                        [VanillaEffects.WhileActiveTeethToAllies] = "While Active Teeth To Allies",
                        [VanillaEffects.WhileActiveUnmovableToEnemies] = "While Active Unmovable To Enemies",
                        [VanillaEffects.WhileInHandReduceAttackToAllies] = "While In Hand Reduce Attack To Allies",
                        [VanillaEffects.WhileLastInHandDoubleEffectsToSelf] =
                            "While Last In Hand Double Effects To Self",
                    };

            public struct EffectData
            {
                public string EffectName = "";
                public int EffectAmount = 1;

                public EffectData()
                {
                }
                public EffectData(string name="",int amount=1)
                {
                    EffectName = name;
                    EffectAmount = amount;
                }
            }

            public CardBuilder AddStartWithEffects(params EffectData[] Effects)
            {
                var dictionary = AddressableLoader.groups["StatusEffectData"].lookup;
                var listTest = data.startWithEffects.ToList();
                foreach (var effect in Effects)
                {
                    try
                    {
                        listTest.Add(new CardData.StatusEffectStacks(
                            dictionary[effect.EffectName].Cast<StatusEffectData>(), effect.EffectAmount));
                    }
                    catch (KeyNotFoundException e)
                    {
                        Mod.Instance.LoggerInstance.Warning(
                            $"Tried to add effect with name {effect} but it doesnt exist");
                    }
                }

                data.startWithEffects =
                    new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<CardData.StatusEffectStacks>(
                        listTest.ToArray());
                return this;
            }


            public CardBuilder AddAttackEffects(params EffectData[] Effects)
            {
                var dictionary = AddressableLoader.groups["StatusEffectData"].lookup;
                var listTest = data.attackEffects.ToList();

                foreach (var effect in Effects)
                {
                    try
                    {
                        listTest.Add(new CardData.StatusEffectStacks(
                            dictionary[effect.EffectName].Cast<StatusEffectData>(), effect.EffectAmount));
                    }
                    catch (KeyNotFoundException e)
                    {
                        Mod.Instance.LoggerInstance.Warning(
                            $"Tried to add effect with name {effect.EffectName} but it doesnt exist");
                    }
                }

                data.attackEffects =
                    new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<CardData.StatusEffectStacks>(
                        listTest.ToArray());
                return this;
            }

            public struct TraitData
            {
                public string TraitName = "";
                public int TraitAmount = 1;

                public TraitData()
                {
                }
                public TraitData(string name="",int amount=1)
                {
                    TraitName = name;
                    TraitAmount = amount;
                }
            }

            public CardBuilder AddTraits(params TraitData[] Traits)
            {
                var dictionary = AddressableLoader.groups["TraitData"].lookup;
                foreach (var trait in Traits)
                {
                    try
                    {
                        var newTrait = new CardData.TraitStacks();
                        newTrait.count = trait.TraitAmount;
                        newTrait.data = dictionary[trait.TraitName].Cast<Il2Cpp.TraitData>();
                        data.traits.Add(newTrait);
                    }
                    catch (KeyNotFoundException e)
                    {
                        Mod.Instance.LoggerInstance.Warning(
                            $"Tried to add trait with name {trait} but it doesnt exist");
                    }
                }

                return this;
            }

            public CardBuilder SetIsItem(bool value = false, int uses = 1)
            {
                if (value) data.playType = Card.PlayType.Play;
                else data.playType = Card.PlayType.Place;
                if (data.IsItem)
                {
                    data.uses = uses;
                    data.canBeHit = false;
                    data.cardType = AddressableLoader.groups["CardType"].lookup["Item"].Cast<CardType>();
                }

                return this;
            }

            public CardBuilder SetStats(int? health = null, int? damage = null, int counter = 0)
            {
                if (health.HasValue)
                {
                    data.hp = health.Value;
                }
                else
                {
                    data.hasHealth = false;
                    data.hp = 0;
                }

                if (damage.HasValue)
                {
                    data.damage = damage.Value;
                }
                else
                {
                    data.hasAttack = false;
                    data.damage = 0;
                }

                data.counter = counter;
                return this;
            }
        }

        public static event Func<List<CardBuilder>, List<CardBuilder>> AddCards;
        public static List<string> AddedCards;


        internal static void InvokeEvents(ref List<CardBuilder> cardBuilders)
        {
            if (AddCards != null) cardBuilders = AddCards(cardBuilders);

            /*
            string resultString = "";
            string allEnumNames = "";
            foreach (var effect in AddressableLoader.groups["TraitData"].lookup)
            {
                var name = effect.key;
                var enumerizedName = effect.key.Replace(" ", "").Replace("&", "And").Replace("(", "").Replace(")", "")
                    .Replace("-", "").Replace(",", "And").Replace(".", "").Replace("\'", "");
                resultString += $"[VanillaTraits.{enumerizedName}]=\"{name}\",\n";
                allEnumNames += enumerizedName + ",\n";
            }

            Mod.Instance.LoggerInstance.Warning(resultString);
            Mod.Instance.LoggerInstance.Msg("Enums incoming!");
            Mod.Instance.LoggerInstance.Warning(allEnumNames);
            */
        }

        internal static void AddAllCards(List<CardBuilder> cardBuilders)
        {
            var loc = typeof(Mod).Assembly.Location.Replace("WildfrostModMiya.dll", "");
            var jsonCardsLoc = loc + "JsonCards\\";
            Mod.Instance.LoggerInstance.Msg(JSON.Dump(new Mod.JSONCardData(){AttackEffects =new []{new CardBuilder.EffectData(CardBuilder.VanillaEffects.Frost.VanillaEffectName(),1)}}));
            foreach (var jsonFile in Directory.EnumerateFiles(jsonCardsLoc, "*.json"))
            {
                Mod.Instance.LoggerInstance.Warning("Will add "+jsonFile);
                Mod.JSONCardData data= File.ReadAllText((jsonFile)).FromJson<Mod.JSONCardData>();
                var builder = new WildFrostAPI.CardBuilder()
                    .SetTitle(data.Title)
                    .SetStats(data.Health, data.Damage, data.Counter)
                    .AddAttackEffects(data.AttackEffects);
               
                cardBuilders.Add(builder
                );
            }
            Mod.Instance.LoggerInstance.Warning("Added all json cards, now other cards up to add");
            foreach (var builder in cardBuilders)
            {
                var card = builder.data;
                card.id = (ulong)AddressableLoader.groups["CardData"].list.Count;
                Mod.Instance.LoggerInstance.Warning($"Added card {card.forceTitle}!");
                AddressableLoader.groups["CardData"].list.Add(card);
            }
        }
    }

    public class Mod : MelonMod
    {
        internal static Mod Instance;

        public struct JSONCardData
        {
            public string Title;
            public int? Damage;
            public int? Health;
            public int Counter;
            public WildFrostAPI.CardBuilder.EffectData[] AttackEffects;
        }
        public override void OnInitializeMelon()
        {
            Instance = this;
            LoggerInstance.Msg("Hi from miyas mod!");
            base.OnInitializeMelon();

            WildFrostAPI.AddCards += delegate(List<WildFrostAPI.CardBuilder> list)
            {

                list.Add(new WildFrostAPI.CardBuilder()
                    .SetTitle("NewCard")
                    .SetStats(damage: 1)
                    .SetIsItem(true)
                    .AddAttackEffects(new WildFrostAPI.CardBuilder.EffectData("Demonize", 1))
                    .AddTraits(new WildFrostAPI.CardBuilder.TraitData("Noomlin", 1)));
           

                /*  A card user DJ Rose#6020 suggested to make for testing purposes
                list.Add(new WildFrostAPI.CardBuilder()
                    .SetTitle("DJ Rose card")
                    .SetStats(5, 1, 3)
                    .AddTraits(new WildFrostAPI.CardBuilder.TraitData("Smackback"))
                    .AddTraits(new WildFrostAPI.CardBuilder.TraitData("Barrage"))
                    .AddAttackEffects(
                        new WildFrostAPI.CardBuilder.EffectData("Frost", 5))
                );
                */

                return list;
            };
        }


        public override void OnUpdate()
        {
            if (AddressableLoader.IsGroupLoaded("CardData") && AddressableLoader.IsGroupLoaded("CardType") &&
                AddressableLoader.IsGroupLoaded("StatusEffectData"))
            {
                if (!AddressableLoader.groups["CardData"].list.Contains(debug))
                {
                    AddressableLoader.groups["CardData"].list.Add(debug);
                    List<WildFrostAPI.CardBuilder> builders = new List<WildFrostAPI.CardBuilder>();
                    WildFrostAPI.InvokeEvents(ref builders);
                    WildfrostModMiya.WildFrostAPI.AddAllCards(builders);
                }
            }

            base.OnUpdate();
        }

        public static bool CardsAdded = false;

        public static CardData debug = new CardData() { forceTitle = "APIADDDED" };


        [HarmonyPatch(typeof(MetaprogressionSystem), nameof(MetaprogressionSystem.IsUnlocked),
            new[] { typeof(string), typeof(Il2CppSystem.Collections.Generic.List<string>) })]
        class IsUnlocked1
        {
            [HarmonyPostfix]
            static void Postfix(MetaprogressionSystem __instance, ref bool __result)
            {
                if (UnlockAll)

                    __result = UnlockAll;
            }
        }

        [HarmonyPatch(typeof(MetaprogressionSystem), nameof(MetaprogressionSystem.IsUnlocked),
            new[] { typeof(UnlockData), typeof(Il2CppSystem.Collections.Generic.List<string>) })]
        class IsUnlocked2
        {
            [HarmonyPostfix]
            static void Postfix(MetaprogressionSystem __instance, ref bool __result)
            {
                if (UnlockAll)

                    __result = UnlockAll;
            }
        }


        [HarmonyPatch(typeof(MetaprogressionSystem), nameof(MetaprogressionSystem.GetUnlockedList))]
        class Unlockall
        {
            [HarmonyPostfix]
            static void Postfix(MetaprogressionSystem __instance,
                ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                if (UnlockAll)
                {
                    foreach (var pet in MetaprogressionSystem.data["pets"]
                                 .Cast<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray>())
                    {
                        __result.Add(pet);
                    }
                }

                ;
            }
        }

        public static bool UnlockAll;

        public bool MatchCardName(Object o)
        {
            var card = o.Cast<CardData>();
            LoggerInstance.Warning(card.title);
            return card.title.Equals(CardName, StringComparison.OrdinalIgnoreCase) ||
                   card.forceTitle.Equals(CardName, StringComparison.OrdinalIgnoreCase);
        }

        public Rect _rect = new Rect(0, 0, 175, 300);
        public string CardName;

        public override void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.MouseDrag && _rect.Contains(e.mousePosition))
            {
                _rect.x += e.delta.x;
                _rect.y += e.delta.y;
            }

            GUI.BeginGroup(_rect, "");
            GUILayout.Label("Enter the card name: ");
            CardName = GUILayout.TextField(CardName);
            if (GUILayout.Button("Give me!") && !string.IsNullOrEmpty(CardName))
            {
                CardData card = null;
                foreach (var cardinList in AddressableLoader.groups["CardData"].list)
                {
                    if (MatchCardName(cardinList))
                    {
                        card = cardinList.Cast<CardData>();
                        break;
                    }
                }

                if (card != null)
                {
                    LoggerInstance.Msg("Gave out card " + card.title);
                    var clone = card.Clone();
                    clone.original = card;
                    Campaign.instance.characters._items[0].data.inventory.deck.Add(clone);
                    Campaign.instance.characters._items[0].data.inventory.Save();
                }
                else LoggerInstance.Msg("No such card!");
            }

            UnlockAll = GUILayout.Toggle(UnlockAll, "Unlock all?");


            GUI.EndGroup();
            base.OnGUI();
        }
    }
}