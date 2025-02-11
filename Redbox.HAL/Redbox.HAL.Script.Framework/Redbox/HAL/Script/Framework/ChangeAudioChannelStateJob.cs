using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "change-audio-channel-state", Operand = "CHANGE-AUDIO-CHANNEL-STATE")]
    internal sealed class ChangeAudioChannelStateJob : NativeJobAdapter
    {
        internal ChangeAudioChannelStateJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            var newState = Context.PopTop<AudioChannelState>();
            if (ControlSystem.SetAudio(newState).Success)
            {
                Context.CreateInfoResult("AudioChangeSuccessful",
                    string.Format("Changed audio channel state to {0}", newState.ToString().ToUpper()));
            }
            else
            {
                Context.CreateInfoResult("AudioChangeFailure", "Failed to change channel state.");
                AddError("STATE CHANGE FAILURE");
            }
        }
    }
}