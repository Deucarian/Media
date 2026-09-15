using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Media.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Media.Editor.Definitions
{
    public sealed class MediaDefinitionSchema : DeucarianSerializedDefinitionSchema<MediaDefinitionAsset, MediaDefinitionSpec>
    {
        public override string Id => "media";
        public override string DisplayName => "Media";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((MediaDefinitionAsset)asset).Validate();
        }
        public override void RefreshCatalog(bool validateOnly = false) { ProjectDefinitionCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/Media/Media Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new MediaDefinitionSchema(), "NewMedia"); }
    }
}
