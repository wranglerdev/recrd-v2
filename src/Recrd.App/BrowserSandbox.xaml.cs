using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using Recrd.Domain.Entities;
using Recrd.Domain.Selectors;

namespace Recrd.App;

/// <summary>
/// Browser Sandbox (PRD §10): Chromium embarcado que captura cliques, teclado,
/// navegação e URLs, e — em modo Inspect — descreve o elemento sob o mouse.
/// A captura é feita por um script injetado que devolve os atributos via
/// <c>window.chrome.webview.postMessage</c>; o host gera o seletor (PRD §11).
/// </summary>
public partial class BrowserSandbox : UserControl
{
    public event EventHandler<CapturedAction>? ActionCaptured;
    public event EventHandler<InspectedElement>? ElementInspected;

    private bool _ready;
    private bool _inspect;

    public BrowserSandbox()
    {
        InitializeComponent();
        Loaded += async (_, _) => await InitAsync();
    }

    /// <summary>Modo Inspect (PRD §10): liga a descrição do elemento sob o mouse.</summary>
    public bool InspectMode
    {
        get => _inspect;
        set
        {
            _inspect = value;
            if (_ready)
                _ = Web.CoreWebView2.ExecuteScriptAsync($"window.__recrdInspect={(value ? "true" : "false")}");
        }
    }

    public void Reload() => Web.CoreWebView2?.Reload();

    /// <summary>
    /// Drag-and-drop de massa (PRD §12): solta a variável sobre o elemento na
    /// posição dada e grava <c>{{variavel}}</c> — não o valor literal.
    /// </summary>
    public async Task DropVariableAsync(string variable, System.Windows.Point posInWeb)
    {
        if (!_ready) return;
        var tpl = "{{" + variable + "}}";
        // ponytail: posição em DIPs ≈ CSS px a 100%/zoom 1. Ajustar por DevicePixelRatio
        // se a app rodar com zoom/DPI != 100%.
        var x = posInWeb.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var y = posInWeb.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var result = await Web.CoreWebView2.ExecuteScriptAsync(
            $"window.__recrdDrop({x},{y},{JsonSerializer.Serialize(tpl)})");
        if (result is null or "null") return;

        using var doc = JsonDocument.Parse(result);
        var el = ToElementInfo(doc.RootElement);
        var selector = TryGenerate(el);
        ActionCaptured?.Invoke(this, new CapturedAction(new ScriptAction(ActionKind.Input, selector?.Value, tpl), selector));
    }

    private async void OnDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(System.Windows.DataFormats.StringFormat) is string variable)
            await DropVariableAsync(variable, e.GetPosition(Web));
    }

    public void Navigate(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!url.Contains("://")) url = "https://" + url;
        Web.Source = new Uri(url);
    }

    private async Task InitAsync()
    {
        await Web.EnsureCoreWebView2Async();
        Web.CoreWebView2.Settings.IsWebMessageEnabled = true;
        // Deixa o host WPF tratar o drop da massa em vez do Chromium (PRD §12).
        Web.AllowExternalDrop = false;
        await Web.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(CaptureScript);
        Web.CoreWebView2.WebMessageReceived += OnWebMessage;
        // Captura de navegação/URL (PRD §10): toda troca de página vira uma ação.
        Web.CoreWebView2.SourceChanged += (_, _) =>
        {
            var url = Web.Source?.ToString();
            AddressBar.Text = url;
            ActionCaptured?.Invoke(this, new CapturedAction(new ScriptAction(ActionKind.Navigate, null, url), null));
        };
        _ready = true;
        InspectMode = _inspect; // aplica estado escolhido antes do load
    }

    private void OnWebMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        using var doc = JsonDocument.Parse(e.WebMessageAsJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("el", out var elJson) || elJson.ValueKind != JsonValueKind.Object) return;

        var el = ToElementInfo(elJson);
        var selector = TryGenerate(el);

        switch (root.GetProperty("kind").GetString())
        {
            case "inspect":
                ElementInspected?.Invoke(this, new InspectedElement(el, Str(elJson, "classes"), selector));
                break;
            case "capture":
                var action = root.GetProperty("action").GetString();
                var value = root.TryGetProperty("value", out var v) ? v.GetString() : null;
                var sa = action == "input"
                    ? new ScriptAction(ActionKind.Input, selector?.Value, value)
                    : new ScriptAction(ActionKind.Click, selector?.Value);
                ActionCaptured?.Invoke(this, new CapturedAction(sa, selector));
                break;
        }
    }

    private static ElementInfo ToElementInfo(JsonElement el) => new(
        Tag: Str(el, "tag") ?? "*",
        TestId: Str(el, "testId"),
        AriaLabel: Str(el, "ariaLabel"),
        Id: Str(el, "id"),
        Name: Str(el, "name"),
        Role: Str(el, "role"),
        Text: Str(el, "text"),
        Css: Str(el, "css"));

    private static string? Str(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    // SelectorGenerator lança quando não há nada selecionável — sandbox não pode quebrar por isso.
    private static Selector? TryGenerate(ElementInfo el)
    {
        try { return SelectorGenerator.Generate(el); }
        catch { return null; }
    }

    private void Go_Click(object sender, System.Windows.RoutedEventArgs e) => Navigate(AddressBar.Text);

    private void AddressBar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Navigate(AddressBar.Text);
    }

    // Script injetado em cada documento: captura cliques/inputs e, em modo Inspect,
    // descreve o elemento sob o mouse. Atributos seguem a prioridade do PRD §11.
    private const string CaptureScript = """
        (function () {
          function info(el) {
            if (!el || !el.tagName) return null;
            var cls = (el.className && el.className.baseVal !== undefined) ? el.className.baseVal : (el.className || '');
            cls = ('' + cls).trim();
            var css = el.id ? '#' + el.id
              : el.tagName.toLowerCase() + (cls ? '.' + cls.split(/\s+/).join('.') : '');
            return {
              tag: el.tagName.toLowerCase(),
              testId: el.getAttribute('data-testid'),
              ariaLabel: el.getAttribute('aria-label'),
              id: el.id || null,
              name: el.getAttribute('name'),
              role: el.getAttribute('role'),
              text: (el.textContent || '').trim().slice(0, 40) || null,
              css: css,
              classes: cls || null
            };
          }
          document.addEventListener('click', function (e) {
            window.chrome.webview.postMessage({ kind: 'capture', action: 'click', el: info(e.target) });
          }, true);
          document.addEventListener('change', function (e) {
            var t = e.target;
            if (t && (t.tagName === 'INPUT' || t.tagName === 'TEXTAREA' || t.tagName === 'SELECT'))
              window.chrome.webview.postMessage({ kind: 'capture', action: 'input', el: info(t), value: t.value });
          }, true);
          // Expostos p/ o drag-and-drop de massa (PRD §12).
          window.__recrdInfo = info;
          window.__recrdDrop = function (x, y, tpl) {
            var el = document.elementFromPoint(x, y);
            if (!el) return null;
            if ('value' in el) { el.value = tpl; el.dispatchEvent(new Event('input', { bubbles: true })); }
            return info(el);
          };
          var last = 0;
          document.addEventListener('mousemove', function (e) {
            if (!window.__recrdInspect) return;
            var now = Date.now(); if (now - last < 60) return; last = now;
            window.chrome.webview.postMessage({ kind: 'inspect', el: info(e.target) });
          }, true);
        })();
        """;
}

/// <summary>Ação capturada na gravação + seletor sugerido (null = elemento sem âncora estável).</summary>
public sealed record CapturedAction(ScriptAction Action, Selector? Selector);

/// <summary>Elemento descrito pelo modo Inspect (PRD §10).</summary>
public sealed record InspectedElement(ElementInfo Element, string? Classes, Selector? Selector);
