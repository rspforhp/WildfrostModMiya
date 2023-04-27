using UnityEngine;

namespace WildfrostModMiya;

public static class JSONApi
{
    public class JSONCardUpdateData
    {
        public string name;
    }
    public class JSONStatusEffectData
    {
        public string name;
    }
    public class JSONTraitData
    {
        public string name;
    }

    public class JSONCardData
    {
        public ulong id;

        public string name;

        public string title;

        public int hp;

        public int damage;

        public int counter;

        public Vector3 random3;

        public JSONCardUpdateData[] upgrades;

        public JSONStatusEffectData[] attackEffects;

        public JSONStatusEffectData[] startWithEffects;

        public JSONTraitData[] traits;

        public JSONStatusEffectData[] injuries;

        public Dictionary<string, object> customData;
    }
}