using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Recorre el flujo completo en Play Mode (Menú -> Nivel 1 -> Nivel 2 -> Menú), toma capturas y comprueba la navegación.
public static class Tarea6Capturas
{
    static IEnumerator rutina;
    static readonly System.Collections.Generic.Stack<IEnumerator> pila = new System.Collections.Generic.Stack<IEnumerator>();
    static string salida;
    static int errores;

    [MenuItem("Tarea6/Capturar")]
    public static void Capturar()
    {
        salida = System.Environment.GetEnvironmentVariable("CAPTURAS_DIR") ?? "Capturas";
        Directory.CreateDirectory(salida);
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        PlayerPrefs.DeleteKey("NivelDesbloqueado");
        Application.logMessageReceived += (c, s, t) => { if (t == LogType.Error || t == LogType.Exception) { errores++; Debug.Log("[T6] ERROR CAPTURADO: " + c); } };
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
            Debug.Log("[T6] Terminado. Errores en consola: " + errores);
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
        Debug.Log("[T6] Captura " + nombre + " en escena " + SceneManager.GetActiveScene().name);
    }

    static IEnumerator IrA(string escena, System.Action accion)
    {
        accion();
        yield return Esperar(1.0f);
        if (SceneManager.GetActiveScene().name != escena) { errores++; Debug.Log("[T6] FALLO: se esperaba " + escena + " y estamos en " + SceneManager.GetActiveScene().name); }
        else Debug.Log("[T6] OK navegación a " + escena);
    }

    static void ForzarNivelCompletado()
    {
        var cn = Object.FindFirstObjectByType<ControlNivel>();
        typeof(ControlNivel).GetMethod("CompletarNivel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(cn, null);
    }

    static IEnumerator Flujo()
    {
        yield return Esperar(1.5f);
        yield return IrA("Game", () => Object.FindFirstObjectByType<MainMenu>().Jugar());
        yield return Esperar(1.0f);
        var pelota = Object.FindFirstObjectByType<MovimientoPelota>();
        var rb = pelota.GetComponent<Rigidbody>();
        var estela = pelota.GetComponent<TrailRenderer>();
        Foto("01_Nivel1_ConBarraYOrbes");
        Debug.Log("[T6] Antes: velY=" + rb.linearVelocity.y + " estelaEmitiendo=" + estela.emitting + " tiempoEstela=" + estela.time);

        // Recoger un orbe de verdad: teletransportamos la pelota a un orbe activo y esperamos al trigger
        Potenciador orbe = null;
        foreach (var o in Object.FindObjectsByType<Potenciador>(FindObjectsSortMode.None)) { orbe = o; break; }
        Debug.Log("[T6] Orbes activos en la escena: " + Object.FindObjectsByType<Potenciador>(FindObjectsSortMode.None).Length);
        rb.position = orbe.transform.position;
        yield return Esperar(0.4f);
        Debug.Log("[T6] Tras tocar orbe: SuperVelocidadActiva=" + pelota.SuperVelocidadActiva + " orbeVisible=" + orbe.gameObject.activeSelf);
        if (!pelota.SuperVelocidadActiva || orbe.gameObject.activeSelf) { errores++; Debug.Log("[T6] FALLO: el orbe no activó la súper velocidad"); }
        yield return Esperar(1.0f);
        Debug.Log("[T6] Durante: velY=" + rb.linearVelocity.y + " estelaEmitiendo=" + estela.emitting + " estelaTime=" + estela.time);
        if (!estela.emitting || estela.time > 3f || rb.linearVelocity.y > -5f) { errores++; Debug.Log("[T6] FALLO: la estela o la velocidad no cambiaron"); }
        Foto("02_Nivel1_SuperVelocidad");
        yield return Esperar(3.2f);
        Debug.Log("[T6] Después: SuperVelocidadActiva=" + pelota.SuperVelocidadActiva + " estelaEmitiendo=" + estela.emitting);
        if (pelota.SuperVelocidadActiva || estela.emitting) { errores++; Debug.Log("[T6] FALLO: la súper velocidad no terminó"); }
        var barra = Object.FindFirstObjectByType<ControlNivel>();
        Debug.Log("[T6] Progreso barra: " + UnityEngine.Object.FindFirstObjectByType<UnityEngine.UI.Slider>().value);
        Foto("03_Nivel1_BarraAvanzada");

        // Nivel 2 también
        yield return IrA("Nivel2", () => SceneManager.LoadScene("Nivel2"));
        yield return Esperar(1.0f);
        pelota = Object.FindFirstObjectByType<MovimientoPelota>();
        pelota.ActivarSuperVelocidad();
        yield return Esperar(1.0f);
        Foto("04_Nivel2_SuperVelocidad");
    }
}
