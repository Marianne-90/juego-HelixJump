using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Recorre el flujo completo en Play Mode (Menú -> Nivel 1 -> Nivel 2 -> Menú), toma capturas y comprueba la navegación.
public static class Tarea5Capturas
{
    static IEnumerator rutina;
    static readonly System.Collections.Generic.Stack<IEnumerator> pila = new System.Collections.Generic.Stack<IEnumerator>();
    static string salida;
    static int errores;

    [MenuItem("Tarea5/Capturar")]
    public static void Capturar()
    {
        salida = System.Environment.GetEnvironmentVariable("CAPTURAS_DIR") ?? "Capturas";
        Directory.CreateDirectory(salida);
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        PlayerPrefs.DeleteKey("NivelDesbloqueado");
        Application.logMessageReceived += (c, s, t) => { if (t == LogType.Error || t == LogType.Exception) { errores++; Debug.Log("[T5] ERROR CAPTURADO: " + c); } };
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        rutina = Flujo();
        EditorApplication.update += Paso;
        EditorApplication.EnterPlaymode();
    }

    static void Paso()
    {
        if (!EditorApplication.isPlaying) return;
        if (pila.Count == 0 && rutina != null) pila.Push(rutina);
        while (pila.Count > 0)
        {
            var actual = pila.Peek();
            if (!actual.MoveNext()) { pila.Pop(); continue; }
            if (actual.Current is IEnumerator hijo) { pila.Push(hijo); continue; }
            return;
        }
        {
            EditorApplication.update -= Paso;
            Debug.Log("[T5] Terminado. Errores en consola: " + errores);
            EditorApplication.Exit(errores == 0 ? 0 : 1);
        }
    }

    static IEnumerator Esperar(float s)
    {
        double fin = EditorApplication.timeSinceStartup + s;
        while (EditorApplication.timeSinceStartup < fin) yield return null;
    }

    static void Foto(string nombre)
    {
        var cam = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        var rt = new RenderTexture(540, 960, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(540, 960, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 540, 960), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(salida, nombre + ".png"), tex.EncodeToPNG());
        cam.targetTexture = null;
        RenderTexture.active = null;
        Debug.Log("[T5] Captura " + nombre + " en escena " + SceneManager.GetActiveScene().name);
    }

    static IEnumerator IrA(string escena, System.Action accion)
    {
        accion();
        yield return Esperar(1.0f);
        if (SceneManager.GetActiveScene().name != escena) { errores++; Debug.Log("[T5] FALLO: se esperaba " + escena + " y estamos en " + SceneManager.GetActiveScene().name); }
        else Debug.Log("[T5] OK navegación a " + escena);
    }

    static void ForzarNivelCompletado()
    {
        var cn = Object.FindFirstObjectByType<ControlNivel>();
        typeof(ControlNivel).GetMethod("CompletarNivel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cn, null);
    }

    static IEnumerator Flujo()
    {
        yield return Esperar(1.5f);
        Foto("01_MenuPrincipal_Nivel2Bloqueado");

        // Jugar -> Nivel 1
        yield return IrA("Game", () => Object.FindFirstObjectByType<MainMenu>().Jugar());
        yield return Esperar(1.0f);
        Foto("02_Nivel1_Jugando");
        Object.FindFirstObjectByType<GameManagerJuego>().Perder();
        yield return Esperar(0.3f);
        Foto("03_Nivel1_GameOver");
        Object.FindFirstObjectByType<GameManagerJuego>().Reiniciar();
        yield return Esperar(1.0f);

        // Completar nivel 1 -> panel -> Nivel 2
        ForzarNivelCompletado();
        yield return Esperar(0.3f);
        Foto("04_Nivel1_Completado");
        Debug.Log("[T5] NivelDesbloqueado = " + PlayerPrefs.GetInt("NivelDesbloqueado", 1));
        yield return IrA("Nivel2", () => Object.FindFirstObjectByType<ControlNivel>().IrAlSiguienteNivel());
        yield return Esperar(1.0f);
        Foto("05_Nivel2_Jugando");
        ForzarNivelCompletado();
        yield return Esperar(0.3f);
        Foto("06_Nivel2_JuegoCompletado");

        // Volver al menú: ahora el Nivel 2 aparece habilitado
        yield return IrA("MainMenu", () => Object.FindFirstObjectByType<ControlNivel>().VolverAlMenu());
        yield return Esperar(1.0f);
        Foto("07_MenuPrincipal_Nivel2Desbloqueado");
        yield return IrA("Nivel2", () => Object.FindFirstObjectByType<MainMenu>().JugarNivel2());
        yield return IrA("MainMenu", () => Object.FindFirstObjectByType<GameManagerJuego>().IrAlMenu());
    }
}
