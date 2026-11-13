using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Producto integrador: tercer nivel, menú con selección de niveles y Player Settings de Android.
public static class IntegradorSetup
{
    const string N2 = "Assets/Scenes/Nivel2.unity", N3 = "Assets/Scenes/Nivel3.unity";

    static T Buscar<T>(string nombre) where T : Component
    {
        foreach (var o in Resources.FindObjectsOfTypeAll<T>())
            if (o.gameObject.name == nombre && o.gameObject.scene.IsValid()) return o;
        return null;
    }

    [MenuItem("Integrador/Construir")]
    public static void Construir()
    {
        // ---- Nivel 2 deja de ser el último ----
        var sc = EditorSceneManager.OpenScene(N2);
        var c2 = new SerializedObject(Buscar<ControlNivel>("ControlNivel"));
        c2.FindProperty("siguienteEscena").stringValue = "Nivel3";
        c2.ApplyModifiedProperties();
        Buscar<Transform>("PanelNivelCompletado").Find("BotonMenu").gameObject.SetActive(true);
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        // ---- Nivel 3: copia con más dificultad ----
        AssetDatabase.DeleteAsset(N3);
        AssetDatabase.CopyAsset(N2, N3);
        sc = EditorSceneManager.OpenScene(N3);
        Buscar<TMP_Text>("TextoNivel").text = "Nivel 3";
        var c3 = new SerializedObject(Buscar<ControlNivel>("ControlNivel"));
        c3.FindProperty("puntajeObjetivo").intValue = 700;
        c3.FindProperty("numeroNivel").intValue = 3;
        c3.FindProperty("siguienteEscena").stringValue = "";
        c3.ApplyModifiedProperties();
        Buscar<Transform>("PanelNivelCompletado").Find("BotonMenu").gameObject.SetActive(false);

        var f = new SerializedObject(Buscar<FondoDinamico>("Main Camera"));
        f.FindProperty("colorInicial").colorValue = new Color(0.55f, 1f, 0.8f);
        f.FindProperty("colorFinal").colorValue = new Color(0.02f, 0.2f, 0.25f);
        f.FindProperty("velocidadCambio").floatValue = 0.4f;
        f.ApplyModifiedProperties();
        var g = new SerializedObject(Buscar<NivelGenerador>("NivelManager"));
        g.FindProperty("distanciaVertical").floatValue = 1.9f;
        g.FindProperty("probabilidadPotenciador").floatValue = 0.15f;
        g.ApplyModifiedProperties();
        var m = new SerializedObject(Buscar<MovimientoPelota>("Pelota"));
        m.FindProperty("sensibilidadMaxima").floatValue = 0.09f;
        m.FindProperty("velocidadMaximaCaida").floatValue = 4.2f;
        m.FindProperty("multiplicadorVelocidad").floatValue = 3.2f;
        m.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        // ---- Menú: botón del nivel 3 ----
        sc = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var mm = Buscar<MainMenu>("MenuManager");
        var b2 = Buscar<Button>("BtnNivel2");
        var salir = Buscar<Button>("BtnSalir");
        var b3 = Object.Instantiate(b2.gameObject, b2.transform.parent);
        b3.name = "BtnNivel3";
        var btn3 = b3.GetComponent<Button>();
        for (int i = btn3.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn3.onClick, i);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn3.onClick, mm.JugarNivel3);
        b3.GetComponentInChildren<TMP_Text>().text = "NIVEL 3";
        ((RectTransform)b3.transform).anchoredPosition = ((RectTransform)b2.transform).anchoredPosition + new Vector2(0, -190);
        ((RectTransform)salir.transform).anchoredPosition = ((RectTransform)b2.transform).anchoredPosition + new Vector2(0, -380);
        var so = new SerializedObject(mm);
        so.FindProperty("botonNivel3").objectReferenceValue = btn3;
        so.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
            new EditorBuildSettingsScene(N2, true),
            new EditorBuildSettingsScene(N3, true),
        };

        // ---- Player Settings ----
        var icono = "Assets/Iconos/IconoApp.png";
        AssetDatabase.ImportAsset(icono);
        var imp = (TextureImporter)AssetImporter.GetAtPath(icono);
        imp.textureType = TextureImporterType.Default; imp.npotScale = TextureImporterNPOTScale.None; imp.isReadable = true;
        imp.SaveAndReimport();
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(icono);
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { tex }, IconKind.Any);

        PlayerSettings.companyName = "Marianne";
        PlayerSettings.productName = "Helix Jump";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.marianne.helixjump");
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, true);
        AssetDatabase.SaveAssets();
        Debug.Log("[Integrador] Listo");
    }

    [MenuItem("Integrador/Compilar APK")]
    public static void CompilarApk()
    {
        string salida = System.Environment.GetEnvironmentVariable("APK_SALIDA") ?? "Builds/HelixJump.apk";
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(salida));
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; // menos memoria
        var opciones = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Game.unity", N2, N3 },
            locationPathName = salida,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        var r = BuildPipeline.BuildPlayer(opciones);
        Debug.Log("[Integrador] Build: " + r.summary.result + " " + r.summary.totalSize + " bytes, errores=" + r.summary.totalErrors);
        EditorApplication.Exit(r.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
    }
}
