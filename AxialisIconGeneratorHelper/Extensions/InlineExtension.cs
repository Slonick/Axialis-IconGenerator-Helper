#region Usings

using System.Windows.Documents;

#endregion

namespace AxialisIconGeneratorHelper.Extensions
{
    public static class InlineExtension
    {
        #region Public Methods

        public static void AddLine(this InlineCollection inlines, string text)
        {
            inlines.Add(new Run(text));
            inlines.Add(new LineBreak());
        }

        #endregion
    }
}