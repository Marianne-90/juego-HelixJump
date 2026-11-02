using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Automatiza la Tarea 6: orbe potenciador en el prefab del anillo, estela en la pelota y barra de progreso.
public static class Tarea6Setup
{
    static T Buscar<T>(string nombre) where T : Component
    {
        foreach (var o in Resources.FindObjectsOfTypeAll<T>())
            if (o.gameObject.name == nombre && o.gameObject.scene.IsValid()) return o;
        return null;
    }

    static Material CrearMaterial(string ruta, string shader, Color color, bool emision)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null) { m = new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(m, ruta); }
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (emision) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * 2f); }
        EditorUtility.SetDirty(m);
        return m;
    }

    [MenuItem("Tarea6/Construir")]
    public static void Construir()
    {
        var matOrbe = CrearMaterial("Assets/Materials/MatPotenciador.mat", "Universal Render Pipeline/Lit", new Color(1f, 0.9f, 0.1f), true);
        var matEstela = CrearMaterial("Assets/Materials/MatEstela.mat", "Sprites/Default", Color.white, false);

        // ---- Prefab del anillo: orbe en uno de los huecos ----
        string rutaPrefab = "Assets/Prefabs/Anillo.prefab";
        var raiz = PrefabUtility.LoadPrefabContents(rutaPrefab);
        var viejo = raiz.transform.Find("Potenciador");
        if (viejo != null) Object.DestroyImmediate(viejo.gameObject);
        var orbe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orbe.name = "Potenciador";
        orbe.transform.SetParent(raiz.transform, false);
        orbe.transform.localPosition = new Vector3(-1.8f, 0.9f, 0f);   // en el hueco, sobre el anillo
        orbe.transform.localScale = Vector3.one * 0.6f;
        orbe.GetComponent<Renderer>().sharedMaterial = matOrbe;
        var col = orbe.GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.9f;                                             // zona de contacto generosa
        orbe.AddComponent<Potenciador>();
        PrefabUtility.SaveAsPrefabAsset(raiz, rutaPrefab);
        PrefabUtility.UnloadPrefabContents(raiz);

        foreach (var ruta in new[] { "Assets/Scenes/Game.unity", "Assets/Scenes/Nivel2.unity" })
        {
            var sc = EditorSceneManager.OpenScene(ruta);
            var canvas = Buscar<Canvas>("Canvas").transform;
            var pelota = Buscar<MovimientoPelota>("Pelota");

            // ---- Estela (Trail Renderer) ----
            var estela = pelota.GetComponent<TrailRenderer>();
            if (estela == null) estela = pelota.gameObject.AddComponent<TrailRenderer>();
            estela.time = 1.5f;                       // máximo permitido: 3 s
            estela.startWidth = 0.7f;
            estela.endWidth = 0f;
            estela.minVertexDistance = 0.05f;
            estela.sharedMaterial = matEstela;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.9f, 0.1f), 0f), new GradientColorKey(new Color(1f, 0.2f, 0.8f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            estela.colorGradient = g;
            estela.emitting = false;

            // ---- Aviso de súper velocidad ----
            var plantilla = Buscar<TMP_Text>("TextoNivel");
            var aviso = Object.Instantiate(plantilla.gameObject, canvas);
            aviso.name = "AvisoSuperVelocidad";
            var ta = aviso.GetComponent<TMP_Text>();
            ta.text = "¡SÚPER VELOCIDAD!";
            ta.fontSize = 60;
            ta.color = new Color(1f, 0.9f, 0.1f);
            ta.textWrappingMode = TextWrappingModes.NoWrap;
            ta.rectTransform.sizeDelta = new Vector2(900, 100);
            ta.rectTransform.anchoredPosition = new Vector2(0, -400);

            // ---- Barra de progreso (Slider) ----
            var sl = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sl.name = "BarraProgreso";
            sl.transform.SetParent(canvas, false);
            var rt = (RectTransform)sl.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(700, 40);
            rt.anchoredPosition = new Vector2(0, -320);
            var slider = sl.GetComponent<Slider>();
            slider.minValue = 0; slider.maxValue = 1; slider.value = 0;
            slider.interactable = false;
            Object.DestroyImmediate(sl.transform.Find("Handle Slide Area").gameObject);
            slider.handleRect = null;
            slider.transition = Selectable.Transition.None;
            sl.transform.Find("Background").GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            sl.transform.Find("Fill Area/Fill").GetComponent<Image>().color = new Color(1f, 0.9f, 0.1f);

            // ---- Cableado ----
            var so = new SerializedObject(pelota);
            so.FindProperty("estela").objectReferenceValue = estela;
            so.FindProperty("avisoSuperVelocidad").objectReferenceValue = aviso;
            so.FindProperty("duracionSuperVelocidad").floatValue = 3f;
            so.FindProperty("multiplicadorVelocidad").floatValue = ruta.Contains("Nivel2") ? 3f : 2.5f;
            so.ApplyModifiedProperties();

            var cn = new SerializedObject(Buscar<ControlNivel>("ControlNivel"));
            cn.FindProperty("barraProgreso").objectReferenceValue = slider;
            cn.ApplyModifiedProperties();

            var gen = new SerializedObject(Buscar<NivelGenerador>("NivelManager"));
            gen.FindProperty("probabilidadPotenciador").floatValue = ruta.Contains("Nivel2") ? 0.2f : 0.3f;
            gen.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(sc);
            EditorSceneManager.SaveScene(sc);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Tarea6] Listo");
    }
}
