using UnityEngine;

namespace Test.Biomes
{
    [CreateAssetMenu(fileName = "Biome", menuName = "GameAssets/BiomeSpecification", order = 1)]
    public class BiomeSpecification : ScriptableObject
    {
        public uint BiomeId;
        public string Name;
        public int MinHeight;
        public int MaxHeight;
    }
}