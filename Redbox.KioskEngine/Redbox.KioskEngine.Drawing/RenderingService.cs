using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.Integration;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Drawing
{
    public class RenderingService : IRenderingService
    {
        private IScene m_activeScene;
        private IDictionary<string, Scene> m_scenes;

        private RenderingService()
        {
        }

        public static RenderingService Instance => Singleton<RenderingService>.Instance;

        internal IDictionary<string, Scene> Scenes
        {
            get
            {
                if (m_scenes == null)
                    m_scenes = new Dictionary<string, Scene>();
                return m_scenes;
            }
        }

        public IScene CreateScene(
            string name,
            int width,
            int height,
            Color backgroundColor,
            ElementHost elementHost)
        {
            if (Scenes.ContainsKey(name))
                return Scenes[name];
            var scene = new Scene(name, width, height, backgroundColor, elementHost);
            Scenes[name] = scene;
            return scene;
        }

        public IScene GetScene(string name)
        {
            return !Scenes.ContainsKey(name) ? null : (IScene)Scenes[name];
        }

        public void RemoveScene(string name)
        {
            var scene = GetScene(name);
            if (scene == null)
                return;
            Scenes.Remove(name);
            scene.Dispose();
        }

        public bool IsBitmapReferenced(Image image)
        {
            foreach (var scene in Scenes)
            {
                if (scene.Value.BackgroundImage == image)
                    return true;
                foreach (var actor in scene.Value.Actors)
                    if (actor.Image == image)
                        return true;
            }

            return false;
        }

        public IScene ActiveScene
        {
            get => m_activeScene;
            set
            {
                if (m_activeScene == value)
                    return;
                m_activeScene = value;
                m_activeScene.MakeDirty(null);
            }
        }

        public Color BackgroundColor
        {
            get
            {
                var service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
                return service == null
                    ? Color.FromArgb(153, 153, 152)
                    : (Color)ConversionHelper.ChangeType(
                        service.GetValue("Core", nameof(BackgroundColor), "153,153,152"), typeof(Color));
            }
            set => ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core",
                nameof(BackgroundColor), ConversionHelper.ChangeType(value, typeof(string)));
        }

        public ErrorList Initialize()
        {
            LogHelper.Instance.Log("Initialize rendering service.");
            var errorList = new ErrorList();
            ServiceLocator.Instance.AddService(typeof(IRenderingService), Instance);
            return errorList;
        }
    }
}