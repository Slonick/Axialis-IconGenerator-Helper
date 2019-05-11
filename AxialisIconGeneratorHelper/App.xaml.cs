#region Usings

using AxialisIconGeneratorHelper.Handlers;

#endregion

namespace AxialisIconGeneratorHelper
{
    public partial class App
    {
        #region Not Static Constructors

        public App()
        {
            OneInstanceHandler.Current.RegisterHandler();
        }

        #endregion
    }
}