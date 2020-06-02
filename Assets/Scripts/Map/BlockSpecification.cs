using UnityEngine;

namespace Test.Map
{
    [CreateAssetMenu(fileName = "Block", menuName = "GameAssets/Block")]
    public class BlockSpecification : ScriptableObject
    {
        public string Name;
        public Texture2D Texture;
        public bool IsSolid = true;
        public int Duribality = 10;
        public ViewType ViewType = ViewType.Block;
    }
}