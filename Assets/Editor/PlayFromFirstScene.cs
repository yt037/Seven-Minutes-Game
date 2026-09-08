using UnityEditor;
using UnityEditor.SceneManagement;

namespace SevenMinutes.EditorTools
{
    [InitializeOnLoad]
    public static class PlayFromFirstScene
    {
        private const string MenuName = "Tools/Play From First Scene";
        private const string PrefKey = "SevenMinutes.PlayFromFirstScene";

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey, false);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        static PlayFromFirstScene() => Apply();

        [MenuItem(MenuName)]
        private static void Toggle()
        {
            Enabled = !Enabled;
            Apply();
        }

        [MenuItem(MenuName, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuName, Enabled);
            return true;
        }

        private static void Apply()
        {
            if (!Enabled || EditorBuildSettings.scenes.Length == 0)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[0].path);
        }
    }
}