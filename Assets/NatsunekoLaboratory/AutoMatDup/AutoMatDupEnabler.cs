// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace NatsunekoLaboratory.AutoMatDup
{
    [InitializeOnLoad]
    public class AutoMatDupEnabler
    {
        private const string Path = "NatsunekoLaboratory/Automatic Duplicate Material";
        private const string PrefKey = "NatsunekoLaboratory_AutomaticDuplicateMaterial";
        private static readonly List<int> Processed = new List<int>();

        static AutoMatDupEnabler()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        [MenuItem(Path)]
        public static void Toggle()
        {
            EditorPrefs.SetBool(PrefKey, !EditorPrefs.GetBool(PrefKey, false));
        }

        [MenuItem(Path, true)]
        public static bool Validator()
        {
            Menu.SetChecked(Path, EditorPrefs.GetBool(PrefKey, false));
            return true;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selection)
        {
            if (!EditorPrefs.GetBool(PrefKey, false))
                return;
            if (Selection.activeInstanceID != instanceId)
                return;

            if (Event.current?.commandName == "Duplicate")
                EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            EditorApplication.delayCall -= OnDelayCall;

            var go = Selection.activeGameObject;
            if (go == null)
                return;

            if (Processed.Contains(go.GetInstanceID()))
                return;

            Processed.Add(go.GetInstanceID());
            DuplicateMaterialsOnMeshRenderer(go);
            DuplicateMaterialsOnSkinnedMeshRenderer(go);
        }

        private static void DuplicateMaterialsOnMeshRenderer(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>();
            var materials = renderers.SelectMany(w => w.sharedMaterials).Distinct();
            var dict = new Dictionary<Material, Material>();

            foreach (var material in materials)
            {
                var src = AssetDatabase.GetAssetPath(material);
                var dst = AssetDatabase.GenerateUniqueAssetPath(src);
                AssetDatabase.CopyAsset(src, dst);

                dict.Add(material, AssetDatabase.LoadAssetAtPath<Material>(dst));
            }

            foreach (var renderer in renderers)
            {
                var m = new Material[renderer.sharedMaterials.Length];
                for (var i = 0; i < renderer.sharedMaterials.Length; i++)
                    m[i] = dict[renderer.sharedMaterials[i]];

                renderer.sharedMaterials = m;
            }

            AssetDatabase.SaveAssets();
        }

        private static void DuplicateMaterialsOnSkinnedMeshRenderer(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            var materials = renderers.SelectMany(w => w.sharedMaterials).Distinct();
            var dict = new Dictionary<Material, Material>();

            foreach (var material in materials)
            {
                var src = AssetDatabase.GetAssetPath(material);
                var dst = AssetDatabase.GenerateUniqueAssetPath(src);
                AssetDatabase.CopyAsset(src, dst);

                dict.Add(material, AssetDatabase.LoadAssetAtPath<Material>(dst));
            }

            foreach (var renderer in renderers)
            {
                var m = new Material[renderer.sharedMaterials.Length];
                for (var i = 0; i < renderer.sharedMaterials.Length; i++)
                    m[i] = dict[renderer.sharedMaterials[i]];

                renderer.sharedMaterials = m;
            }

            AssetDatabase.SaveAssets();
        }
    }
}