using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Redbox.KioskEngine.Environment.TextToSpeech
{
  public class SpeechPart : ISpeechPart
  {
    private List<INeededMacro> _neededMacros = new List<INeededMacro>();
    private List<ITextPart> _texts = new List<ITextPart>();
    private List<ISpeechPartEvent> _events = new List<ISpeechPartEvent>();
    private bool _autoRun = true;

    public ISpeechControl SpeechControl { get; set; }

    public int KeySequenceDelay { get; set; }

    public int Sequence { get; set; }

    public string Name { get; set; }

    public string Language { get; set; }

    public bool Loop { get; set; }

    public int StartPause { get; set; }

    public int EndPause { get; set; }

    public bool AutoRun
    {
      get => this._autoRun;
      set => this._autoRun = value;
    }

    public List<INeededMacro> NeededMacros => this._neededMacros;

    public List<ITextPart> Texts => this._texts;

    public void ValidateMacros(IMacroService macroService, Dictionary<string, string> macrosFound)
    {
      foreach (INeededMacro neededMacro in this.NeededMacros)
      {
        if (macroService[neededMacro.Name] != null)
        {
          macrosFound.Add(neededMacro.Name, macroService[neededMacro.Name]);
        }
        else
        {
          macrosFound.Add(neededMacro.Name, neededMacro?.Default?.ToString());
          LogHelper.Instance.Log("TTS((> Required macro {{" + neededMacro.Name + "}} not found; using default value");
        }
      }
      if (macrosFound.Count == this.NeededMacros.Count)
        return;
      LogHelper.Instance.Log("TTS((> Required text macro lookup mismatch: requires " + this.NeededMacros.Count.ToString() + ", found only " + macrosFound.Count.ToString());
    }

    public ISpeechPart EvaluateRegularExpression(Dictionary<string, string> macrosFound)
    {
      ISpeechPart regularExpression = (ISpeechPart) null;
      if (this.RegularExpression != null)
      {
        string input = this.RegularExpression.Value;
        foreach (KeyValuePair<string, string> keyValuePair in macrosFound)
          input = input.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value);
        string expression = this.RegularExpression.Expression;
        LogHelper.Instance.Log("TTS((> regex:" + expression + "  value: " + input);
        bool flag = expression != null && new Regex(expression).IsMatch(input);
        LogHelper.Instance.Log("TTS((> regex result:" + flag.ToString());
        string s = flag ? this.RegularExpression.Success : this.RegularExpression.Failure;
        int goToSequence = 0;
        if (int.TryParse(s, out goToSequence))
        {
          ISpeechControl speechControl = this.SpeechControl;
          regularExpression = speechControl != null ? speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == goToSequence)) : (ISpeechPart) null;
        }
        else
        {
          ISpeechControl speechControl = this.SpeechControl;
          regularExpression = speechControl != null ? speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Name == this.RegularExpression.Success)) : (ISpeechPart) null;
        }
        if (regularExpression != null)
        {
          LogHelper.Instance.Log("TTS((> Skipping to Sequence: #{0} {1}", (object) regularExpression.Sequence, (object) regularExpression.Name);
        }
        else
        {
          LogHelper.Instance.Log("TTS((> Skipping to Sequence: {0}", (object) s);
          LogHelper.Instance.Log("TTS((> Unable to find to Sequence: {0}", (object) s);
        }
      }
      return regularExpression;
    }

    public void EnqueueText(
      Queue<string> queue,
      Dictionary<string, string> macrosFound,
      IViewService viewService)
    {
      if (queue == null || macrosFound == null || viewService == null)
        return;
      foreach (ITextPart text in this.Texts)
      {
        string str1 = string.Empty;
        if (text.TextAvailable != null)
        {
          bool flag = false;
          foreach (KeyValuePair<string, string> keyValuePair in macrosFound)
          {
            if (text.TextAvailable.Contains(keyValuePair.Key) && !string.IsNullOrEmpty(keyValuePair.Value))
            {
              flag = true;
              break;
            }
          }
          if (flag)
            str1 += text.Text;
        }
        else if (text.IfActorVisible != null)
        {
          IActor actor = viewService?.PeekViewFrame()?.ViewFrame is IViewFrame viewFrame ? viewFrame.Scene?.GetActor(text.IfActorVisible) : (IActor) null;
          bool flag = true;
          if (!string.IsNullOrEmpty(text.IfControlEnabled) && actor?.WPFFrameworkElement != null && actor.WPFFrameworkElement is ITextToSpeechControl frameworkElement)
            flag = frameworkElement.IsControlEnabled(text.IfControlEnabled);
          if (((actor == null || !actor.Visible ? 0 : (actor.Enabled ? 1 : 0)) & (flag ? 1 : 0)) != 0)
            str1 += text.Text;
        }
        else
          str1 += text.Text;
        foreach (KeyValuePair<string, string> keyValuePair in macrosFound)
          str1 = str1.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value);
        string str2 = str1.Replace("\r\n", " ").Replace("\t", "");
        if (str2.Length > 0)
          queue.Enqueue(str2);
      }
    }

    public Action Refresh { get; set; }

    public void Clear()
    {
      this._neededMacros.Clear();
      this._texts.Clear();
      this._events.Clear();
      this.RegularExpression?.Clear();
    }

    public IRegularExpression RegularExpression { get; set; }

    public List<ISpeechPartEvent> Events => this._events;
  }
}
