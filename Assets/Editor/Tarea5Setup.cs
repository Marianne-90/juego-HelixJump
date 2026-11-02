using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Automatiza la Tarea 5: panel de nivel completado, botones de menú y el segundo nivel.
public static class Tarea5Setup
{
    const string Game = "Assets/Scenes/Game.unity";
    const string Nivel2 = "Assets/Scenes/Nivel2.unity";
    const string Menu = "Assets/Scenes/MainMenu.unity";

    static T Buscar<T>(string nombre) where T : Component
    {
        foreach (var o in Resources.FindObjectsOfTypeAll<T>())
            if (o.gameObject.name == nombre && o.gameObject.scene.IsValid()) return o;
        return null;
    }

    static GameObject ClonarBoton(GameObject plantilla, Transform padre, string nombre, string texto, Vector2 pos)
    {
        var go = Object.Instantiate(plantilla, padre);
        go.name = nombre;
        var btn = go.GetComponent<Button>();
        var n = btn.onClick.GetPersistentEventCount();
        for (int i = n - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(btn.onClick, i);
        go.GetComponentInChildren<TMP_Text>().text = texto;
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        go.SetActive(true);
        return go;
    }

    [MenuItem("Tarea5/Construir")]
    public static void Construir()
    {
        // ---------- Nivel 1 (escena Game) ----------
        var sc = EditorSceneManager.OpenScene(Game);
        var canvas = Buscar<Canvas>("Canvas").transform;
        var gm = Buscar<GameManagerJuego>("GameManager");
        var panelOver = Buscar<Transform>("PanelGameOver");
        var botonReintentar = panelOver.GetComponentInChildren<Button>(true);
        var textoNivel = Buscar<TMP_Text>("TextoNivel");
        textoNivel.text = "Nivel 1";

        // Panel de nivel completado: copia del PanelGameOver con otros textos
        var panelOK = Object.Instantiate(panelOver.gameObject, canvas);
        panelOK.name = "PanelNivelCompletado";
        var textos = panelOK.GetComponentsInChildren<TMP_Text>(true);
        TMP_Text titulo = null, textoSiguiente = null;
        foreach (var t in textos)
        {
            if (t.text.Contains("GAME OVER")) titulo = t;
            else if (t.text.Contains("Reintentar")) textoSiguiente = t;
        }
        titulo.rectTransform.sizeDelta = new Vector2(1000, 120);
        titulo.rectTransform.anchoredPosition = new Vector2(0, 150);
        titulo.fontSize = 60;
        titulo.textWrappingMode = TextWrappingModes.NoWrap;
        titulo.alignment = TextAlignmentOptions.Center;
        var botonSiguiente = panelOK.GetComponentInChildren<Button>(true);
        ((RectTransform)botonSiguiente.transform).sizeDelta = new Vector2(520, 100);
        var cn = new GameObject("ControlNivel").AddComponent<ControlNivel>();

        // Botón "Menú" en ambos paneles
        ClonarBoton(botonReintentar.gameObject, panelOver, "BotonMenu", "MENÚ",
            ((RectTransform)botonReintentar.transform).anchoredPosition + new Vector2(0, -170));
        var botonMenuOK = ClonarBoton(botonSiguiente.gameObject, panelOK.transform, "BotonMenu", "MENÚ",
            ((RectTransform)botonSiguiente.transform).anchoredPosition + new Vector2(0, -170));

        // Texto de progreso "x / objetivo" bajo el puntaje
        var textoScore = Buscar<TMP_Text>("TextoScore");
        var progreso = Object.Instantiate(textoScore.gameObject, canvas);
        progreso.name = "TextoProgreso";
        var rtp = (RectTransform)progreso.transform;
        rtp.anchoredPosition = new Vector2(0, -250);
        rtp.sizeDelta = new Vector2(500, 60);
        progreso.GetComponent<TMP_Text>().fontSize = 36;
        var hs = Buscar<TMP_Text>("TextoHighScore");
        hs.rectTransform.sizeDelta = new Vector2(500, 60);

        // Cableado de ControlNivel
        var so = new SerializedObject(cn);
        so.FindProperty("scoreManager").objectReferenceValue = Buscar<ScoreManager>("GameManager");
        so.FindProperty("puntajeObjetivo").intValue = 300;
        so.FindProperty("numeroNivel").intValue = 1;
        so.FindProperty("siguienteEscena").stringValue = "Nivel2";
        so.FindProperty("panelNivelCompletado").objectReferenceValue = panelOK;
        so.FindProperty("textoTituloNivelCompletado").objectReferenceValue = titulo;
        so.FindProperty("textoBotonSiguiente").objectReferenceValue = textoSiguiente;
        so.FindProperty("textoProgreso").objectReferenceValue = progreso.GetComponent<TMP_Text>();
        so.ApplyModifiedProperties();

        // Eventos de los botones
        foreach (var t in panelOver.GetComponentsInChildren<TMP_Text>(true))
            if (t.text.Contains("GAME OVER")) { t.rectTransform.sizeDelta = new Vector2(1000, 120); t.rectTransform.anchoredPosition = new Vector2(0, 150); t.textWrappingMode = TextWrappingModes.NoWrap; }
        UnityEventTools.AddPersistentListener(botonReintentar.onClick, gm.Reiniciar);
        UnityEventTools.AddPersistentListener(panelOver.Find("BotonMenu").GetComponent<Button>().onClick, gm.IrAlMenu);
        UnityEventTools.AddPersistentListener(botonSiguiente.onClick, cn.IrAlSiguienteNivel);
        UnityEventTools.AddPersistentListener(botonMenuOK.GetComponent<Button>().onClick, cn.VolverAlMenu);
        panelOK.SetActive(false);

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        // ---------- Nivel 2: copia de Game con más dificultad y otros colores ----------
        AssetDatabase.DeleteAsset(Nivel2);
        AssetDatabase.CopyAsset(Game, Nivel2);
        sc = EditorSceneManager.OpenScene(Nivel2);
        Buscar<TMP_Text>("TextoNivel").text = "Nivel 2";

        var c2 = Buscar<ControlNivel>("ControlNivel");
        var so2 = new SerializedObject(c2);
        so2.FindProperty("puntajeObjetivo").intValue = 500;
        so2.FindProperty("numeroNivel").intValue = 2;
        so2.FindProperty("siguienteEscena").stringValue = "";
        var panel2 = Buscar<Transform>("PanelNivelCompletado");
        panel2.Find("BotonMenu").gameObject.SetActive(false);   // el botón principal ya es "MENÚ"
        so2.ApplyModifiedProperties();

        var fondo = new SerializedObject(Buscar<FondoDinamico>("Main Camera"));
        fondo.FindProperty("colorInicial").colorValue = new Color(1f, 0.75f, 0.45f);
        fondo.FindProperty("colorFinal").colorValue = new Color(0.45f, 0.08f, 0.2f);
        fondo.FindProperty("velocidadCambio").floatValue = 0.3f;
        fondo.ApplyModifiedProperties();

        var gen = new SerializedObject(Buscar<NivelGenerador>("NivelManager"));
        gen.FindProperty("distanciaVertical").floatValue = 2.2f;   // anillos más juntos
        gen.ApplyModifiedProperties();

        var mov = new SerializedObject(Buscar<MovimientoPelota>("Pelota"));
        mov.FindProperty("sensibilidadMaxima").floatValue = 0.07f;
        mov.FindProperty("velocidadMaximaCaida").floatValue = 3.5f;
        mov.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        // ---------- Menú principal ----------
        sc = EditorSceneManager.OpenScene(Menu);
        var jugar = Buscar<Button>("BtnJugar");
        var mm = Buscar<MainMenu>("MenuManager");
        var rtj = (RectTransform)jugar.transform;
        var b2 = ClonarBoton(jugar.gameObject, jugar.transform.parent, "BtnNivel2", "NIVEL 2", rtj.anchoredPosition + new Vector2(0, -90));
        var bs = ClonarBoton(jugar.gameObject, jugar.transform.parent, "BtnSalir", "SALIR", rtj.anchoredPosition + new Vector2(0, -180));
        jugar.GetComponentInChildren<TMP_Text>().text = "JUGAR";
        var escalador = Buscar<UnityEngine.UI.CanvasScaler>("Canvas");
        escalador.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1080, 1920);
        escalador.matchWidthOrHeight = 0.5f;
        int k = 0;
        foreach (var b in new[] { jugar.gameObject, b2, bs })
        {
            var r = (RectTransform)b.transform;
            r.sizeDelta = new Vector2(640, 130);
            r.anchoredPosition = new Vector2(0, 100 - 190 * k++);
            b.GetComponentInChildren<TMP_Text>().fontSize = 56;
        }
        var tit = new GameObject("Titulo", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        tit.transform.SetParent(jugar.transform.parent, false);
        tit.text = "HELIX JUMP";
        tit.fontSize = 120;
        tit.alignment = TextAlignmentOptions.Center;
        tit.color = Color.white;
        tit.textWrappingMode = TextWrappingModes.NoWrap;
        tit.rectTransform.sizeDelta = new Vector2(1000, 200);
        tit.rectTransform.anchoredPosition = new Vector2(0, 450);
        UnityEventTools.AddPersistentListener(b2.GetComponent<Button>().onClick, mm.JugarNivel2);
        UnityEventTools.AddPersistentListener(bs.GetComponent<Button>().onClick, mm.Salir);
        var somm = new SerializedObject(mm);
        somm.FindProperty("botonNivel2").objectReferenceValue = b2.GetComponent<Button>();
        somm.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(sc);
        EditorSceneManager.SaveScene(sc);

        // ---------- Build Settings ----------
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Menu, true),
            new EditorBuildSettingsScene(Game, true),
            new EditorBuildSettingsScene(Nivel2, true),
        };
        AssetDatabase.SaveAssets();
        Debug.Log("[Tarea5] Listo");
    }
}
