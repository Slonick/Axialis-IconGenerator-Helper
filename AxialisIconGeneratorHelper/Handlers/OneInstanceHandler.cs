#region Usings

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using AxialisIconGeneratorHelper.Extensions;

#endregion

namespace AxialisIconGeneratorHelper.Handlers
{
    public class OneInstanceHandler
    {
        #region Private Fields

        private static OneInstanceHandler current;
        private readonly Mutex mutex = new Mutex(true, Assembly.GetExecutingAssembly().GetAssemblyAttribute<GuidAttribute>().Value);

        #endregion

        #region Public Properties

        public static OneInstanceHandler Current
        {
            get => current ?? (current = new OneInstanceHandler());
        }

        #endregion

        #region Not Static Constructors

        private OneInstanceHandler() { }

        #endregion

        #region Public Methods

        public void RegisterHandler()
        {
            if (!this.mutex.WaitOne(TimeSpan.Zero, true)) Environment.Exit(0);
            Application.Current.Exit += (sender, args) => this.mutex.ReleaseMutex();
        }

        #endregion
    }
}