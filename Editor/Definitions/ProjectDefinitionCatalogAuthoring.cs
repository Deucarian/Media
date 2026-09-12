using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.Media.Unity;
using UnityEditor;
namespace Deucarian.Media.Editor.Definitions
{
    internal static class ProjectDefinitionCatalogAuthoring
    {
        internal static void Refresh(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:MediaDefinitionAsset", new[] { "Assets" }).Select(x => AssetDatabase.LoadAssetAtPath<MediaDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x))).Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("Media definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<MediaDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/MediaDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
    }
}
