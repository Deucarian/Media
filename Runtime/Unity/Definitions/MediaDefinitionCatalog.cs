using System;
using System.Linq;
using UnityEngine;
namespace Deucarian.Media.Unity
{
    public sealed class MediaDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/MediaDefinitionCatalog";
        [SerializeField] private MediaDefinitionAsset[] definitions = Array.Empty<MediaDefinitionAsset>();
        public static MediaDefinitionCatalog LoadProject() => Resources.Load<MediaDefinitionCatalog>(ResourcePath) ?? throw new InvalidOperationException("Create a Media definition in Definitions before using its catalog.");
        public MediaDefinitionAsset Get(MediaKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key), "Select a Media key in the Inspector or pass a generated key.");
            var matches = definitions.Where(x => x != null && x.Id == key.Id).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("The Media catalog needs exactly one definition for '" + key.Id + "'. Synchronize Definitions before using it.");
            matches[0].Validate(); return matches[0];
        }
    }
}
