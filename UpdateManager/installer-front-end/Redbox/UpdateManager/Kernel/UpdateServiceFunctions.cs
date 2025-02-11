using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Redbox.UpdateManager.Kernel
{
    internal static class UpdateServiceFunctions
    {
        [KernelFunction(Name = "kernel.uploadfile")]
        internal static LuaTable UploadFile(string file)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().UploadFile(file);
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e.ToString(), (object)e.Details)));
            return UpdateServiceFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.pollupdateservice")]
        internal static LuaTable PollUpdateService()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().Poll();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e.ToString(), (object)e.Details)));
            return UpdateServiceFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.serverpoll")]
        internal static LuaTable ServerPoll()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().ServerPoll();
            errors.ToLogHelper();
            return UpdateServiceFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.startupdate")]
        internal static LuaTable StartUpate()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().StartDownloads();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e.ToString(), (object)e.Details)));
            return UpdateServiceFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.finishupdate")]
        internal static LuaTable FinishUpdate(string jobId)
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().FinishDownload(new Guid(jobId));
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e.ToString(), (object)e.Details)));
            return UpdateServiceFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.finishallupdates")]
        internal static LuaTable FinishUpdates()
        {
            ErrorList errors = ServiceLocator.Instance.GetService<IUpdateService>().FinishDownloads();
            if (errors.ContainsError())
                errors.ForEach((Action<Redbox.UpdateManager.ComponentModel.Error>)(e => LogHelper.Instance.Log("{0} Details: {1}", (object)e.ToString(), (object)e.Details)));
            return UpdateServiceFunctions.CreateResultTable(errors);
        }

        [KernelFunction(Name = "kernel.dowork")]
        internal static LuaTable DoWork(string filter)
        {
            return new LuaTable(KernelService.Instance.LuaRuntime);
        }

        [KernelFunction(Name = "kernel.updateaddstoretorepository")]
        internal static LuaTable AddStoreToRepository(string store, string name)
        {
            return UpdateServiceFunctions.CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().AddStoreToRepository(store, name));
        }

        [KernelFunction(Name = "kernel.updateaddgrouptorepository")]
        internal static LuaTable AddGroupToRepository(string group, string name)
        {
            return UpdateServiceFunctions.CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().AddGroupToRepository(group, name));
        }

        [KernelFunction(Name = "kernel.updateremovestorefromrepository")]
        internal static LuaTable RemoveStoreFromRepository(string number, string name)
        {
            return UpdateServiceFunctions.CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().RemoveStoreFromRepository(number, name));
        }

        [KernelFunction(Name = "kernel.updateremovegroupfromrepository")]
        internal static LuaTable RemoveGroupFromRepository(string group, string name)
        {
            return UpdateServiceFunctions.CreateResultTable(ServiceLocator.Instance.GetService<IUpdateService>().RemoveGroupFromRepository(group, name));
        }

        [KernelFunction(Name = "kernel.startinstaller")]
        internal static LuaTable StartInstaller(string repositoryHash, string frontEndVersion)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            string repositoryHash1 = repositoryHash;
            string frontEndVersion1 = frontEndVersion;
            Dictionary<string, string> dictionary;
            ErrorList errors = service.StartInstaller(repositoryHash1, frontEndVersion1, out dictionary);
            if (errors.ContainsError())
            {
                UpdateServiceFunctions.LogErrors((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errors);
                luaTable = UpdateServiceFunctions.CreateResultTable(errors);
                luaTable[(object)"success"] = (object)bool.FalseString;
            }
            else
            {
                luaTable[(object)"success"] = (object)bool.TrueString;
                foreach (KeyValuePair<string, string> keyValuePair in dictionary)
                    luaTable[(object)keyValuePair.Key] = (object)keyValuePair.Value;
            }
            return luaTable;
        }

        [KernelFunction(Name = "kernel.finishinstaller")]
        internal static bool FinishInstaller(string guid, LuaTable data)
        {
            IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
            string str1 = data.Keys.Cast<object>().ToDictionary<object, string, string>((Func<object, string>)(k => k as string), (Func<object, string>)(v => data[v] as string)).ToJson();
            if (str1.Length > 252)
                str1 = string.Empty;
            string guid1 = guid;
            string str2 = str1;
            ErrorList errorList = service.FinishInstaller(guid1, "FinishInstallerData", str2);
            if (!errorList.ContainsError())
                return true;
            UpdateServiceFunctions.LogErrors((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList);
            return false;
        }

        [KernelFunction(Name = "kernel.sendstatusmessage")]
        internal static bool StatusMessage(
          string type,
          string key,
          string subKey,
          string description,
          string data,
          bool encode)
        {
            ErrorList errorList = new ErrorList();
            Redbox.UpdateService.Model.StatusMessage.StatusMessageType result;
            if (!Extensions.TryParse<Redbox.UpdateService.Model.StatusMessage.StatusMessageType>(type, out result, true))
            {
                result = Redbox.UpdateService.Model.StatusMessage.StatusMessageType.Info;
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("SSM01", "The type " + type + " is unparsable", "Defaulting to 'info'"));
                UpdateServiceFunctions.LogErrors((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList);
                errorList.Clear();
            }
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StatusMessageService.Instance.EnqueueMessage(result, key, subKey, description, data, encode));
            if (!errorList.ContainsError())
                return true;
            UpdateServiceFunctions.LogErrors((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)errorList);
            return false;
        }

        [KernelFunction(Name = "kernel.sendscreenshot")]
        internal static bool SendScreenShot(double size)
        {
            try
            {
                Bitmap screenshot = UpdateServiceFunctions.TakeScreenshot();
                string data = UpdateServiceFunctions.BitmapToString((Image)UpdateServiceFunctions.ResizeBitmap((Image)screenshot, (int)Math.Round((double)screenshot.Width * size), (int)Math.Round((double)screenshot.Height * size)));
                return UpdateServiceFunctions.StatusMessage("Info", "screenshot", string.Empty, "A kiosk screenshot", data, false);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in kernel.sendscreenshot", ex);
                return false;
            }
        }

        private static LuaTable CreateResultTable(ErrorList errors)
        {
            LuaTable resultTable = new LuaTable(KernelService.Instance.LuaRuntime);
            resultTable[(object)"success"] = (object)!errors.ContainsError();
            LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
            foreach (Redbox.UpdateManager.ComponentModel.Error error in (List<Redbox.UpdateManager.ComponentModel.Error>)errors)
                luaTable[(object)error.Code] = (object)error.Description;
            resultTable[(object)nameof(errors)] = (object)luaTable;
            return resultTable;
        }

        private static void LogErrors(IEnumerable<Redbox.UpdateManager.ComponentModel.Error> errors)
        {
            foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
                LogHelper.Instance.Log(error.Description, LogEntryType.Debug);
        }

        private static string BitmapToString(Image bitmap)
        {
            try
            {
                byte[] array;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save((Stream)memoryStream, ImageFormat.Jpeg);
                    array = memoryStream.ToArray();
                }
                return Convert.ToBase64String(array);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in BitmapToString", ex);
                return (string)null;
            }
        }

        private static Bitmap ResizeBitmap(Image bitmap, int width, int height)
        {
            try
            {
                Bitmap bitmap1 = new Bitmap(width, height);
                using (Graphics graphics = Graphics.FromImage((Image)bitmap1))
                {
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(bitmap, 0, 0, width, height);
                }
                return bitmap1;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in ResizeBitmap", ex);
                return (Bitmap)null;
            }
        }

        private static Bitmap TakeScreenshot()
        {
            try
            {
                Rectangle bounds1 = Screen.PrimaryScreen.Bounds;
                int width = bounds1.Width;
                bounds1 = Screen.PrimaryScreen.Bounds;
                int height = bounds1.Height;
                Bitmap screenshot = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (Graphics graphics1 = Graphics.FromImage((Image)screenshot))
                {
                    Graphics graphics2 = graphics1;
                    Rectangle bounds2 = Screen.PrimaryScreen.Bounds;
                    int x = bounds2.X;
                    bounds2 = Screen.PrimaryScreen.Bounds;
                    int y = bounds2.Y;
                    Size size = Screen.PrimaryScreen.Bounds.Size;
                    graphics2.CopyFromScreen(x, y, 0, 0, size, CopyPixelOperation.SourceCopy);
                }
                return screenshot;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in TakeScreenshot", ex);
                return (Bitmap)null;
            }
        }
    }
}
