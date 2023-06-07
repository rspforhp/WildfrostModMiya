using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.Localization;

namespace WildfrostModMiya;

public static class BattleAdder
{
    public static event Action<int> OnAskForAddingBattles;

    public static BattleData CreateBattleData(string modName, string battleName)
    {
        string oldName = battleName;
        battleName=battleName.StartsWith(modName) ? battleName : $"{modName}.{battleName}";
        if (modName == "") battleName= oldName;
        BattleData newData = ScriptableObject.CreateInstance<BattleData>();
        newData.name = battleName;
        newData.sprite =  CardAdder.LoadSpriteFromCardPortraits("CardPortraits\\FALLBACKBATTLESPRITE.png");
        newData.title = battleName;
        newData.bonusUnitRange = new Vector2Int();
        newData.bonusUnitPool = new Il2CppReferenceArray<CardData>(new CardData[0]);
        newData.generationScript = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<BattleGenerationScriptWaves>()).ToList().Find(a=>a.name=="WaveBattleGenerator").Cast<BattleGenerationScriptWaves>();
        newData.goldGivers = 0;
        newData.goldGiverPool = new Il2CppReferenceArray<CardData>(new CardData[0]);
        newData.pointFactor = 1;
        newData.pools=new Il2CppReferenceArray<BattleWavePoolData>(new BattleWavePoolData[0]);
        newData.setUpScript = UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<ScriptBattleSetUp>()).ToList().Find(a=>a.name=="BattleSetUp").Cast<ScriptBattleSetUp>();
        newData.waveCounter = 0;
        return newData;
    }

    public static BattleWavePoolData CreateWave(List<List<string>> waves)
    {
        var wave = ScriptableObject.CreateInstance<BattleWavePoolData>();
        wave.maxPulls = 1;
        wave.forcePulls = 1;
        wave.weight = 1;
        List<BattleWavePoolData.Wave> wavesList = new List<BattleWavePoolData.Wave>();
        var cards = UnityEngine.Object.FindObjectsOfType<CardData>().ToList();
        foreach (var w in waves)
        {
            var waveNew = new BattleWavePoolData.Wave();
            waveNew.value = 100;
            waveNew.maxSize = w.Count;
            Il2CppSystem.Collections.Generic.List<CardData> Cards = new ();
            foreach (var card in w)
            {
                Cards.Add(cards?.Find(c => c.name == card));
            }
            waveNew.units =Cards;
                wavesList.Add(waveNew);

        }
        wave.waves = wavesList.ToArray();
        return wave;
    }
    public static BattleData SetWaves(this BattleData t,BattleWavePoolData[] pools, int waveCounter)
    {
        t.pools = new Il2CppReferenceArray<BattleWavePoolData>(pools);
        t.waveCounter = waveCounter;
        return t;
    }

    
    public static BattleData SetTitle(this BattleData t,string title)
    {
        t.title = title;
        t.nameRef = LocalizationHelper.FromId(LocalizationHelper.CreateLocalizedString(t.name + ".Title", title));
        return t;
    }

    public static BattleData RegisterBattleInApi(this BattleData t,int tier)
    {
        WildFrostAPIMod.BattleDataAdditions.Add((t,tier));
        return t;
    }
    internal static void LaunchEvent()
    {
        OnAskForAddingBattles?.Invoke(0);
    }
}