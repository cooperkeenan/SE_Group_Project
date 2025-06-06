﻿// App.xaml.cs
using System;
using System.Diagnostics;           // ← add this
using EnviroMonitorApp.Views;
using Microsoft.Maui.Controls;

namespace EnviroMonitorApp
{
    /// <summary>
    /// Main application class that represents the EnviroMonitorApp application.
    /// Configures the application shell and initial page.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the App class.
        /// Sets up the application shell using dependency injection.
        /// </summary>
        /// <param name="shell">The shell to use as the main page, injected by the DI container.</param>
        public App(AppShell shell)
        {
            InitializeComponent();

            try
            {
                MainPage = shell;
                Debug.WriteLine("[App] MainPage set to AppShell");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App] 🔥 Exception in ctor: {ex}");
                throw;
            }
        }
    }
}
