﻿using System;

using Duality.IO;

namespace Duality.Backend.Android
{
    public class SystemBackend : ISystemBackend
    {
        private NativeFileSystem fileSystem = new NativeFileSystem();

        string IDualityBackend.Id
        {
            get { return "AndroidSystemBackend"; }
        }
        string IDualityBackend.Name
        {
            get { return "Android"; }
        }
        int IDualityBackend.Priority
        {
            get { return 0; }
        }

        IFileSystem ISystemBackend.FileSystem
        {
            get { return this.fileSystem; }
        }

        bool IDualityBackend.CheckAvailable()
        {
            return true;
        }
        void IDualityBackend.Init() { }
        void IDualityBackend.Shutdown() { }

        string ISystemBackend.GetNamedPath(NamedDirectory dir)
        {
            string path;
            switch (dir) {
                default: path = null; break;
                case NamedDirectory.Current: path = System.IO.Directory.GetCurrentDirectory(); break;
                case NamedDirectory.ApplicationData: path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); break;
                case NamedDirectory.MyDocuments: path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); break;
                case NamedDirectory.MyMusic: path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); break;
                case NamedDirectory.MyPictures: path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); break;
                // Not supported on Android
                case NamedDirectory.SavedGames: path = null; break;
            }

            if (path == null) {
                return null;
            }

            return this.fileSystem.GetDualityPathFormat(path);
        }
    }
}