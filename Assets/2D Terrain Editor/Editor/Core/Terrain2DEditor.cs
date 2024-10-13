using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using T2D.Modules;
using UnityEditor;
using UnityEngine;

namespace T2D.Editor
{
    [CustomEditor(typeof(Terrain2D))]
    [CanEditMultipleObjects]
    public class Terrain2DEditor : UnityEditor.Editor
    {
        public static IReadOnlyDictionary<Type, Terrain2DModuleInfoAttribute> Modules
        {
            get
            {
                if (_modules == null)
                {
                    _modules = new Dictionary<Type, Terrain2DModuleInfoAttribute>();
                    foreach (var t in TypeCache.GetTypesDerivedFrom<Terrain2DModule>().Where(t => !t.IsAbstract))
                    {
                        var info = t.GetCustomAttribute(typeof(Terrain2DModuleInfoAttribute)) as Terrain2DModuleInfoAttribute ??
                                   new Terrain2DModuleInfoAttribute(t.Name, true);
                        _modules.Add(t, info);
                    }
                }
                return _modules;
            }
        }
        private static Dictionary<Type, Terrain2DModuleInfoAttribute> _modules;

        protected Terrain2D Target;
        protected SerializedObject Objects;

        void OnEnable()
        {
            Target = (Terrain2D) target;
            Objects = new SerializedObject(targets);

            Undo.undoRedoPerformed += RebuildTargets;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= RebuildTargets;
        }

        void RebuildTargets()
        {
            foreach (var t in targets.OfType<Terrain2D>())
                t.Build();
        }

        void ShowSelectModuleMenu(Action<KeyValuePair<Type, Terrain2DModuleInfoAttribute>> moduleSelected)
        {
            GenericMenu menu = new GenericMenu();

            foreach (var module in Modules)
            {
                bool disabled = !module.Value.AllowMultipleInstances && Target != null && Target.GetComponent(module.Key) != null;
                GUIContent moduleName = new GUIContent(module.Value.Name);

                if (disabled)
                    menu.AddDisabledItem(moduleName);
                else
                {
                    menu.AddItem(moduleName, false, () =>
                    {
                        moduleSelected?.Invoke(module);
                    });
                }
            }

            menu.ShowAsContext();
        }

        void AddModule(KeyValuePair<Type, Terrain2DModuleInfoAttribute> module)
        {
            GameObject targetGo = Target.gameObject;
            if (module.Value.NewGameObjectRequired)
            {
                targetGo = new GameObject(module.Value.Name);
                targetGo.transform.parent = Target.transform;
                targetGo.transform.localPosition = Vector3.zero;
                targetGo.transform.localEulerAngles = Vector3.zero;
            }

            Terrain2DModule mc = Undo.AddComponent(targetGo, module.Key) as Terrain2DModule;
            if (mc != null)
            {
                mc.Parent = Target;
                mc.Build();
            }

            if (module.Value.NewGameObjectRequired)
            {
                Undo.RegisterCreatedObjectUndo(targetGo, $"Add {module.Value.Name} Module");
                Selection.activeGameObject = targetGo;
            }
        }


        public override void OnInspectorGUI()
        {
            Objects.Update();

            if (targets.Length <= 1)
                EditorGUILayout.EditorToolbarForTarget(EditorGUIUtility.TrTempContent(" "), target);
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.PropertyField(Objects.FindProperty("_meshResolution"));
            EditorGUILayout.PropertyField(Objects.FindProperty("_textureSize"));
            EditorGUILayout.PropertyField(Objects.FindProperty("_vertexColorMode"));
            EditorGUILayout.PropertyField(Objects.FindProperty("_sortingLayer"));
            
            EditorGUILayout.Separator();
            
            GUI.enabled = false;
            EditorGUILayout.FloatField(
                new GUIContent("Path Length",
                    "Total length of 2D Terrain path. This is the sum of distances between all Path Points"),
                Target.PathLength);
            GUI.enabled = true;


            if (targets.Length <= 1)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("Modules");
                if (EditorGUILayout.DropdownButton(EditorGUIUtility.TrTempContent("Add Module"), FocusType.Keyboard))
                    ShowSelectModuleMenu(AddModule);

                EditorGUILayout.EndHorizontal();

                GUI.enabled = false;
                foreach (var m in Target.GetRegisteredModules())
                    EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent(" "), m, typeof(Terrain2DModule), false);
                GUI.enabled = true;
            }

            if (Objects.ApplyModifiedProperties())
                RebuildTargets();
        }

        
        [MenuItem("GameObject/2D Object/2D Terrain")]
        static void CreateNewTerrain2D(MenuCommand command)
        {
            var terrain2DObj = new GameObject("2D Terrain");
            var terrain2D = terrain2DObj.AddComponent<Terrain2D>();

            var fillMatGuids = AssetDatabase.FindAssets($"Terrain2D_rocky_road_fill t: Material");
            if (fillMatGuids.Any())
                terrain2D.MeshRenderer.sharedMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(fillMatGuids.First()));

            var cap = terrain2D.AddModule(Modules.FirstOrDefault(p => p.Value.Name == "Cap").Key);
            if (cap != null && cap.GetComponent<MeshRenderer>() != null)
            {
                var capMatGuids = AssetDatabase.FindAssets("Terrain2D_rocky_road_cap t: Material");
                if (capMatGuids.Any())
                    cap.GetComponent<MeshRenderer>().sharedMaterial =
                        AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(capMatGuids.First()));
            }

            terrain2D.AddModule(Modules.FirstOrDefault(p => p.Value.Name == "Edge Collider").Key);

            terrain2D.Build();
            
            GameObjectUtility.SetParentAndAlign(terrain2DObj, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(terrain2DObj, "Create " + terrain2DObj.name);
            Selection.activeObject = terrain2DObj;
        }
    }
}