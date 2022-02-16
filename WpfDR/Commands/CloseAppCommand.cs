using System.Windows;
using WpfDR.Commands.Base;

namespace WpfDR.Commands
{
    public class CloseAppCommand : Command
    {
        public override void Execute(object parameter) => Application.Current.Shutdown();
    }
}
