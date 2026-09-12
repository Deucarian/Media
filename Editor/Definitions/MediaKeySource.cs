using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Media.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Media.Editor.Definitions
{
    public sealed class MediaKeySource : DeucarianAssetKeySource<MediaDefinitionAsset>
    {
        public override Type KeyType => typeof(MediaKey);
        public override Type DefinitionSetAttribute => typeof(MediaKeySetAttribute);
        public override string GeneratedClassName => "ProjectMedia";
        protected override DeucarianKeyChoice ReadDefinition(MediaDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
