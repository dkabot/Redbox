using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Redbox.KioskEngine.Environment
{
  public class TextToSpeechService : ITextToSpeechService
  {
    private const int MOUSEEVENTF_LEFTDOWN = 2;
    private const int MOUSEEVENTF_LEFTUP = 4;
    private const int MOUSEEVENTF_RIGHTDOWN = 8;
    private const int MOUSEEVENTF_RIGHTUP = 16;
    private bool m_audioDeviceConnected;
    private SpeechSynthesizer m_reader;
    private ISpeechControl _speechControl;
    private ISpeechPart _currentSpeechPart;
    private int m_speechRate;
    private int m_speechVolume;
    private object m_speechSynthesizerLock = new object();
    private Dictionary<string, Delegate> m_keyHandlers = new Dictionary<string, Delegate>();
    private Dictionary<string, string> m_macrosFound = new Dictionary<string, string>();
    private Queue<string> m_textQueue;
    private string m_keySequence;
    private int m_keySequenceDelay;
    private Dictionary<string, string> m_textReplacement = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);

    public static TextToSpeechService Instance => Singleton<TextToSpeechService>.Instance;

    public event AudioDeviceConnectionChanged OnAudioDeviceConnectionChanged;

    public void ClearOnAudioDeviceConnectionChanged()
    {
      this.OnAudioDeviceConnectionChanged = (AudioDeviceConnectionChanged) null;
    }

    public void Reset()
    {
      LogHelper.Instance.Log("TTS((> Reset Text-To-Speech service.");
      ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
      if (service != null)
      {
        service.RemoveTimer("TTSTimer");
        service.RemoveTimer("TTSInputTimer");
      }
      this.SilenceTTS();
      this.m_keyHandlers.Clear();
      this.m_speechRate = 0;
      this.m_speechVolume = this.GetTextToSpeechVolume;
    }

    public bool AudioDeviceConnected
    {
      get => this.m_audioDeviceConnected;
      set
      {
        if (this.m_audioDeviceConnected == value)
          return;
        this.m_audioDeviceConnected = value;
        this._speechControl = (ISpeechControl) null;
        this._currentSpeechPart = (ISpeechPart) null;
        if (!this.m_audioDeviceConnected)
        {
          LogHelper.Instance.Log("TTS((> Headphones disconnected.");
          this.Reset();
          this.FireOnAudioDeviceConnectionChanged(false);
        }
        else
        {
          LogHelper.Instance.Log("TTS((> Headphones connected.");
          this.FireOnAudioDeviceConnectionChanged(true);
        }
      }
    }

    public void ClearAudioDeviceConnected()
    {
      this.m_audioDeviceConnected = false;
      this._speechControl = (ISpeechControl) null;
      this._currentSpeechPart = (ISpeechPart) null;
      LogHelper.Instance.Log("TTS((> ClearAudioDeviceConnected");
      this.Reset();
    }

    public bool TTSEnabled => this.AudioDeviceConnected;

    public int GetTextToSpeechVolume
    {
      get
      {
        IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
        int textToSpeechVolume = service != null ? service.TextToSpeechVolume : 100;
        if (textToSpeechVolume < 0)
          textToSpeechVolume = 0;
        if (textToSpeechVolume > 100)
          textToSpeechVolume = 100;
        return textToSpeechVolume;
      }
    }

    public int GetTimeout(string timeoutType, int defaultTimeout)
    {
      if (!this.TTSEnabled)
        return defaultTimeout;
      switch (timeoutType)
      {
        case "shorter":
          return 15000;
        case "short":
          return 30000;
        case "medium":
          return 60000;
        case "long":
          return 180000;
        case "help":
          return 480000;
        default:
          return 30000;
      }
    }

        public void RunSpeechWorkflow(string resourceName)
        {
            this.m_keyHandlers.Clear();
            if (this._speechControl != null)
            {
                LogHelper instance = LogHelper.Instance;
                object[] objArray = new object[3]
                {
            (object) this._speechControl?.Name,
            null,
            null
                };
                ISpeechPart currentSpeechPart = this._currentSpeechPart;
                objArray[1] = (object)(currentSpeechPart != null ? currentSpeechPart.Sequence : 0);
                objArray[2] = (object)this._currentSpeechPart?.Name;
                instance.Log("TTS((> stop existing speech workflow {0} #{1} {2}", objArray);
                ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
                if (service != null)
                {
                    service.RemoveTimer("TTSTimer");
                    service.RemoveTimer("TTSInputTimer");
                }
                this.SilenceTTS();
                this.m_keyHandlers.Clear();
            }
            LogHelper.Instance.Log("TTS((> Run speech workflow for " + resourceName);
            this._speechControl = (ISpeechControl)this.LoadSpeechControlFromResouce(resourceName);
            if (this._speechControl == null)
            {
                LogHelper.Instance.Log("TTS((> Attempting to get Speech Control for {0} from the view.", (object)resourceName);

                // Declare textToSpeechControl here to ensure it's accessible in the entire block
                ITextToSpeechControl textToSpeechControl = null;

                IViewFrameInstance viewFrameInstance = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();

                // Ensure viewFrame is assigned before using it
                IViewFrame viewFrame = viewFrameInstance?.ViewFrame as IViewFrame;
                if (viewFrame != null && viewFrame.Scene != null)
                {
                    foreach (IActor actor in viewFrame.Scene.Actors)
                    {
                        if (actor?.WPFFrameworkElement is ITextToSpeechControl ttsControl)
                        {
                            textToSpeechControl = ttsControl;
                            break;
                        }
                    }
                }
                else if (viewFrameInstance?.ViewFrame is IWPFViewFrame)
                {
                    textToSpeechControl = ServiceLocator.Instance.GetService<IRenderingService>()
                        .GetScene("Default")?.GetWPFGridChild((object)viewFrameInstance?.Id) as ITextToSpeechControl;
                }

                if (textToSpeechControl != null)
                {
                    this._speechControl = textToSpeechControl.GetSpeechControl();
                    LogHelper.Instance.Log("TTS((> Loaded Speech Control for {0} from the view. {1} speech parts loaded",
                        (object)resourceName, (object)this._speechControl?.SpeechParts?.Count);
                }
            }
            if (this._speechControl == null)
                return;
            ISpeechPart speechPart = this._speechControl.SpeechParts
                .FirstOrDefault<ISpeechPart>((x => x.Sequence == this._speechControl.SpeechParts.Min(y => y.Sequence)));

            if (speechPart.AutoRun)
            {
                this.RunSpeechPart(speechPart);
            }
            else
            {
                this._currentSpeechPart = speechPart;
                LogHelper.Instance.Log("TTS((> AutoRun is false. Setting current speech part to {0} #{1} {2} but not running it.",
                    (object)this._currentSpeechPart?.SpeechControl?.Name,
                    (object)this._currentSpeechPart.Sequence.ToString(),
                    (object)this._currentSpeechPart?.Name);
            }
        }


        private void FireOnAudioDeviceConnectionChanged(bool isConnected)
    {
      if (this.OnAudioDeviceConnectionChanged == null)
        return;
      this.OnAudioDeviceConnectionChanged((ITextToSpeechService) this, isConnected);
    }

    private TextToSpeechService()
    {
      LogHelper.Instance.Log("TTS((> Service Initialized.");
      this.m_speechVolume = this.GetTextToSpeechVolume;
      this.m_textReplacement.Add("blu-ray", "blue ray");
      this.m_textReplacement.Add("sec", "second");
      this.m_textReplacement.Add("pg", "p g");
      this.m_textReplacement.Add("pg13", "p g 13");
      this.m_textReplacement.Add("pg-13", "p g 13");
      this.m_textReplacement.Add("esrb", "e s r b");
      this.m_textReplacement.Add("727272", "7 2 7 2 7 2");
      this.m_textReplacement.Add("msg", "message");
      this.m_textReplacement.Add("msgs", "messages");
      this.m_textReplacement.Add("*", "");
      this.m_textReplacement.Add("a-z", "'a' to z");
      this.m_textReplacement.Add("uhd", "u h d");
      this.m_textReplacement.Add("pssst", "");
      this.m_textReplacement.Add("pts", "points");
      this.m_textReplacement.Add("onceonly", "once only");
    }

    private void ProcessSpeechPart(ISpeechPart speechPart)
    {
      if (speechPart == null)
        return;
      this.m_textQueue = new Queue<string>();
      IViewService service = ServiceLocator.Instance.GetService<IViewService>();
      speechPart.EnqueueText(this.m_textQueue, this.m_macrosFound, service);
      if (this.m_textQueue.Count > 0)
        this.SayTTS(this.m_textQueue.Dequeue());
      ISpeechPart regularExpression = speechPart.EvaluateRegularExpression(this.m_macrosFound);
      if (regularExpression == null)
        return;
      this.RunSpeechPartAfterDelay(regularExpression, speechPart.EndPause);
    }

    private void RunSpeechPart(ISpeechPart speechPart)
    {
      if (speechPart == null)
        return;
      if (speechPart.Refresh != null)
      {
        ISpeechPart speechPart1 = speechPart;
        if (speechPart1 != null)
          speechPart1.Refresh();
      }
      int num1 = -1;
      if (this._currentSpeechPart != null && speechPart.SpeechControl == this._currentSpeechPart.SpeechControl)
        num1 = this._currentSpeechPart.Sequence;
      if (num1 == speechPart.Sequence)
        LogHelper.Instance.Log("TTS((> Repeat speech control {0} part #{1} {2}", (object) speechPart?.SpeechControl?.Name, (object) speechPart?.Sequence.ToString(), (object) speechPart?.Name);
      else
        LogHelper.Instance.Log("TTS((> Run speech part {0} #{1} {2}", (object) speechPart?.SpeechControl?.Name, (object) speechPart.Sequence.ToString(), (object) speechPart?.Name);
      this._currentSpeechPart = speechPart;
      this.m_keyHandlers.Clear();
      IMacroService service1 = ServiceLocator.Instance.GetService<IMacroService>();
      this.m_macrosFound = new Dictionary<string, string>();
      speechPart.ValidateMacros(service1, this.m_macrosFound);
      this.ResetTTSEventHandlers(speechPart);
      this.m_keySequenceDelay = this._currentSpeechPart.KeySequenceDelay <= 0 ? 1000 : this._currentSpeechPart.KeySequenceDelay;
      ITimerService service2 = ServiceLocator.Instance.GetService<ITimerService>();
      if (service2 == null)
      {
        LogHelper.Instance.Log("TTS((> no delay TimerService NULL - passing directly to SayTTs");
        this.ProcessSpeechPart(speechPart);
      }
      else
      {
        service2.RemoveTimer("TTSTimer");
        int num2 = speechPart.Loop ? 1 : 0;
        int startPause = speechPart.StartPause;
        TimerCallback callback = (TimerCallback) (o => this.ProcessSpeechPart(speechPart));
        if (startPause == 0)
        {
          LogHelper.Instance.Log("TTS((> no delay startPause = 0 - passing directly to SayTTs");
          this.ProcessSpeechPart(speechPart);
        }
        else
        {
          LogHelper.Instance.Log("TTS((> dueTime = " + startPause.ToString());
          service2.CreateTimer("TTSTimer", new int?(startPause), new int?(), callback).Start();
        }
      }
    }

    private void RunSpeechPartAfterDelay(ISpeechPart speechPart, int delay)
    {
      if (delay > 0)
      {
        ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
        service.RemoveTimer("TTSTimer");
        service.CreateTimer("TTSTimer", new int?(delay), new int?(), (TimerCallback) (_param1 => this.RunSpeechPart(speechPart))).Start();
      }
      else
        this.RunSpeechPart(speechPart);
    }

    public void TTSRepeatSequence()
    {
      if (this._currentSpeechPart == null)
        return;
      this.RunSpeechPart(this._currentSpeechPart);
    }

    private void ResetTTSEventHandlers(ISpeechPart speechPart)
    {
      IInputService service = ServiceLocator.Instance.GetService<IInputService>();
      this.m_keySequence = "";
      service.RemoveKeyPressHandler("TTS-KeyHandler");
      if (speechPart.Events == null)
        return;
      foreach (ISpeechPartEvent speechPartEvent in speechPart.Events)
      {
        ISpeechPartEvent eachSpeechPartEvent = speechPartEvent;
        if (eachSpeechPartEvent is MapKeyPress)
        {
          if (eachSpeechPartEvent.Function != null)
          {
            string function = eachSpeechPartEvent.Function;
            if (function != null)
            {
              switch (function.Length)
              {
                case 9:
                  switch (function[3])
                  {
                    case 'F':
                      if (function == "TTSFaster")
                      {
                        Action action = (Action) (() =>
                        {
                          this.TTSFaster();
                          this.SayTTS("Faster");
                        });
                        this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                        continue;
                      }
                      continue;
                    case 'S':
                      if (function == "TTSSlower")
                      {
                        Action action = (Action) (() =>
                        {
                          this.TTSSlower();
                          this.SayTTS("Slower");
                        });
                        this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                        continue;
                      }
                      continue;
                    default:
                      continue;
                  }
                case 11:
                  if (function == "TTSVolumeUp")
                  {
                    Action action = (Action) (() =>
                    {
                      this.TTSVolumeUp();
                      this.SayTTS(eachSpeechPartEvent.Text != null ? eachSpeechPartEvent.Text : "Volume Up");
                    });
                    this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                    continue;
                  }
                  continue;
                case 13:
                  if (function == "TTSVolumeDown")
                  {
                    Action action = (Action) (() =>
                    {
                      this.TTSVolumeDown();
                      this.SayTTS(eachSpeechPartEvent.Text != null ? eachSpeechPartEvent.Text : "Volume Down");
                    });
                    this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                    continue;
                  }
                  continue;
                case 17:
                  if (function == "TTSNextSpeechPart")
                  {
                    Action action = (Action) (() =>
                    {
                      int sequence = this._currentSpeechPart.Sequence;
                      ISpeechPart speechPart1 = this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == sequence + 1));
                      if (speechPart1 == null)
                        return;
                      ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer("TTSTimer");
                      this.RunSpeechPart(speechPart1);
                    });
                    this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                    continue;
                  }
                  continue;
                case 19:
                  switch (function[3])
                  {
                    case 'N':
                      if (function == "TTSNextAudioSnippet")
                      {
                        Action action = (Action) (() =>
                        {
                          if (this.m_textQueue.Count > 0)
                            this.SayTTS(this.m_textQueue.Dequeue());
                          else
                            this.ReaderNothingLeftToSay();
                        });
                        this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                        continue;
                      }
                      continue;
                    case 'R':
                      if (function == "TTSRepeatSpeechPart")
                      {
                        Action action = (Action) (() =>
                        {
                          int sequence = this._currentSpeechPart.Sequence;
                          this.RunSpeechPart(this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == sequence)));
                        });
                        this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                        continue;
                      }
                      continue;
                    case 'S':
                      if (function == "TTSSkipToSpeechPart")
                      {
                        Action action = (Action) (() =>
                        {
                          if (eachSpeechPartEvent.Value == null)
                            return;
                          int goToSequence = 0;
                          this.RunSpeechPart((int.TryParse(eachSpeechPartEvent.Value, out goToSequence) ? 1 : 0) == 0 ? this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Name == eachSpeechPartEvent.Value)) : this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == goToSequence)));
                        });
                        this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
                        continue;
                      }
                      continue;
                    default:
                      continue;
                  }
                default:
                  continue;
              }
            }
          }
          else if (eachSpeechPartEvent.Text != null)
          {
            Action action = (Action) (() =>
            {
              string speech = eachSpeechPartEvent.Text;
              foreach (KeyValuePair<string, string> keyValuePair in this.m_macrosFound)
                speech = speech.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value);
              this.SayTTS(speech);
            });
            this.m_keyHandlers.Add(eachSpeechPartEvent.KeyCode, (Delegate) action);
          }
        }
        else if (eachSpeechPartEvent is IMapKeyPressToActorHit)
        {
          IMapKeyPressToActorHit mapKeyPressToActorHitEvent = eachSpeechPartEvent as IMapKeyPressToActorHit;
          if (mapKeyPressToActorHitEvent != null && !string.IsNullOrEmpty(mapKeyPressToActorHitEvent.ActorName))
          {
            string actor_name = mapKeyPressToActorHitEvent.ActorName;
            foreach (KeyValuePair<string, string> keyValuePair in this.m_macrosFound)
              actor_name = actor_name.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value);
            Action action = (Action) (() =>
            {
              LogHelper.Instance.Log("TTS((> actor_name:" + actor_name);
              if (!string.IsNullOrEmpty(mapKeyPressToActorHitEvent.Command))
                this.ExecuteActorCommand(actor_name, mapKeyPressToActorHitEvent.Command, mapKeyPressToActorHitEvent.Parameter);
              else
                this.ClickActor(actor_name);
              if (mapKeyPressToActorHitEvent.Function == null)
                return;
              if (mapKeyPressToActorHitEvent.Text != null)
              {
                string speech = mapKeyPressToActorHitEvent.Text;
                LogHelper.Instance.Log("TTS((> Say Action: " + speech);
                foreach (KeyValuePair<string, string> keyValuePair in this.m_macrosFound)
                  speech = speech.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value);
                LogHelper.Instance.Log("TTS((> Say Action: " + speech);
                this.SayTTS(speech);
              }
              if (!(mapKeyPressToActorHitEvent.Function == "TTSSkipToSpeechPart") || mapKeyPressToActorHitEvent.Value == null)
                return;
              int goToSequence = 0;
              if (!int.TryParse(mapKeyPressToActorHitEvent.Value, out goToSequence))
                return;
              this.RunSpeechPart(this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == goToSequence)));
            });
            if (ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame()?.ViewFrame is IViewFrame viewFrame)
            {
              IActor actor = viewFrame.Scene.GetActor(actor_name);
              if (actor != null && (!string.IsNullOrEmpty(mapKeyPressToActorHitEvent.Command) || actor.Visible && actor.Enabled))
                this.m_keyHandlers.Add(mapKeyPressToActorHitEvent.KeyCode, (Delegate) action);
            }
          }
        }
        else if (eachSpeechPartEvent is IMapKeyPressToAction)
        {
          IMapKeyPressToAction mapKeyPressToAction = eachSpeechPartEvent as IMapKeyPressToAction;
          if (mapKeyPressToAction != null)
          {
            Action action = (Action) (() =>
            {
              mapKeyPressToAction.Action();
              if (mapKeyPressToAction.ActionText != null)
                mapKeyPressToAction.Text = mapKeyPressToAction.ActionText();
              if (mapKeyPressToAction.Text != null)
              {
                string speech = mapKeyPressToAction.Text;
                LogHelper.Instance.Log("Say Action: " + speech);
                foreach (KeyValuePair<string, string> keyValuePair in this.m_macrosFound)
                  speech = speech.Replace("{{" + keyValuePair.Key + "}}", keyValuePair.Value);
                LogHelper.Instance.Log("Say Action: " + speech);
                this.SayTTS(speech);
              }
              if (mapKeyPressToAction.Function == "TTSSkipToSpeechPart" && mapKeyPressToAction.Value != null)
              {
                int goToSequence = 0;
                ISpeechPart speechPart2 = !int.TryParse(mapKeyPressToAction.Value, out goToSequence) ? this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Name == mapKeyPressToAction.Value)) : this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == goToSequence));
                if (speechPart2 != null)
                {
                  ISpeechPart speechPart3 = speechPart2;
                  ISpeechPart currentSpeechPart = this._currentSpeechPart;
                  int endPause = currentSpeechPart != null ? currentSpeechPart.EndPause : 0;
                  this.RunSpeechPartAfterDelay(speechPart3, endPause);
                }
              }
              if (!(mapKeyPressToAction.Function == "TTSRepeatSpeechPart"))
                return;
              this.TTSRepeatSequence();
            });
            this.m_keyHandlers.Add(mapKeyPressToAction.KeyCode, (Delegate) action);
          }
        }
      }
      KeyPressHandler handler = (KeyPressHandler) ((key, keyCode, modifier) =>
      {
        LogHelper.Instance.Log("TTS((> Key Sequence, mapping:" + key);
        if (key == "A")
          key = "ENTER";
        if (key == "B")
          key = "CLEAR";
        if (key == "C")
          key = "CANCEL";
        if (key == ".")
          key = "PLUS";
        if (key == "*")
          key = "MINUS";
        if (key == "#")
          key = "TAB";
        if (key == "a")
          key = "1";
        if (key == "b")
          key = "2";
        if (key == "d")
          key = "4";
        if (key == "e")
          key = "5";
        if (key == "f")
          key = "6";
        if (key == "g")
          key = "7";
        if (key == "h")
          key = "8";
        if (key == "i")
          key = "9";
        if (key == "`")
          key = "0";
        if (key == "k")
          key = "PLUS";
        if (key == "m")
          key = "MINUS";
        if (key == "\r")
          key = "ENTER";
        if (key == "c")
          key = "CANCEL";
        if (key == "C")
          key = "CANCEL";
        if (key == "x")
          key = "CANCEL";
        if (key == "X")
          key = "CANCEL";
        if (key == "\b")
          key = "CLEAR";
        if (key == "+")
          key = "PLUS";
        if (key == "-")
          key = "MINUS";
        if (key == "\t")
          key = "TAB";
        if (key == "#")
          key = "TAB";
        LogHelper.Instance.Log("TTS((> Key Sequence, adding:" + key);
        this.m_keySequence += key;
        ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
        if (this.m_keySequenceDelay > 0 && timerService != null)
        {
          timerService.RemoveTimer("TTSInputTimer");
          timerService.CreateTimer("TTSInputTimer", new int?(this.m_keySequenceDelay), new int?(), (TimerCallback) (_param1 =>
          {
            timerService.RemoveTimer("TTSInputTimer");
            this.TTSFinalizeKeyEntry();
          })).Start();
        }
        else
          this.TTSFinalizeKeyEntry();
        var viewFrameInstance = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
        if (viewFrameInstance?.ViewFrame is IViewFrame viewFrame2)
        {
            var actors = viewFrame2.Scene?.Actors;
            if (actors != null)
            {
                foreach (IActor actor in actors)
                {
                    if (actor?.WPFFrameworkElement is ITextToSpeechControl frameworkElement2)
                    {
                        frameworkElement2.HandleWPFHit();
                        break;
                    }
                }
            }
        }
      });
      service.RegisterKeyPressHandler("TTS-KeyHandler", handler);
    }

    private void TTSFinalizeKeyEntry()
    {
      LogHelper.Instance.Log("TTS((> Final Key Sequence: " + this.m_keySequence);
      if (this.m_keyHandlers.ContainsKey(this.m_keySequence))
      {
        Delegate keyHandler = this.m_keyHandlers[this.m_keySequence];
        this.m_keySequence = "";
        if ((object) keyHandler != null)
          keyHandler.DynamicInvoke();
        else
          this.SayTTS("Sorry, there was a problem.  Please make your selection again.");
      }
      else
        this.SayTTS("Sorry, there was a problem.  Please make your selection again.");
      this.m_keySequence = "";
    }

    private SpeechControl LoadSpeechControlFromResouce(string resourceName)
    {
      SpeechControl speechControl = (SpeechControl) null;
      LogHelper.Instance.Log("TTS((> Attempting to load XML Control markup for " + resourceName);
      IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
      if (service != null)
      {
        XmlNode xml = service.GetXml(resourceName);
        if (xml != null)
        {
          speechControl = new SpeechControl()
          {
            Name = resourceName
          };
          XmlNodeList xmlNodeList = xml.SelectNodes("speechPart");
          if (xmlNodeList != null)
          {
            foreach (XmlNode xmlNode in xmlNodeList)
            {
              int attributeValue1 = xmlNode.GetAttributeValue<int>("keySequenceDelay", 0);
              int attributeValue2 = xmlNode.GetAttributeValue<int>("sequence", 0);
              string attributeValue3 = xmlNode.GetAttributeValue<string>("language", "en-US");
              bool attributeValue4 = xmlNode.GetAttributeValue<bool>("loop", false);
              bool attributeValue5 = xmlNode.GetAttributeValue<bool>("auto_run", true);
              int attributeValue6 = xmlNode.GetAttributeValue<int>("startPause", 0);
              int attributeValue7 = xmlNode.GetAttributeValue<int>("endPause", 0);
              SpeechPart speechPart = new SpeechPart()
              {
                SpeechControl = (ISpeechControl) speechControl,
                KeySequenceDelay = attributeValue1,
                Sequence = attributeValue2,
                Language = attributeValue3,
                Loop = attributeValue4,
                StartPause = attributeValue6,
                EndPause = attributeValue7,
                AutoRun = attributeValue5
              };
              TextToSpeechService.ReadNeededMacros(xmlNode, speechPart);
              TextToSpeechService.ReadRegularExpressions(xmlNode, speechPart);
              TextToSpeechService.ReadText(xmlNode, speechPart);
              TextToSpeechService.ReadEvents(xmlNode, speechPart);
              speechControl.SpeechParts.Add((ISpeechPart) speechPart);
            }
          }
          LogHelper.Instance.Log("TTS((> Loaded XML Control Markup for {0}. {1} speech parts into memory.", (object) resourceName, (object) speechControl.SpeechParts.Count.ToString());
        }
      }
      return speechControl;
    }

    private static void ReadNeededMacros(XmlNode part, SpeechPart speechPart)
    {
      XmlNode xmlNode = part.SelectSingleNode("macrosNeeded");
      if (xmlNode == null)
        return;
      XmlNodeList xmlNodeList = xmlNode.SelectNodes("value");
      if (xmlNodeList == null)
        return;
      foreach (XmlNode node in xmlNodeList)
      {
        string attributeValue = node.GetAttributeValue<string>("default");
        NeededMacro neededMacro = new NeededMacro()
        {
          Name = node.InnerText,
          Default = (object) attributeValue
        };
        speechPart.NeededMacros.Add((INeededMacro) neededMacro);
      }
    }

    private static void ReadRegularExpressions(XmlNode part, SpeechPart speechPart)
    {
      XmlNode xmlNode1 = part.SelectSingleNode("regex");
      if (xmlNode1 == null)
        return;
      XmlNode xmlNode2 = xmlNode1.SelectSingleNode("value");
      XmlNode xmlNode3 = xmlNode1.SelectSingleNode("expression");
      XmlNode xmlNode4 = xmlNode1.SelectSingleNode("success");
      XmlNode xmlNode5 = xmlNode1.SelectSingleNode("failure");
      if (xmlNode2 == null || xmlNode3 == null || xmlNode4 == null || xmlNode5 == null)
        return;
      speechPart.RegularExpression = (IRegularExpression) new RegularExpression()
      {
        Expression = xmlNode3.InnerText,
        Value = xmlNode2.InnerText,
        Success = xmlNode4.InnerText,
        Failure = xmlNode5.InnerText
      };
    }

    private static void ReadText(XmlNode part, SpeechPart speechPart)
    {
      XmlNodeList xmlNodeList = part.SelectNodes("text");
      if (xmlNodeList == null)
        return;
      foreach (XmlNode node in xmlNodeList)
      {
        string attributeValue1 = node.GetAttributeValue<string>("textAvailable");
        string attributeValue2 = node.GetAttributeValue<string>("ifActorVisible");
        string attributeValue3 = node.GetAttributeValue<string>("ifControlEnabled");
        string innerText = node.InnerText;
        speechPart.Texts.Add((ITextPart) new TextPart()
        {
          TextAvailable = attributeValue1,
          IfActorVisible = attributeValue2,
          IfControlEnabled = attributeValue3,
          Text = innerText
        });
      }
    }

    private static void ReadEvents(XmlNode part, SpeechPart speechPart)
    {
      XmlNode xmlNode1 = part.SelectSingleNode("events");
      if (xmlNode1 == null)
        return;
      foreach (XmlNode node in xmlNode1)
      {
        SpeechPartEvent speechPartEvent = (SpeechPartEvent) null;
        string attributeValue = node.GetAttributeValue<string>("keyCode");
        XmlNode xmlNode2 = node.SelectSingleNode("function");
        XmlNode xmlNode3 = node.SelectSingleNode("text");
        XmlNode xmlNode4 = node.SelectSingleNode("value");
        switch (node.Name)
        {
          case "mapKeyPress":
            MapKeyPress mapKeyPress = new MapKeyPress();
            mapKeyPress.KeyCode = attributeValue;
            mapKeyPress.Function = xmlNode2?.InnerText;
            mapKeyPress.Value = xmlNode4?.InnerText;
            mapKeyPress.Text = xmlNode3?.InnerText;
            speechPartEvent = (SpeechPartEvent) mapKeyPress;
            break;
          case "mapKeyPressToActorHit":
            XmlNode xmlNode5 = node.SelectSingleNode("actorName");
            XmlNode xmlNode6 = node.SelectSingleNode("command");
            XmlNode xmlNode7 = node.SelectSingleNode("parameter");
            MapKeyPressToActorHit keyPressToActorHit = new MapKeyPressToActorHit();
            keyPressToActorHit.KeyCode = attributeValue;
            keyPressToActorHit.Function = xmlNode2?.InnerText;
            keyPressToActorHit.Value = xmlNode4?.InnerText;
            keyPressToActorHit.Text = xmlNode3?.InnerText;
            keyPressToActorHit.ActorName = xmlNode5?.InnerText;
            keyPressToActorHit.Command = xmlNode6?.InnerText;
            keyPressToActorHit.Parameter = xmlNode7?.InnerText;
            speechPartEvent = (SpeechPartEvent) keyPressToActorHit;
            break;
        }
        speechPart.Events.Add((ISpeechPartEvent) speechPartEvent);
      }
    }

    private void TTSVolumeUp()
    {
      this.m_speechVolume += 10;
      if (this.m_speechVolume <= 100)
        return;
      this.m_speechVolume = 100;
    }

    private void TTSVolumeDown()
    {
      this.m_speechVolume -= 10;
      if (this.m_speechVolume >= 0)
        return;
      this.m_speechVolume = 0;
    }

    private void TTSFaster()
    {
      ++this.m_speechRate;
      if (this.m_speechRate <= 10)
        return;
      this.m_speechRate = 10;
    }

    private void TTSSlower()
    {
      --this.m_speechRate;
      if (this.m_speechRate >= -10)
        return;
      this.m_speechRate = -10;
    }

    private string ReplaceCommonText(string input)
    {
      if (string.IsNullOrEmpty(input) || this.m_textReplacement.Count <= 0)
        return input;
      this.m_textReplacement.ForEach<KeyValuePair<string, string>>((Action<KeyValuePair<string, string>>) (x => input = input.ToLowerInvariant().Replace(x.Key, x.Value)));
      return input;
    }

    private void SayTTS(string speech)
    {
      this.SilenceTTS();
      speech = this.ReplaceCommonText(speech);
      ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
      {
        try
        {
          lock (this.m_speechSynthesizerLock)
          {
            if (string.IsNullOrEmpty(speech) || !this.AudioDeviceConnected)
              return;
            this.m_reader = new SpeechSynthesizer()
            {
              Rate = this.m_speechRate,
              Volume = this.m_speechVolume
            };
            LogHelper.Instance.Log("TTS((> Say: " + speech);
            this.m_reader.SpeakAsync(speech);
            this.m_reader.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(this.ReaderSpeakCompleted);
          }
        }
        catch (Exception ex)
        {
          LogHelper.Instance.Log("TTS((> An exception was raised in TextToSpeechService:SayTTS()", ex);
        }
      }));
    }

    private void SilenceTTS()
    {
      lock (this.m_speechSynthesizerLock)
      {
        if (this.m_reader == null)
          return;
        this.m_reader.SpeakAsyncCancelAll();
        this.m_reader?.Dispose();
        this.m_reader = (SpeechSynthesizer) null;
      }
    }

    private void RunNextSpeechPart()
    {
      if (this._currentSpeechPart == null)
        return;
      ISpeechPart speechPart = this._currentSpeechPart;
      if (!this._currentSpeechPart.Loop && this._speechControl.SpeechParts.Count > 1)
      {
        int num = this._speechControl.SpeechParts.Max<ISpeechPart>((Func<ISpeechPart, int>) (x => x.Sequence));
        int minSequence = this._speechControl.SpeechParts.Min<ISpeechPart>((Func<ISpeechPart, int>) (x => x.Sequence));
        if (this._currentSpeechPart.Sequence == num)
        {
          speechPart = this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == minSequence));
        }
        else
        {
          List<ISpeechPart> list = this._speechControl.SpeechParts.Where<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence > this._currentSpeechPart.Sequence)).ToList<ISpeechPart>();
          if (list.Count > 0)
          {
            int nextSpeechSequence = list.Min<ISpeechPart>((Func<ISpeechPart, int>) (x => x.Sequence));
            speechPart = this._speechControl.SpeechParts.FirstOrDefault<ISpeechPart>((Func<ISpeechPart, bool>) (x => x.Sequence == nextSpeechSequence));
          }
        }
      }
      this.RunSpeechPart(speechPart);
    }

    private void ReaderNothingLeftToSay()
    {
      LogHelper.Instance.Log("TTS((> Reader speak completed speech part #" + this._currentSpeechPart.Sequence.ToString());
      ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
      if (service == null)
        return;
      service.RemoveTimer("TTSTimer");
      if (this._currentSpeechPart == null)
        return;
      int endPause = this._currentSpeechPart.EndPause;
      if (endPause > 0)
        service.CreateTimer("TTSTimer", new int?(endPause), new int?(), (TimerCallback) (_param1 => this.RunNextSpeechPart())).Start();
      else
        this.RunNextSpeechPart();
    }

    private void ReaderSpeakCompleted(object sender, SpeakCompletedEventArgs e)
    {
      try
      {
        if (this.m_textQueue.Count > 0)
          this.SayTTS(this.m_textQueue.Dequeue());
        else
          this.ReaderNothingLeftToSay();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("TTS((> An exception was raised in TextToSpeechService:SayTTS()", ex);
      }
    }

    private void ExecuteActorCommand(string actorName, string command, string parameter)
    {
      LogHelper.Instance.Log("TTS((> Execute Command requested: {0}, {1}, {2}", (object) actorName, (object) command, (object) parameter);
      IActor actor = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame()?.ViewFrame is IViewFrame viewFrame ? viewFrame.Scene?.GetActor(actorName) : (IActor) null;
      if (actor != null)
      {
        if (actor.WPFFrameworkElement != null)
        {
          if (actor.WPFFrameworkElement is ITextToSpeechControl frameworkElement)
          {
            LogHelper.Instance.Log("TTS((> Execute command: {0} parameter: {1}", (object) command, (object) parameter);
            frameworkElement.ExecuteCommand(command, parameter);
          }
          else
            LogHelper.Instance.Log("TTS((> actor WPFFrameworkElement is not a ITextToSpeechControl.");
        }
        else
          LogHelper.Instance.Log("TTS((> actor does not have a WPFFrameworkElement.");
      }
      else
        LogHelper.Instance.Log("TTS((> actor NOT FOUND in scene.");
    }

    private void ClickActor(string actorName)
    {
      LogHelper.Instance.Log("TTS((> Click Actor requested: " + actorName);
      try
      {
        IActor actor = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame()?.ViewFrame is IViewFrame viewFrame ? viewFrame.Scene?.GetActor(actorName) : (IActor) null;
        if (actor == null)
          LogHelper.Instance.Log("TTS((> actor NOT FOUND in scene.");
        else if (!actor.Visible)
          LogHelper.Instance.Log("TTS((> actor is NOT VISIBLE.");
        else if (!actor.Enabled)
        {
          LogHelper.Instance.Log("TTS((> actor is NOT ENABLED.");
        }
        else
        {
          int window = TextToSpeechService.FindWindow((string) null, "Redbox Kiosk Engine");
          if (window <= 0)
            return;
          TextToSpeechService.RECT lprect = new TextToSpeechService.RECT();
          TextToSpeechService.GetWindowRect(window, out lprect);
          int x = lprect.X + actor.X + (int) Math.Floor((double) (actor.Width / 2));
          int y = lprect.Y + actor.Y + (int) Math.Floor((double) (actor.Height / 2));
          if (Debugger.IsAttached)
            y += 25;
          Cursor.Position = new Point(x, y);
          Thread.Sleep(10);
          this.MouseClick(x, y);
        }
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("TTS((> ClickActor: exception - " + ex.Message);
      }
    }

    [DllImport("user32.dll")]
    private static extern void mouse_event(
      int dwFlags,
      int dx,
      int dy,
      int cButtons,
      int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string strClassName, int nptWindowName);

    [DllImport("user32.dll")]
    private static extern int FindWindow(string ClassName, string WindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(int hWnd, out TextToSpeechService.RECT lprect);

    private void MouseClick(int x, int y)
    {
      TextToSpeechService.mouse_event(2, x, y, 0, 0);
      TextToSpeechService.mouse_event(4, x, y, 0, 0);
    }

    public struct RECT
    {
      public int X;
      public int Y;
      public int Right;
      public int Bottom;

      public int Width => this.Right - this.X;

      public int Height => this.Bottom - this.Y;
    }
  }
}
