﻿using Android.App;

namespace Jazz2.Game
{
    public partial class App
    {
        public static string AssemblyTitle
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).ApplicationInfo.Name;
            }
        }

        public static string AssemblyVersion
        {
            get
            {
                return Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0).VersionName;
            }
        }
    }
}